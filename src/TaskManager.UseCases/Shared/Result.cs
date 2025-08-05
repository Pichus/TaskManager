namespace TaskManager.UseCases.Shared;

public class Result
{
    public bool Success { get; set; } = true;
    public Error? Error { get; set; }
}