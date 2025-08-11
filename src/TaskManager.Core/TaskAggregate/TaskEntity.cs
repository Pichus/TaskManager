using TaskManager.Core.ProjectAggregate;

namespace TaskManager.Core.TaskAggregate;

public class TaskEntity : EntityBase, IAggregateRoot
{
    public string Title { get; set; }
    public string Description { get; set; }
    public Status Status { get; set; }
    public DateTime DueDate { get; set; }
    public string CreatedByUserId { get; set; }
    public string AssigneeUserId { get; set; }
    public long ProjectId { get; set; }

    public ProjectEntity Project { get; set; }

    public bool IsOverdue => Status != Status.Complete && DueDate < DateTime.UtcNow;
}