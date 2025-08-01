using TaskManager.Core.TaskAggregate;

namespace TaskManager.Core.ProjectAggregate;

public class ProjectEntity : EntityBase, IAggregateRoot
{
    public string Title { get; set; }
    
    public List<TaskEntity> Tasks { get; set; }
    public string LeadUserId { get; set; }
    public List<string> MemberUserIds { get; set; }
}