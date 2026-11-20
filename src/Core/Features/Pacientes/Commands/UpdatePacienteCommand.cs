using Core.Exceptions;
using Core.Features.Pacientes.Interfaces;
using Domain.Entities;
using MediatR;

namespace Core.Features.Pacientes.Commands;

public record UpdatePacienteCommand(
    long PacienteID,
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
    string? TipoSangre,
    bool Activo
) : IRequest<Paciente>;

public class UpdatePacienteCommandHandler : IRequestHandler<UpdatePacienteCommand, Paciente>
{
    private readonly IPacienteRepository _repository;

    public UpdatePacienteCommandHandler(IPacienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<Paciente> Handle(UpdatePacienteCommand request, CancellationToken cancellationToken)
    {
        var paciente = await _repository.GetByIdAsync(request.PacienteID, cancellationToken)
            ?? throw new DomainException(Errores.RECURSO_NO_ENCONTRADO,
                $"No existe el paciente {request.PacienteID}.");

        if (await _repository.ExisteDocumentoAsync(request.NumeroDocumento.Trim(), request.PacienteID, cancellationToken))
        {
            throw new DomainException(Errores.REGISTRO_DUPLICADO,
                $"Ya existe otro paciente con el documento {request.NumeroDocumento}.");
        }

        paciente.TipoDocumento = request.TipoDocumento;
        paciente.NumeroDocumento = request.NumeroDocumento.Trim();
        paciente.Nombres = request.Nombres.Trim();
        paciente.Apellidos = request.Apellidos.Trim();
        paciente.FechaNacimiento = request.FechaNacimiento;
        paciente.Sexo = request.Sexo;
        paciente.EstadoCivil = request.EstadoCivil;
        paciente.Telefono = request.Telefono;
        paciente.TelefonoSecundario = request.TelefonoSecundario;
        paciente.Email = request.Email;
        paciente.Direccion = request.Direccion;
        paciente.Ciudad = request.Ciudad;
        paciente.Pais = request.Pais;
        paciente.Ocupacion = request.Ocupacion;
        paciente.TipoSangre = request.TipoSangre;
        paciente.Activo = request.Activo;

        await _repository.UpdateAsync(paciente, cancellationToken);
        return paciente;
    }
}