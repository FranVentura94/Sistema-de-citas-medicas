using Core.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
public class AtencionesController : GenericController<Atenciones, long>
{
    public AtencionesController(IRepository<Atenciones, long> repository) : base(repository)
    {
    }
}
