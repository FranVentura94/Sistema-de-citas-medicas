using Domain.Entities;
using MediatR;

namespace Core.Features.Pacientes.Commands;

public record CreatePacienteCommand(
    string CodigoPaciente,
    string TipoDocumento,
    string NumeroDocumento,
    string Nombres,
    string Apellidos,
    DateTime FechaNacimiento,
    string Sexo,
    string? EstadoCivil,
    string? Telefono,
    string? TelefonoSecundario,
    string? Email,
    string? Direccion,
    string? Ciudad,
    string? Pais,
    string? Ocupacion,
    string? TipoSangre
) : IRequest<Paciente>;