using Core.Exceptions;
using Core.Features.Medicos.Dtos;
using Core.Features.Medicos.Interfaces;
using MediatR;

namespace Core.Features.Medicos.Queries;

// ---------- Listar médicos (opcional: por especialidad) ----------
public record GetMedicosQuery(int? EspecialidadId) : IRequest<List<MedicoDto>>;

public class GetMedicosQueryHandler : IRequestHandler<GetMedicosQuery, List<MedicoDto>>
{
    private readonly IMedicoRepository _repository;

    public GetMedicosQueryHandler(IMedicoRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<MedicoDto>> Handle(GetMedicosQuery request, CancellationToken cancellationToken)
    {
        var medicos = await _repository.GetAllAsync(request.EspecialidadId, cancellationToken);
        var generos = await _repository.GetGenerosAsync(medicos.Select(m => m.MedicoID), cancellationToken);
        return medicos.Select(m => m.ToDto(generos.GetValueOrDefault(m.MedicoID))).ToList();
    }
}

// ---------- Un médico por Id ----------
public record GetMedicoByIdQuery(int MedicoID) : IRequest<MedicoDto>;

public class GetMedicoByIdQueryHandler : IRequestHandler<GetMedicoByIdQuery, MedicoDto>
{
    private readonly IMedicoRepository _repository;

    public GetMedicoByIdQueryHandler(IMedicoRepository repository)
    {
        _repository = repository;
    }

    public async Task<MedicoDto> Handle(GetMedicoByIdQuery request, CancellationToken cancellationToken)
    {
        var medico = await _repository.GetByIdAsync(request.MedicoID, cancellationToken)
            ?? throw new DomainException(Errores.RECURSO_NO_ENCONTRADO,
                $"No existe el médico {request.MedicoID}.");

        var generos = await _repository.GetGenerosAsync(new[] { medico.MedicoID }, cancellationToken);
        return medico.ToDto(generos.GetValueOrDefault(medico.MedicoID));
    }
}

// ---------- Catálogo de especialidades ----------
public record GetEspecialidadesQuery : IRequest<List<EspecialidadDto>>;

public class GetEspecialidadesQueryHandler : IRequestHandler<GetEspecialidadesQuery, List<EspecialidadDto>>
{
    private readonly IMedicoRepository _repository;

    public GetEspecialidadesQueryHandler(IMedicoRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<EspecialidadDto>> Handle(GetEspecialidadesQuery request, CancellationToken cancellationToken)
    {
        var especialidades = await _repository.GetEspecialidadesAsync(cancellationToken);
        return especialidades
            .Select(e => new EspecialidadDto(e.EspecialidadID, e.Codigo, e.Nombre, e.Descripcion))
            .ToList();
    }
}