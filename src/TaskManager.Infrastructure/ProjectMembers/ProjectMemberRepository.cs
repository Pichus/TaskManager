using Microsoft.EntityFrameworkCore;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.ProjectMembers;

public class ProjectMemberRepository : IProjectMemberRepository
{
    private readonly AppDbContext _context;

    public ProjectMemberRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectMember?> GetByProjectIdAndMemberIdAsync(long projectId, string memberId)
    {
        return await _context
            .ProjectMembers
            .Include(member => member.MemberRole)
            .FirstOrDefaultAsync(member => member.ProjectId == projectId
                                           && member.MemberId == memberId);
    }

    public async Task<bool> IsUserProjectMember(string userId, long projectId)
    {
        return await _context
            .ProjectMembers
            .AnyAsync(member => member.ProjectId == projectId
                                && member.MemberId == userId);
    }

    public async Task<IEnumerable<ProjectMemberWithUser>> GetProjectMembersWithUsersAsync(long projectId)
    {
        return await _context
            .ProjectMembers
            .Where(member => member.ProjectId == projectId)
            .Join(_context.Users, member => member.MemberId, user => user.Id, (member, user) =>
                new ProjectMemberWithUser
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = member.MemberRole.Role
                }
            )
            .ToListAsync();
    }

    public void Update(ProjectMember projectMember)
    {
        _context.Update(projectMember);
    }

    public void Delete(ProjectMember projectMember)
    {
        _context.ProjectMembers.Remove(projectMember);
    }
}