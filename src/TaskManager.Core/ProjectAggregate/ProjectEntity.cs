using TaskManager.Core.TaskAggregate;

namespace TaskManager.Core.ProjectAggregate;

public class ProjectEntity : EntityBase, IAggregateRoot
{
    public string Title { get; set; }
    public string LeadUserId { get; set; }
    public IEnumerable<ProjectMember> Members { get; set; }
    public IEnumerable<TaskEntity> Tasks { get; set; }
    public IEnumerable<ProjectInvite> Invites { get; set; }
}