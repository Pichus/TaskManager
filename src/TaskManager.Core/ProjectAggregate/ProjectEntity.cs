using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Core.TaskAggregate;

namespace TaskManager.Core.ProjectAggregate;

public class ProjectEntity : EntityBase, IAggregateRoot
{
    public string Title { get; set; }
    public string LeadUserId { get; set; }
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
    public ICollection<ProjectInvite> Invites { get; set; } = new List<ProjectInvite>();
}