using TaskManager.Core.TaskAggregate;

namespace TaskManager.UseCases.Tasks.Retrieve;

public class RetrieveAllTasksDto
{
    public long? ProjectId { get; set; }
    public string? AssigneeUserId { get; set; }
    public Status? Status { get; set; }
    public SortDto? Sort { get; set; }
    public OrderDto? Order { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}