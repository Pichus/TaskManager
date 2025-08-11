namespace TaskManager.UseCases.Tasks.Update;

public class UpdateTaskDto
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }
}