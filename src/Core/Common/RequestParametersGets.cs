namespace Core.Common;

/// <summary>
/// Parámetros comunes de las consultas paginadas: número de página, tamaño
/// de página, filtro dinámico (texto, ej. x.Nombres == "Ana") y orden
/// dinámico (texto, ej. "Nombres desc").
/// </summary>
public class RequestParametersGets
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? Filter { get; set; }

    public string? OrderBy { get; set; }
}
