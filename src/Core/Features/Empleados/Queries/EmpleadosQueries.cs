using Core.Exceptions;
using Core.Features.Empleados.Dtos;
using Core.Features.Empleados.Interfaces;
using MediatR;

namespace Core.Features.Empleados.Queries;

// ---------- Todo el personal ----------
public record GetEmpleadosQuery : IRequest<List<EmpleadoDto>>;

public class GetEmpleadosQueryHandler : IRequestHandler<GetEmpleadosQuery, List<EmpleadoDto>>
{
    private readonly IEmpleadoRepository _repository;

    public GetEmpleadosQueryHandler(IEmpleadoRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<EmpleadoDto>> Handle(GetEmpleadosQuery request, CancellationToken ct)
    {
        var empleados = await _repository.GetAllAsync(ct);
        return empleados.Select(e => e.ToDto()).ToList();
    }
}

// ---------- Un empleado ----------
public record GetEmpleadoByIdQuery(int EmpleadoID) : IRequest<EmpleadoDto>;

public class GetEmpleadoByIdQueryHandler : IRequestHandler<GetEmpleadoByIdQuery, EmpleadoDto>
{
    private readonly IEmpleadoRepository _repository;

    public GetEmpleadoByIdQueryHandler(IEmpleadoRepository repository)
    {
        _repository = repository;
    }

    public async Task<EmpleadoDto> Handle(GetEmpleadoByIdQuery request, CancellationToken ct)
    {
        var empleado = await _repository.GetByIdAsync(request.EmpleadoID, ct)
            ?? throw new DomainException(Errores.RECURSO_NO_ENCONTRADO, "El empleado no existe.");
        return empleado.ToDto();
    }
}

// ---------- El perfil de quien inició sesión (para saludarlo) ----------
public record GetMiPerfilQuery(int UsuarioID) : IRequest<MiPerfilDto>;

public class GetMiPerfilQueryHandler : IRequestHandler<GetMiPerfilQuery, MiPerfilDto>
{
    private readonly IEmpleadoRepository _repository;

    public GetMiPerfilQueryHandler(IEmpleadoRepository repository)
    {
        _repository = repository;
    }

    public async Task<MiPerfilDto> Handle(GetMiPerfilQuery request, CancellationToken ct)
    {
        var e = await _repository.GetByUsuarioAsync(request.UsuarioID, ct)
            ?? throw new DomainException(Errores.RECURSO_NO_ENCONTRADO, "Tu usuario no tiene ficha de empleado.");

        return new MiPerfilDto(e.EmpleadoID, e.Nombres, e.Apellidos, e.Genero, e.Cargo, e.FechaNacimiento, e.FechaIngreso, e.Asignados());
    }
}