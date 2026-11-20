using Core.Exceptions;
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
        if (string.IsNullOrWhiteSpace(request.NumeroDocumento) ||
            string.IsNullOrWhiteSpace(request.Nombres) ||
            string.IsNullOrWhiteSpace(request.Apellidos))
        {
            throw new DomainException(Errores.DATOS_INVALIDOS,
                "Documento, nombres y apellidos son obligatorios.");
        }

        if (await _repository.ExisteDocumentoAsync(request.NumeroDocumento.Trim(), null, cancellationToken))
        {
            throw new DomainException(Errores.REGISTRO_DUPLICADO,
                $"Ya existe un paciente con el documento {request.NumeroDocumento}.");
        }

        var paciente = new Paciente
        {
            // Si no envían código, se genera uno automático
            CodigoPaciente = string.IsNullOrWhiteSpace(request.CodigoPaciente)
                ? $"PAC-{DateTime.Now:yyMMddHHmmss}"
                : request.CodigoPaciente,
            TipoDocumento = request.TipoDocumento,
            NumeroDocumento = request.NumeroDocumento.Trim(),
            Nombres = request.Nombres.Trim(),
            Apellidos = request.Apellidos.Trim(),
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