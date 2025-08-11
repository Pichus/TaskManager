using System.ComponentModel.DataAnnotations;

namespace TaskManager.ProjectTasks.Create;

public class FutureDateAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return (value is DateTime dateTime) && (dateTime > DateTime.UtcNow);
    }
}