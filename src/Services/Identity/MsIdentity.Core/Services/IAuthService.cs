using MsIdentity.Core.Dtos;

namespace MsIdentity.Core.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<LoginResponse> PrimerIngresoAsync(PrimerIngresoRequest request, CancellationToken cancellationToken);
}