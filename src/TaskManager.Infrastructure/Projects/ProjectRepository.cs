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

    public async Task<ProjectEntity?> FindByIdWithProjectMembersIncludedAsync(long id)
    {
        return await _context.Projects
            .Include(project => project.Members)
            .FirstOrDefaultAsync(project => project.Id == id);
    }

    public async Task<ProjectEntity?> FindByIdWithInvitesIncludedAsync(long id)
    {
        return await _context.Projects
            .Include(project => project.Invites)
            .FirstOrDefaultAsync(project => project.Id == id);
        ;
    }

    public async Task<IEnumerable<ProjectEntity>> GetAllByUserIdAsync(string userId)
    {
        return await _context
            .Projects
            .Where(project => project.LeadUserId == userId ||
                              _context
                                  .ProjectMembers
                                  .Any(member => member.MemberId == userId
                                                 && member.ProjectId == project.Id))
            .ToListAsync();
    }

    public void Update(ProjectEntity project)
    {
        _context.Update(project);
    }

    public void Remove(ProjectEntity project)
    {
        _context.Projects.Remove(project);
    }

    public async Task<bool> IsUserProjectMemberAsync(string currentUserId, long projectId)
    {
        return await _context.ProjectMembers.AnyAsync(projectMember =>
            projectMember.MemberId == currentUserId && projectMember.ProjectId == projectId);
    }

    public void AddMember(ProjectEntity project, string memberId)
    {
        _context.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = project.Id,
            MemberId = memberId,
            ProjectRole = ProjectRole.Member
        });
    }

    public async Task<IEnumerable<ProjectEntity>> GetAllByUserIdWhereUserIsLead(string userId)
    {
        return await _context.Projects.Where(project => project.LeadUserId == userId).ToListAsync();
    }

    public async Task<IEnumerable<ProjectEntity>> GetAllByUserIdWhereUserHasRoleAsync(string userId, ProjectRole role)
    {
        return await _context
            .Projects
            .Where(project => _context.ProjectMembers
                .Any(member => member.ProjectId == project.Id
                               && member.MemberId == userId
                               && member.ProjectRole == role))
            .ToListAsync();
    }
}