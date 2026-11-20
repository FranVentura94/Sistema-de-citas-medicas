using MsIdentity.Core.Dtos;
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
        if (string.IsNullOrWhiteSpace(request.NombreUsuario) || string.IsNullOrWhiteSpace(request.Password))
            throw new DomainException(Errores.DATOS_INCOMPLETOS, "Usuario y contraseña son obligatorios.");

        var usuario = await _usuarios.GetByNombreUsuarioAsync(request.NombreUsuario.Trim(), cancellationToken);

        // Mismo mensaje si no existe o si la clave es incorrecta (no damos pistas)
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            throw new DomainException(Errores.CREDENCIALES_INVALIDAS, "Usuario o contraseña incorrectos.", 401);

        if (!usuario.Activo)
            throw new DomainException(Errores.USUARIO_INACTIVO, "Tu usuario está desactivado. Contacta al administrador.", 403);

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

        return new LoginResponse(token, expira, info);
    }
}