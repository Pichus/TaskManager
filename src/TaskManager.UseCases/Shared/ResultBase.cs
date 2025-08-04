namespace TaskManager.UseCases.Shared;

public class ResultBase
{
    public bool Success { get; set; } = true;
    public string ErrorMessage { get; set; } = string.Empty;
}