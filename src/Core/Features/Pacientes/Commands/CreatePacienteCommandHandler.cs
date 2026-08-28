using Core.Features.Pacientes.Interfaces;
using Domain.Entities;
using MediatR;

namespace Core.Features.Pacientes.Commands;

public class CreatePacienteCommandHandler
    : IRequestHandler<CreatePacienteCommand, Paciente>
{
    private readonly IPacienteRepository _repository;

    public CreatePacienteCommandHandler(IPacienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<Paciente> Handle(
        CreatePacienteCommand request,
        CancellationToken cancellationToken)
    {
        var paciente = new Paciente
        {
            CodigoPaciente = request.CodigoPaciente,
            TipoDocumento = request.TipoDocumento,
            NumeroDocumento = request.NumeroDocumento,
            Nombres = request.Nombres,
            Apellidos = request.Apellidos,
            FechaNacimiento = request.FechaNacimiento,
            Sexo = request.Sexo,
            EstadoCivil = request.EstadoCivil,
            Telefono = request.Telefono,
            TelefonoSecundario = request.TelefonoSecundario,
            Email = request.Email,
            Direccion = request.Direccion,
            Ciudad = request.Ciudad,
            Pais = request.Pais,
            Ocupacion = request.Ocupacion,
            TipoSangre = request.TipoSangre,
            Activo = true,
            FechaRegistro = DateTime.Now
        };

        return await _repository.AddAsync(paciente, cancellationToken);
    }
}