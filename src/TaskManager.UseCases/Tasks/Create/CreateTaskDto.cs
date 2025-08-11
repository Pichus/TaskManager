namespace TaskManager.UseCases.Tasks.Create;

public class CreateTaskDto
{
    public long ProjectId { get; set; }
    public string AssigneeUserId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }
}