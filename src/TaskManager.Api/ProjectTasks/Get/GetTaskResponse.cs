namespace TaskManager.ProjectTasks.Get;

public class GetTaskResponse
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public DateTime DueDate { get; set; }
    public string CreatedByUserId { get; set; }
    public string AssigneeUserId { get; set; }
    public long ProjectId { get; set; }
}