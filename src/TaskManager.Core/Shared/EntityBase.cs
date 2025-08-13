using System.ComponentModel.DataAnnotations;

namespace TaskManager.Core.Shared;

public abstract class EntityBase
{
    [Key]
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
}