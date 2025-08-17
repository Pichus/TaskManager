using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Shared;

public class PaginationQuery
{
    [Range(1, 50, ErrorMessage = "Page size must be between 1 and 50")]
    [DefaultValue(25)]
    public int PageSize { get; set; } = 25;

    [Range(1, int.MaxValue, ErrorMessage = "Page number must be positive")]
    [DefaultValue(1)]
    public int PageNumber { get; set; } = 1;
}