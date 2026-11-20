using Core.Exceptions;
using Core.Features.Medicos.Dtos;
using Core.Features.Medicos.Interfaces;
using Domain.Entities;
using MediatR;

namespace Core.Features.Medicos.Commands;

public record CreateMedicoCommand(
    string Nombres,
    string Apellidos,
    string NumeroColegiatura,
    string Identification,
    string Telefono,
    string Email,
    int? UsuarioID,
    List<int> EspecialidadIds,
    int? EspecialidadPrincipalId
) : IRequest<MedicoDto>;

public class CreateMedicoCommandHandler : IRequestHandler<CreateMedicoCommand, MedicoDto>
{
    private readonly IMedicoRepository _repository;

    public CreateMedicoCommandHandler(IMedicoRepository repository)
    {
        _repository = repository;
    }

    public async Task<MedicoDto> Handle(CreateMedicoCommand request, CancellationToken cancellationToken)
    {
        if (request.EspecialidadIds is null || request.EspecialidadIds.Count == 0)
            throw new DomainException(Errores.DATOS_INVALIDOS, "El médico debe tener al menos una especialidad.");

        var principal = request.EspecialidadPrincipalId ?? request.EspecialidadIds[0];

        var medico = new Medico
        {
            Nombres = request.Nombres.Trim(),
            Apellidos = request.Apellidos.Trim(),
            NumeroColegiatura = request.NumeroColegiatura.Trim(),
            Identification = request.Identification.Trim(),
            Telefono = request.Telefono,
            Email = request.Email,
            UsuarioID = request.UsuarioID,
            FechaIngreso = DateTime.Now,
            Activo = true,
            Especialidades = request.EspecialidadIds
                .Distinct()
                .Select(id => new MedicoEspecialidad { EspecialidadID = id, Principal = id == principal })
                .ToList()
        };

        await _repository.AddAsync(medico, cancellationToken);

        // Se vuelve a leer para traer los nombres de las especialidades
        var creado = await _repository.GetByIdAsync(medico.MedicoID, cancellationToken);
        return creado!.ToDto();
    }
}