using Core.Exceptions;
using Core.Features.Empleados.Dtos;
using Core.Features.Empleados.Interfaces;
using MediatR;

namespace Core.Features.Empleados.Commands;

// Enlaza (o desenlaza con null) la ficha del empleado con su usuario del portal. Solo Administrador.
public record AsignarUsuarioCommand(int EmpleadoID, int? UsuarioID) : IRequest<EmpleadoDto>;

public class AsignarUsuarioCommandHandler : IRequestHandler<AsignarUsuarioCommand, EmpleadoDto>
{
    private readonly IEmpleadoRepository _repository;

    public AsignarUsuarioCommandHandler(IEmpleadoRepository repository)
    {
        _repository = repository;
    }

    public async Task<EmpleadoDto> Handle(AsignarUsuarioCommand request, CancellationToken ct)
    {
        var empleado = await _repository.GetByIdAsync(request.EmpleadoID, ct)
            ?? throw new DomainException(Errores.RECURSO_NO_ENCONTRADO, "El empleado no existe.");

        if (request.UsuarioID is int usuarioId && await _repository.UsuarioYaAsignadoAsync(usuarioId, request.EmpleadoID, ct))
            throw new DomainException(Errores.REGISTRO_DUPLICADO, "Ese usuario ya pertenece a otro empleado.");

        empleado.UsuarioID = request.UsuarioID;
        if (empleado.Medico is not null)
            empleado.Medico.UsuarioID = request.UsuarioID;

        await _repository.GuardarCambiosAsync(ct);
        return empleado.ToDto();
    }
}