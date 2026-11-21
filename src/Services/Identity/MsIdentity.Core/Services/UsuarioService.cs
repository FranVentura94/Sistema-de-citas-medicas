using System.Text.RegularExpressions;
using MsIdentity.Core.Dtos;
using MsIdentity.Core.Entities;
using MsIdentity.Core.Exceptions;
using MsIdentity.Core.Interfaces;

namespace MsIdentity.Core.Services;

public interface IUsuarioService
{
    Task<List<UsuarioDto>> GetAllAsync(CancellationToken ct);
    Task<List<RolDto>> GetRolesAsync(CancellationToken ct);
    Task<UsuarioDto> CrearAsync(CrearUsuarioRequest request, CancellationToken ct);
    Task<UsuarioDto> ActualizarAsync(int usuarioId, ActualizarUsuarioRequest request, int usuarioActualId, CancellationToken ct);
    Task<PasswordTemporalDto> GenerarPasswordTemporalAsync(int usuarioId, int usuarioActualId, CancellationToken ct);
}

public class UsuarioService : IUsuarioService
{
    private const string ROL_MEDICO = "Medico";
    private const string ROL_ADMIN = "Administrador";

    private readonly IUsuarioAdminRepository _repo;

    public UsuarioService(IUsuarioAdminRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<UsuarioDto>> GetAllAsync(CancellationToken ct)
    {
        var usuarios = await _repo.GetAllAsync(ct);
        return usuarios.Select(ToDto).ToList();
    }

    public async Task<List<RolDto>> GetRolesAsync(CancellationToken ct)
    {
        var roles = await _repo.GetRolesAsync(ct);
        return roles.Select(r => new RolDto(r.RolId, r.Nombre, r.Descripcion)).ToList();
    }

    // Crea el acceso SIN contraseña: después el Administrador genera una temporal
    public async Task<UsuarioDto> CrearAsync(CrearUsuarioRequest r, CancellationToken ct)
    {
        var nombreUsuario = (r.NombreUsuario ?? string.Empty).Trim().ToLowerInvariant();

        if (!Regex.IsMatch(nombreUsuario, "^[a-z0-9._-]{4,30}$"))
            throw new DomainException(Errores.DATOS_INCOMPLETOS,
                "El usuario debe tener de 4 a 30 caracteres: letras, números, punto, guion o guion bajo, sin espacios.");

        var email = ValidarDatos(r.NombreCompleto, r.Email);

        if (await _repo.ExisteNombreUsuarioAsync(nombreUsuario, ct))
            throw new DomainException("REGISTRO_DUPLICADO", $"Ya existe el usuario '{nombreUsuario}'.", 409);

        var rol = await ValidarRolAsync(r.RolId, r.MedicoId, null, ct);

        var usuario = new Usuario
        {
            NombreUsuario = nombreUsuario,
            NombreCompleto = r.NombreCompleto.Trim(),
            Email = email,
            PasswordHash = string.Empty,       // sin contraseña hasta que se genere la temporal
            DebeCambiarPassword = true,
            RolId = rol.RolId,
            MedicoId = rol.Nombre == ROL_MEDICO ? r.MedicoId : null,
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        await _repo.AddAsync(usuario, ct);
        usuario.Rol = rol;
        return ToDto(usuario);
    }

    public async Task<UsuarioDto> ActualizarAsync(int usuarioId, ActualizarUsuarioRequest r, int usuarioActualId, CancellationToken ct)
    {
        var usuario = await _repo.GetByIdAsync(usuarioId, ct)
            ?? throw new DomainException("NO_ENCONTRADO", "El usuario no existe.", 404);

        var email = ValidarDatos(r.NombreCompleto, r.Email);
        var rol = await ValidarRolAsync(r.RolId, r.MedicoId, usuarioId, ct);

        // El administrador no puede quitarse a sí mismo el acceso
        if (usuarioId == usuarioActualId && (!r.Activo || rol.Nombre != ROL_ADMIN))
            throw new DomainException(Errores.DATOS_INCOMPLETOS,
                "No puedes quitarte el acceso ni el nivel de Administrador a ti mismo.");

        // Siempre debe quedar al menos un administrador activo
        var eraAdminActivo = usuario.Rol?.Nombre == ROL_ADMIN && usuario.Activo;
        var seguiraAdminActivo = rol.Nombre == ROL_ADMIN && r.Activo;
        if (eraAdminActivo && !seguiraAdminActivo && await _repo.ContarAdministradoresActivosAsync(ct) <= 1)
            throw new DomainException(Errores.DATOS_INCOMPLETOS, "Debe quedar al menos un Administrador activo.");

        usuario.NombreCompleto = r.NombreCompleto.Trim();
        usuario.Email = email;
        usuario.RolId = rol.RolId;
        usuario.Rol = rol;
        usuario.MedicoId = rol.Nombre == ROL_MEDICO ? r.MedicoId : null;
        usuario.Activo = r.Activo;

        await _repo.GuardarCambiosAsync(ct);
        return ToDto(usuario);
    }

    // Genera una contraseña temporal: se muestra UNA vez y la persona la cambia al entrar
    public async Task<PasswordTemporalDto> GenerarPasswordTemporalAsync(int usuarioId, int usuarioActualId, CancellationToken ct)
    {
        var usuario = await _repo.GetByIdAsync(usuarioId, ct)
            ?? throw new DomainException("NO_ENCONTRADO", "El usuario no existe.", 404);

        if (usuarioId == usuarioActualId)
            throw new DomainException(Errores.DATOS_INCOMPLETOS, "No puedes generarte una contraseña temporal a ti mismo.");

        if (!usuario.Activo)
            throw new DomainException(Errores.DATOS_INCOMPLETOS, "Primero activa el acceso al portal de esta persona.");

        var temporal = ReglasPassword.GenerarTemporal();
        var vence = DateTime.Now.AddHours(ReglasPassword.HORAS_VIGENCIA_TEMPORAL);

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(temporal);
        usuario.DebeCambiarPassword = true;
        usuario.PasswordTemporalVence = vence;

        await _repo.GuardarCambiosAsync(ct);
        return new PasswordTemporalDto(temporal, vence);
    }

    // ---------- Validaciones ----------
    // Devuelve el correo limpio (puede quedar vacío: no es obligatorio)
    private static string ValidarDatos(string nombreCompleto, string? email)
    {
        if (string.IsNullOrWhiteSpace(nombreCompleto))
            throw new DomainException(Errores.DATOS_INCOMPLETOS, "El nombre completo es obligatorio.");

        var limpio = email?.Trim() ?? string.Empty;
        if (limpio.Length > 0 && !Regex.IsMatch(limpio, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new DomainException(Errores.DATOS_INCOMPLETOS, "El correo electrónico no es válido.");

        return limpio;
    }

    private async Task<Rol> ValidarRolAsync(int rolId, int? medicoId, int? excluirUsuarioId, CancellationToken ct)
    {
        var rol = await _repo.GetRolAsync(rolId, ct)
            ?? throw new DomainException(Errores.DATOS_INCOMPLETOS, "El nivel de acceso seleccionado no existe.");

        if (rol.Nombre == ROL_MEDICO)
        {
            if (medicoId is null)
                throw new DomainException(Errores.DATOS_INCOMPLETOS, "El nivel Médico solo se puede dar a un empleado con cargo de Médico.");

            if (await _repo.MedicoYaTieneUsuarioAsync(medicoId.Value, excluirUsuarioId, ct))
                throw new DomainException("REGISTRO_DUPLICADO", "Ese médico ya tiene un usuario asignado.", 409);
        }

        return rol;
    }

    private static UsuarioDto ToDto(Usuario u) => new(
        u.UsuarioId,
        u.NombreUsuario,
        u.NombreCompleto,
        u.Email,
        u.RolId,
        u.Rol?.Nombre ?? string.Empty,
        u.MedicoId,
        u.Activo,
        !string.IsNullOrEmpty(u.PasswordHash),
        u.DebeCambiarPassword,
        u.DebeCambiarPassword ? u.PasswordTemporalVence : null,
        u.FechaCreacion,
        u.UltimoAcceso);
}