using Microsoft.EntityFrameworkCore;
using TaskManager.Core.ProjectAggregate;
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

    public async Task<bool> IsUserProjectMember(string userId, long projectId)
    {
        return await Context
            .ProjectMembers
            .AnyAsync(member => member.ProjectId == projectId
                                && member.MemberId == userId);
    }

    public async Task<bool> IsUserProjectManager(string userId, long projectId)
    {
        return await Context
            .ProjectMembers
            .AnyAsync(member => member.ProjectId == projectId
                                && member.MemberId == userId
                                && member.ProjectRole == ProjectRole.Manager);
    }

    public async Task<IEnumerable<ProjectMemberWithUser>> GetProjectMembersWithUsersAsync(long projectId)
    {
        return await Context
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
            .ToListAsync();
    }
}