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
            .FirstOrDefaultAsync(projectMember => projectMember.ProjectId == projectId
                                                  && projectMember.MemberId == memberId);
    }
}