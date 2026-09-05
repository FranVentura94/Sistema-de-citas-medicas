using Core.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
public class CitasController : GenericController<Citas, long>
{
    public CitasController(IRepository<Citas, long> repository) : base(repository)
    {
    }
}
