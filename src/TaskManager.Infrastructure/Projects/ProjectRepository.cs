using Microsoft.EntityFrameworkCore;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Projects;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public ProjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Create(ProjectEntity project)
    {
        _context.Add(project);
    }

    public async Task<ProjectEntity?> FindByIdAsync(long id)
    {
        return await _context.Projects.FindAsync(id);
    }

    public async Task<IEnumerable<ProjectEntity>> GetAllByUserIdAsync(string userId)
    {
        return await _context.Projects.Where(project => project.LeadUserId == userId).ToListAsync();
    }

    public Task GetPendingInvitesByProjectId(long projectId)
    {
        throw new NotImplementedException();
    }
}