using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.ProjectInvites;

public class ProjectInviteRepository : IProjectInviteRepository
{
    private readonly AppDbContext _context;

    public ProjectInviteRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Create(ProjectInvite invite)
    {
        _context.Add(invite);
    }

    public async Task<ProjectInvite?> FindByIdAsync(long id)
    {
        return await _context.ProjectInvites.FindAsync(id);
    }

    public async Task<bool> AnyAsync(Expression<Func<ProjectInvite, bool>> predicate)
    {
        return await _context.ProjectInvites
            .AnyAsync(predicate);
    }

    public void Delete(ProjectInvite invite)
    {
        _context.ProjectInvites.Remove(invite);
    }

    public IQueryable<ProjectInvite> GetPendingInvitesByInvitedUserIdAsync(string userId)
    {
        return _context.ProjectInvites.Where(invite =>
            invite.InvitedUserId == userId && invite.Status == InviteStatus.Pending);
    }
}