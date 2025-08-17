using Microsoft.EntityFrameworkCore;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.Shared;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Shared;

namespace TaskManager.Infrastructure.ProjectMembers;

public class ProjectMemberRepository : RepositoryBase<ProjectMember, long>, IProjectMemberRepository
{
    public ProjectMemberRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<ProjectMember?> GetByProjectIdAndMemberIdAsync(long projectId, string memberId)
    {
        return await Context
            .ProjectMembers
            .FirstOrDefaultAsync(member => member.ProjectId == projectId
                                           && member.MemberId == memberId);
    }

    public async Task<bool> IsUserProjectMemberAsync(string userId, long projectId)
    {
        return await Context
            .ProjectMembers
            .AnyAsync(member => member.ProjectId == projectId
                                && member.MemberId == userId
                                && member.ProjectRole == ProjectRole.Member);
    }

    public async Task<bool> IsUserProjectManagerAsync(string userId, long projectId)
    {
        return await Context
            .ProjectMembers
            .AnyAsync(member => member.ProjectId == projectId
                                && member.MemberId == userId
                                && member.ProjectRole == ProjectRole.Manager);
    }

    public async Task<bool> IsUserProjectParticipantAsync(string userId, long projectId)
    {
        var isExplicitMember = await Context
            .ProjectMembers
            .AnyAsync(member => member.ProjectId == projectId
                                && member.MemberId == userId);

        var isLead = await IsUserProjectLeadAsync(userId, projectId);

        return isExplicitMember || isLead;
    }

    public async Task<bool> IsUserProjectLeadAsync(string userId, long projectId)
    {
        return await Context
            .Projects
            .AnyAsync(project => project.Id == projectId
                                 && project.LeadUserId == userId);
    }

    public async Task<PagedData<ProjectMemberWithUser>> GetProjectMembersWithUsersAsync(long projectId, int pageNumber,
        int pageSize)
    {
        var projectMembersQuery = Context
            .ProjectMembers
            .Where(member => member.ProjectId == projectId)
            .Join(Context.Users, member => member.MemberId, user => user.Id, (member, user) =>
                new ProjectMemberWithUser
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    ProjectRole = member.ProjectRole
                }
            )
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        var projectMembers = await projectMembersQuery.ToListAsync();
        var totalRecords = await projectMembersQuery.CountAsync();

        return new PagedData<ProjectMemberWithUser>(projectMembers, pageNumber, pageSize, totalRecords);
    }
}