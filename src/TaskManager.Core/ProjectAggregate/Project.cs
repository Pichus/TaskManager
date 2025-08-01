namespace TaskManager.Core.ProjectAggregate;

public class Project : EntityBase, IAggregateRoot
{
    public string Title { get; set; }
    
    public List<Task> Tasks { get; set; }
    public string LeadUserId { get; set; }
    public List<string> MemberUserIds { get; set; }
}