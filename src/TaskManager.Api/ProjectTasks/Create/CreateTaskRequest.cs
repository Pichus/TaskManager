namespace TaskManager.ProjectTasks.Create;

public class CreateTaskRequest
{
    public string AssigneeUserId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    [FutureDate(ErrorMessage = "Due date must be in the future")]
    public DateTime DueDate { get; set; }
}