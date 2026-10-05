using Core.Dtos;

namespace Core.Services
{
    public interface IRolService
    {
        Task<RolDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    }
}