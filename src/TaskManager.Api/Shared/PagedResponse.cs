namespace TaskManager.Shared;

public class PagedResponse<TData>
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public int TotalRecords { get; init; }
    public IEnumerable<TData> Data { get; init; }
}