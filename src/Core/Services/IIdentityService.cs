using Domain.Models;

namespace Core.Services;

public interface IIdentityService
{
    Task<List<RolDto>> GetRolesAsync();
    Task<RolDto> GetRolByIdAsync(int id);
    Task<RolDto> CrearRolAsync(string nombre, string? descripcion);
}