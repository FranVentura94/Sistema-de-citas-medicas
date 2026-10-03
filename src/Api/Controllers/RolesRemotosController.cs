using Core.Features.Roles.Queries;
using Core.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesRemotosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IIdentityService _identityService;

    public RolesRemotosController(IMediator mediator, IIdentityService identityService)
    {
        _mediator = mediator;
        _identityService = identityService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var roles = await _mediator.Send(new GetRolesRemotosQuery(), cancellationToken);
        return Ok(roles);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRolById(int id)
    {
        var rol = await _identityService.GetRolByIdAsync(id);
        return Ok(rol);
    }

    [HttpPost]
    public async Task<IActionResult> CrearRol(CrearRolRequest request)
    {
        var rol = await _identityService.CrearRolAsync(request.Nombre, request.Descripcion);
        return Ok(rol);
    }
}

public record CrearRolRequest(string Nombre, string? Descripcion);