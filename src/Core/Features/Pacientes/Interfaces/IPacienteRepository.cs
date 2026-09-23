using Domain.Entities;
using GenericPersistence.Abstractions;

namespace Core.Features.Pacientes.Interfaces;

/// <summary>
/// Repositorio específico de Paciente. Hereda todo el contrato genérico
/// (GetAllAsync, GetByIdAsync, AddAsync, UpdateAsync, DeleteAsync, GetPagedAsync)
/// de la biblioteca GenericPersistence; aquí solo se agregarían métodos propios de Paciente.
/// </summary>
public interface IPacienteRepository : IRepository<Paciente, long>
{
}
