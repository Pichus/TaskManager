namespace TaskManager.UseCases.Shared;

public class Error
{
    public Error(string code, string message = "")
    {
        Code = code;
        Message = message;
    }

    public string Code { get; set; }
    public string Message { get; set; }
}