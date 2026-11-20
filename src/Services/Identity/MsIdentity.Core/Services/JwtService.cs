using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MsIdentity.Core.Configs;
using MsIdentity.Core.Entities;

namespace MsIdentity.Core.Services;

public class JwtService : IJwtService
{
    private readonly JwtOptions _options;

    public JwtService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public (string Token, DateTime ExpiraEn) GenerarToken(Usuario usuario, IEnumerable<string> permisos)
    {
        var expira = DateTime.UtcNow.AddMinutes(_options.ExpiraEnMinutos);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.UsuarioId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, usuario.NombreUsuario),
            new("nombre", usuario.NombreCompleto),
            new(ClaimTypes.Role, usuario.Rol!.Nombre)
        };

        if (usuario.MedicoId.HasValue)
            claims.Add(new Claim("medicoId", usuario.MedicoId.Value.ToString()));

        claims.AddRange(permisos.Select(p => new Claim("permiso", p)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expira,
            signingCredentials: credenciales);

        return (new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}