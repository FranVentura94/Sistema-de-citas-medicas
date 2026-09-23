using Core.Features.Pacientes.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories;

/// <summary>
/// Repositorio específico de Paciente. Hereda la implementación genérica de la
/// biblioteca GenericPersistence, por lo que ya no repite la lógica CRUD.
/// </summary>
public class PacienteRepository
    : GenericPersistence.Implementations.Repository<Paciente, long>, IPacienteRepository
{
    public PacienteRepository(DbContext context) : base(context)
    {
    }
}
