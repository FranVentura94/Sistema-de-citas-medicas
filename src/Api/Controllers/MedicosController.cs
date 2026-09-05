using Core.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
public class MedicosController : GenericController<Medico, int>
{
    public MedicosController(IRepository<Medico, int> repository) : base(repository)
    {
    }
}
