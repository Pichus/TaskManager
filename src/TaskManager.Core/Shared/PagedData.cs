namespace TaskManager.Core.Shared;

public class PagedData<TResponse>
{
    public PagedData(List<TResponse> data, int pageNumber, int pageSize, int totalRecords)
    {
        Data = data;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalRecords = totalRecords;
        TotalPages = (int)Math.Ceiling(totalRecords / (decimal)pageSize);
    }

    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalRecords { get; init; }
    public int TotalPages { get; init; }
    public List<TResponse> Data { get; init; }
}