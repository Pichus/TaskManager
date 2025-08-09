using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.Infrastructure.Identity.User;
using TaskManager.UseCases.Invites.Accept;
using TaskManager.UseCases.Invites.Create;
using TaskManager.UseCases.Invites.Delete;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites;

public class InviteService : IInviteService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly AppDbContext _dbContext;
    private readonly IProjectInviteRepository _projectInviteRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly UserManager<TaskManagerUser> _userManager;

    public InviteService(IProjectInviteRepository projectInviteRepository, IProjectRepository projectRepository,
        ICurrentUserService currentUserService, AppDbContext dbContext, UserManager<TaskManagerUser> userManager)
    {
        _projectInviteRepository = projectInviteRepository;
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<Result<ProjectInvite>> CreateAsync(CreateInviteDto createInviteDto)
    {
        var project = await _projectRepository.FindByIdAsyncWithProjectMembersIncludedAsync(createInviteDto.ProjectId);

        if (project is null)
            return Result<ProjectInvite>.Failure(CreateInviteErrors.ProjectNotFound);

        var invitedByUserId = _currentUserService.UserId;

        if (invitedByUserId != project.LeadUserId)
            return Result<ProjectInvite>.Failure(CreateInviteErrors.AccessDenied);

        var invitedUser = await _userManager.FindByIdAsync(createInviteDto.InvitedUserId);

        if (invitedUser is null)
            return Result<ProjectInvite>.Failure(CreateInviteErrors.InvitedUserNotFound);

        var inviteExists = await _projectInviteRepository
            .AnyAsync(projectInvite => projectInvite.InvitedUserId == invitedUser.Id
                                       && projectInvite.ProjectId == project.Id);

        if (inviteExists)
            return Result<ProjectInvite>.Failure(CreateInviteErrors.UserAlreadyInvited);

        var isInvitedUserProjectMember = project.Members.Any(member => member.MemberId == invitedUser.Id);

        if (isInvitedUserProjectMember)
            return Result<ProjectInvite>.Failure(CreateInviteErrors.InvitedUserAlreadyAMember);

        var invite = new ProjectInvite
        {
            CreatedAt = DateTime.UtcNow,
            ProjectId = project.Id,
            InvitedUserId = invitedUser.Id,
            InvitedByUserId = invitedByUserId,
            Status = InviteStatus.Pending
        };

        _projectInviteRepository.Create(invite);
        await _dbContext.SaveChangesAsync();

        return Result<ProjectInvite>.Success(invite);
    }

    public async Task<Result> DeleteAsync(long inviteId)
    {
        var invite = await _projectInviteRepository.FindByIdAsync(inviteId);

        if (invite is null) return Result.Failure(DeleteInviteErrors.InviteNotFound);

        var currentUserId = _currentUserService.UserId;

        if (currentUserId != invite.InvitedByUserId) return Result.Failure(DeleteInviteErrors.AccessDenied);

        _projectInviteRepository.Delete(invite);
        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<IEnumerable<ProjectInvite>>> GetPendingInvitesForCurrentUser()
    {
        var currentUserId = _currentUserService.UserId;

        var pendingInvites = await _projectInviteRepository
            .GetPendingInvitesByInvitedUserIdAsync(currentUserId)
            .ToListAsync();

        return Result<IEnumerable<ProjectInvite>>.Success(pendingInvites);
    }

    public async Task<Result> AcceptInviteAsync(long inviteId)
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            return Result.Failure(AcceptInviteErrors.Unauthenticated);
        }

        var invite = await _projectInviteRepository.FindByIdAsync(inviteId);

        if (invite is null) return Result.Failure(AcceptInviteErrors.InviteNotFound);

        if (invite.Status == InviteStatus.Accepted) return Result.Failure(AcceptInviteErrors.InviteAlreadyAccepted);

        if (invite.Status == InviteStatus.Rejected) return Result.Failure(AcceptInviteErrors.InviteAlreadyRejected);

        if (invite.InvitedUserId != currentUserId) return Result.Failure(AcceptInviteErrors.AccessDenied);

        var isInvitedUserProjectMember =
            await _projectRepository.IsUserProjectMemberAsync(currentUserId, invite.ProjectId);

        if (isInvitedUserProjectMember) return Result.Failure(AcceptInviteErrors.InvitedUserAlreadyAMember);

        var project = await _projectRepository.FindByIdAsync(invite.ProjectId);

        if (project is null) return Result.Failure(AcceptInviteErrors.ProjectNotFound);

        _projectRepository.AddMember(project, currentUserId);
        invite.Status = InviteStatus.Accepted;
        _projectInviteRepository.Update(invite);
        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }

    public Task<Result> DeclineInviteAsync(long inviteId)
    {
        throw new NotImplementedException();
    }
}