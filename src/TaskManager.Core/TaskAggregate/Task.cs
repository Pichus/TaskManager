using TaskManager.Core.ProjectAggregate;

namespace TaskManager.Core.TaskAggregate;

public class Task : EntityBase, IAggregateRoot
{
    public string Title { get; set; }
    public string Description { get; set; }
    public TaskStatus Status { get; set; }
    public long CreatedByUserId { get; set; }
    public long AssigneeUserId { get; set; }
    public long ProjectId { get; set; }
    
    public Project Project { get; set; }
}