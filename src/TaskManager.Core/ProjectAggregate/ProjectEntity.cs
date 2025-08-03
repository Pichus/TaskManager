using TaskManager.Core.TaskAggregate;
using TaskManager.Core.UserAggregate;

namespace TaskManager.Core.ProjectAggregate;

public class ProjectEntity : EntityBase, IAggregateRoot
{
    public string Title { get; set; }
    public string LeadUserId { get; set; }
    public IEnumerable<TaskManagerUser> Members { get; set; }
    public IEnumerable<TaskEntity> Tasks { get; set; }
}