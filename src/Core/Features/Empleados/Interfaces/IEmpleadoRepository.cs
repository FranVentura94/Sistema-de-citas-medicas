using Domain.Entities;

namespace Core.Features.Empleados.Interfaces;

public interface IEmpleadoRepository
{
    Task<List<Empleado>> GetAllAsync(CancellationToken ct);
    Task<Empleado?> GetByIdAsync(int id, CancellationToken ct);              // con seguimiento (para editar)
    Task<Empleado?> GetByUsuarioAsync(int usuarioId, CancellationToken ct);
    Task<bool> ExisteIdentificacionAsync(string identificacion, int? excluirId, CancellationToken ct);
    Task<bool> UsuarioYaAsignadoAsync(int usuarioId, int excluirId, CancellationToken ct);
    Task<List<int>> MedicosExistentesAsync(IEnumerable<int> ids, CancellationToken ct);
    Task AddAsync(Empleado empleado, CancellationToken ct);
    Task GuardarCambiosAsync(CancellationToken ct);
}