using Core.Exceptions;
using Core.Features.Medicos.Dtos;
using Core.Features.Medicos.Interfaces;
using Domain.Entities;
using MediatR;

namespace Core.Features.Medicos.Commands;

// Agregar una especialidad al catálogo. Solo Administrador.
public record CreateEspecialidadCommand(string Codigo, string Nombre, string? Descripcion) : IRequest<EspecialidadDto>;

public class CreateEspecialidadCommandHandler : IRequestHandler<CreateEspecialidadCommand, EspecialidadDto>
{
    private readonly IMedicoRepository _repository;

    public CreateEspecialidadCommandHandler(IMedicoRepository repository)
    {
        _repository = repository;
    }

    public async Task<EspecialidadDto> Handle(CreateEspecialidadCommand request, CancellationToken cancellationToken)
    {
        var codigo = (request.Codigo ?? string.Empty).Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(request.Nombre))
            throw new DomainException(Errores.DATOS_INVALIDOS, "El código y el nombre de la especialidad son obligatorios.");

        if (await _repository.ExisteEspecialidadAsync(codigo, request.Nombre.Trim(), cancellationToken))
            throw new DomainException(Errores.REGISTRO_DUPLICADO, "Ya existe una especialidad con ese código o nombre.");

        var especialidad = new Especialidad
        {
            Codigo = codigo,
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion,
            Activo = true
        };

        await _repository.AddEspecialidadAsync(especialidad, cancellationToken);
        return new EspecialidadDto(especialidad.EspecialidadID, especialidad.Codigo, especialidad.Nombre, especialidad.Descripcion);
    }
}