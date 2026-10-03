using Domain.Models;

namespace Core.Services;

public interface IPasarelaGateway
{
    Task<CobroResponse> CobrarAsync(CobroRequest request);
    Task<CobroResponse?> ConsultarCobroAsync(string referencia);
}