using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.UserAggregate;

namespace TaskManager.Core.TaskAggregate;

public class TaskEntity : EntityBase, IAggregateRoot
{
    public string Title { get; set; }
    public string Description { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime DueDate { get; set; }
    public string CreatedByUserId { get; set; }
    public string AssigneeUserId { get; set; }
    public long ProjectId { get; set; }

    public ProjectEntity Project { get; set; }
    public TaskManagerUser CreatedByUser { get; set; }
    public TaskManagerUser AssigneeUser { get; set; }

    public bool IsOverdue => Status != TaskStatus.Complete && DueDate < DateTime.UtcNow;
}