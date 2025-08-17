using Microsoft.EntityFrameworkCore;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.Shared;
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

    public async Task<PagedData<ProjectEntity>> GetAllByUserIdAsync(string userId, int pageNumber, int pageSize)
    {
        var query = Context
            .Projects
            .Where(project => project.LeadUserId == userId ||
                              Context
                                  .ProjectMembers
                                  .Any(member => member.MemberId == userId
                                                 && member.ProjectId == project.Id));

        var totalRecords = await query.CountAsync();
        var projects = await query.ToListAsync();

        return new PagedData<ProjectEntity>(projects, pageNumber, pageSize, totalRecords);
    }

    public async Task<PagedData<ProjectEntity>> GetAllByUserIdWhereUserIsLead(string userId, int pageNumber,
        int pageSize)
    {
        var query = Context
            .Projects
            .Where(project => project.LeadUserId == userId);

        var totalRecords = await query.CountAsync();
        var projects = await query.ToListAsync();

        return new PagedData<ProjectEntity>(projects, pageNumber, pageSize, totalRecords);
    }

    public async Task<PagedData<ProjectEntity>> GetAllByUserIdWhereUserHasRoleAsync(string userId, ProjectRole role,
        int pageNumber, int pageSize)
    {
        var query = Context
            .Projects
            .Where(project => Context.ProjectMembers
                .Any(member => member.ProjectId == project.Id
                               && member.MemberId == userId
                               && member.ProjectRole == role));

        var totalRecords = await query.CountAsync();
        var projects = await query.ToListAsync();

        return new PagedData<ProjectEntity>(projects, pageNumber, pageSize, totalRecords);
    }

    public async Task<bool> IsUserProjectMemberAsync(string currentUserId, long projectId)
    {
        return await Context.ProjectMembers.AnyAsync(projectMember =>
            projectMember.MemberId == currentUserId && projectMember.ProjectId == projectId);
    }
}