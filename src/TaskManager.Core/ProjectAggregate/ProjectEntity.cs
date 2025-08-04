using TaskManager.Core.TaskAggregate;

namespace TaskManager.Core.ProjectAggregate;

public class ProjectEntity : EntityBase, IAggregateRoot
{
    public string Title { get; set; }
    public string LeadUserId { get; set; }
    public ICollection<ProjectMember> Members { get; set; }
    public ICollection<TaskEntity> Tasks { get; set; }
    public ICollection<ProjectInvite> Invites { get; set; }
}