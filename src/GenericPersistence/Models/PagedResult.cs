namespace GenericPersistence.Models;

/// <summary>
/// Resultado de una consulta paginada: la página de datos más la información
/// necesaria para que el cliente navegue entre páginas.
/// </summary>
public class PagedResult<TEntity>
{
    public IEnumerable<TEntity> Data { get; set; } = new List<TEntity>();
    public int TotalRecords { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }

    /// <summary>Total de páginas (redondeando hacia arriba).</summary>
    public int TotalPage => PageSize > 0
        ? (int)Math.Ceiling(TotalRecords / (double)PageSize)
        : 0;
}
