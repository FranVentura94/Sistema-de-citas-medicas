using Core.Features.Empleados.Dtos;
using Domain.Entities;

namespace Core.Features.Empleados;

public static class EmpleadoMapper
{
    public static string NombreCompleto(this Empleado e) => $"{e.Nombres} {e.Apellidos}".Trim();

    public static List<MedicoAsignadoDto> Asignados(this Empleado e) =>
        e.MedicosAsignados
            .Where(a => a.Medico is not null)
            .Select(a => new MedicoAsignadoDto(a.MedicoID, $"{a.Medico!.Nombres} {a.Medico.Apellidos}".Trim()))
            .OrderBy(a => a.Nombre)
            .ToList();

    public static EmpleadoDto ToDto(this Empleado e) => new(
        e.EmpleadoID,
        e.Nombres,
        e.Apellidos,
        e.NombreCompleto(),
        e.Identificacion,
        e.FechaNacimiento,
        e.Genero,
        e.Telefono,
        e.Email,
        e.Direccion,
        e.Cargo,
        e.FechaIngreso,
        e.Notas,
        e.EmergenciaNombre,
        e.EmergenciaParentesco,
        e.EmergenciaTelefono,
        e.Activo,
        e.FechaRegistro,
        e.MedicoID,
        e.UsuarioID,
        e.Medico?.NumeroColegiatura,
        e.Medico?.Especialidades.Select(x => x.EspecialidadID).ToList() ?? new List<int>(),
        e.Medico?.Especialidades.FirstOrDefault(x => x.Principal)?.EspecialidadID,
        e.Asignados());
}