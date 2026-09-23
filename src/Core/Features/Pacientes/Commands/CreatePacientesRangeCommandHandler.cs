using Core.Features.Pacientes.Interfaces;
using Domain.Entities;
using MediatR;

namespace Core.Features.Pacientes.Commands;

public class CreatePacientesRangeCommandHandler
    : IRequestHandler<CreatePacientesRangeCommand, List<Paciente>>
{
    private readonly IPacienteRepository _pacienteRepository;

    public CreatePacientesRangeCommandHandler(IPacienteRepository pacienteRepository)
    {
        _pacienteRepository = pacienteRepository;
    }

    public async Task<List<Paciente>> Handle(
        CreatePacientesRangeCommand request,
        CancellationToken cancellationToken)
    {
        var pacientes = request.Pacientes.Select(p => new Paciente
        {
            CodigoPaciente = p.CodigoPaciente,
            TipoDocumento = p.TipoDocumento,
            NumeroDocumento = p.NumeroDocumento,
            Nombres = p.Nombres,
            Apellidos = p.Apellidos,
            FechaNacimiento = p.FechaNacimiento,
            Sexo = p.Sexo,
            EstadoCivil = p.EstadoCivil,
            Telefono = p.Telefono,
            TelefonoSecundario = p.TelefonoSecundario,
            Email = p.Email,
            Direccion = p.Direccion,
            Ciudad = p.Ciudad,
            Pais = p.Pais,
            Ocupacion = p.Ocupacion,
            TipoSangre = p.TipoSangre,
            Activo = true,
            FechaRegistro = DateTime.Now
        }).ToList();

        return await _pacienteRepository.AddRangeAsync(pacientes, cancellationToken);
    }
}
