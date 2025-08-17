namespace TaskManager.UseCases.Projects.Get;

public class GetAllByUserDto
{
    public RoleDto Role { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}