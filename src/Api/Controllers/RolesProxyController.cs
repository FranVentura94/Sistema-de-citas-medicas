using Core.Exceptions;
using Core.Features.Roles.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesProxyController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RolesProxyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id, CancellationToken cancellationToken)
        {
            try
            {
                var rol = await _mediator.Send(new GetRolQuery { RolId = id }, cancellationToken);
                return Ok(rol);
            }
            catch (DomainException ex)
            {
                return BadRequest(new { codigo = ex.Codigo.ToString(), mensaje = ex.Message });
            }
        }
    }
}