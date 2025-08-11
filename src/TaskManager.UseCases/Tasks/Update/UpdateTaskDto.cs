namespace TaskManager.UseCases.Tasks.Update;

public class UpdateTaskDto
{
    public long ProjectId { get; set; }
    public long TaskId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }
}