using Microsoft.EntityFrameworkCore;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Shared;

namespace TaskManager.Infrastructure.Projects;

public class ProjectRepository : RepositoryBase<ProjectEntity, long>, IProjectRepository
{
    public ProjectRepository(AppDbContext context) : base(context)
    {
    }

    public void Create(ProjectEntity project)
    {
        Context.Add(project);
    }

    public async Task<ProjectEntity?> FindByIdWithProjectMembersIncludedAsync(long id)
    {
        return await Context.Projects
            .Include(project => project.Members)
            .FirstOrDefaultAsync(project => project.Id == id);
    }

    public async Task<ProjectEntity?> FindByIdWithInvitesIncludedAsync(long id)
    {
        return await Context.Projects
            .Include(project => project.Invites)
            .FirstOrDefaultAsync(project => project.Id == id);
        ;
    }

    public async Task<IEnumerable<ProjectEntity>> GetAllByUserIdAsync(string userId)
    {
        return await Context
            .Projects
            .Where(project => project.LeadUserId == userId ||
                              Context
                                  .ProjectMembers
                                  .Any(member => member.MemberId == userId
                                                 && member.ProjectId == project.Id))
            .ToListAsync();
    }

    public async Task<bool> IsUserProjectMemberAsync(string currentUserId, long projectId)
    {
        return await Context.ProjectMembers.AnyAsync(projectMember =>
            projectMember.MemberId == currentUserId && projectMember.ProjectId == projectId);
    }

    public async Task<IEnumerable<ProjectEntity>> GetAllByUserIdWhereUserIsLead(string userId)
    {
        return await Context.Projects.Where(project => project.LeadUserId == userId).ToListAsync();
    }

    public async Task<IEnumerable<ProjectEntity>> GetAllByUserIdWhereUserHasRoleAsync(string userId, ProjectRole role)
    {
        return await Context
            .Projects
            .Where(project => Context.ProjectMembers
                .Any(member => member.ProjectId == project.Id
                               && member.MemberId == userId
                               && member.ProjectRole == role))
            .ToListAsync();
    }
}