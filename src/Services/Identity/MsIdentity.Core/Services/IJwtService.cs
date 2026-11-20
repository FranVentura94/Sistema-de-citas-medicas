using MsIdentity.Core.Entities;

namespace MsIdentity.Core.Services;

public interface IJwtService
{
    (string Token, DateTime ExpiraEn) GenerarToken(Usuario usuario, IEnumerable<string> permisos);
}