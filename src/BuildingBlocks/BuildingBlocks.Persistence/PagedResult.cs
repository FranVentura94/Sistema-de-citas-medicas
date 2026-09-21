namespace BuildingBlocks.Persistence;

public class PagedResult<T>
{
    public List<T> Data { get; set; } = [];
    public int TotalRecords { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalRecords / (double)PageSize);
}