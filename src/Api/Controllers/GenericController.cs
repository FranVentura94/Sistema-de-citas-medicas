using Core.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Controlador base genérico: expone el CRUD completo (GET, GET/{id}, POST,
/// PUT, DELETE) para cualquier entidad apoyándose únicamente en
/// IRepository&lt;TEntity, TKey&gt;. Los controladores concretos (ej.
/// MedicosController) solo heredan de esta clase e indican su ruta.
/// </summary>
[ApiController]
public abstract class GenericController<TEntity, TKey> : ControllerBase
    where TEntity : class
{
    private readonly IRepository<TEntity, TKey> _repository;

    protected GenericController(IRepository<TEntity, TKey> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<List<TEntity>>> GetAll(CancellationToken cancellationToken)
    {
        var entidades = await _repository.GetAllAsync(cancellationToken);
        return Ok(entidades);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TEntity>> GetById(TKey id, CancellationToken cancellationToken)
    {
        var entidad = await _repository.GetByIdAsync(id, cancellationToken);

        if (entidad is null)
        {
            return NotFound();
        }

        return Ok(entidad);
    }

    [HttpPost]
    public async Task<ActionResult<TEntity>> Create(TEntity entidad, CancellationToken cancellationToken)
    {
        var creada = await _repository.AddAsync(entidad, cancellationToken);
        return Ok(creada);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TEntity>> Update(TKey id, TEntity entidad, CancellationToken cancellationToken)
    {
        var actualizada = await _repository.UpdateAsync(entidad, cancellationToken);
        return Ok(actualizada);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(TKey id, CancellationToken cancellationToken)
    {
        var eliminado = await _repository.DeleteAsync(id, cancellationToken);

        if (!eliminado)
        {
            return NotFound();
        }

        return NoContent();
    }
}
