using Core.Features.Medicos.Dtos;
using Domain.Entities;

namespace Core.Features.Medicos;

public static class MedicoMapper
{
    public static MedicoDto ToDto(this Medico m) => new(
        m.MedicoID,
        m.UsuarioID,
        m.Nombres,
        m.Apellidos,
        $"Dr(a). {m.Nombres} {m.Apellidos}",
        m.NumeroColegiatura,
        m.Identification,
        m.Telefono,
        m.Email,
        m.Activo,
        m.Especialidades
            .OrderByDescending(e => e.Principal)
            .Select(e => new MedicoEspecialidadDto(e.EspecialidadID, e.Especialidad?.Nombre ?? string.Empty, e.Principal))
            .ToList());
}