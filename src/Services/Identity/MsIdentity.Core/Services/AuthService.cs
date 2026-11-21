using MsIdentity.Core.Dtos;
using MsIdentity.Core.Entities;
using MsIdentity.Core.Exceptions;
using MsIdentity.Core.Interfaces;

namespace MsIdentity.Core.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IJwtService _jwt;

    public AuthService(IUsuarioRepository usuarios, IJwtService jwt)
    {
        _usuarios = usuarios;
        _jwt = jwt;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var usuario = await VerificarAsync(request.NombreUsuario, request.Password, cancellationToken);

        // Entró con una contraseña temporal: NO recibe acceso, primero debe crear la suya
        if (usuario.DebeCambiarPassword)
            return new LoginResponse(null, null, null, RequiereCambioPassword: true);

        return await EmitirAsync(usuario, cancellationToken);
    }

    // Primer ingreso: cambia la contraseña temporal por la definitiva y entra al portal
    public async Task<LoginResponse> PrimerIngresoAsync(PrimerIngresoRequest request, CancellationToken cancellationToken)
    {
        var usuario = await VerificarAsync(request.NombreUsuario, request.PasswordTemporal, cancellationToken);

        if (!usuario.DebeCambiarPassword)
            throw new DomainException(Errores.DATOS_INCOMPLETOS, "Tu usuario ya tiene una contraseña propia. Inicia sesión normalmente.");

        ReglasPassword.Validar(request.NuevaPassword);

        if (request.NuevaPassword == request.PasswordTemporal)
            throw new DomainException(Errores.DATOS_INCOMPLETOS, "La nueva contraseña debe ser diferente a la temporal.");

        // Solo la persona conoce esta contraseña: se guarda cifrada
        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NuevaPassword);
        usuario.DebeCambiarPassword = false;
        usuario.PasswordTemporalVence = null;

        return await EmitirAsync(usuario, cancellationToken);
    }

    // Revisa usuario, contraseña, que esté activo y que la temporal no haya vencido
    private async Task<Usuario> VerificarAsync(string nombreUsuario, string password, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(password))
            throw new DomainException(Errores.DATOS_INCOMPLETOS, "Usuario y contraseña son obligatorios.");

        var usuario = await _usuarios.GetByNombreUsuarioAsync(nombreUsuario.Trim().ToLowerInvariant(), cancellationToken);

        // Mismo mensaje si no existe, si aún no tiene contraseña o si es incorrecta (no damos pistas)
        if (usuario is null || string.IsNullOrEmpty(usuario.PasswordHash) || !BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
            throw new DomainException(Errores.CREDENCIALES_INVALIDAS, "Usuario o contraseña incorrectos.", 401);

        if (!usuario.Activo)
            throw new DomainException(Errores.USUARIO_INACTIVO, "Tu acceso al portal está desactivado. Contacta al administrador.", 403);

        if (usuario.DebeCambiarPassword && usuario.PasswordTemporalVence < DateTime.Now)
            throw new DomainException(Errores.TEMPORAL_VENCIDA, "Tu contraseña temporal venció. Pide al administrador una nueva.", 401);

        return usuario;
    }

    private async Task<LoginResponse> EmitirAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        var permisos = usuario.Rol!.RolPermisos
            .Select(rp => rp.Permiso!.Codigo)
            .ToList();

        var (token, expira) = _jwt.GenerarToken(usuario, permisos);

        usuario.UltimoAcceso = DateTime.Now;
        await _usuarios.GuardarCambiosAsync(cancellationToken);

        var info = new UsuarioInfo(
            usuario.UsuarioId,
            usuario.NombreUsuario,
            usuario.NombreCompleto,
            usuario.Email,
            usuario.Rol.Nombre,
            usuario.MedicoId,
            permisos);

        return new LoginResponse(token, expira, info, RequiereCambioPassword: false);
    }
}