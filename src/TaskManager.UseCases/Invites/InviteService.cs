using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.Infrastructure.Identity.User;
using TaskManager.UseCases.Invites.Accept;
using TaskManager.UseCases.Invites.Create;
using TaskManager.UseCases.Invites.Decline;
using TaskManager.UseCases.Invites.Delete;
using TaskManager.UseCases.Invites.GetPendingForProject;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites;

public class InviteService : IInviteService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly AppDbContext _dbContext;
    private readonly IProjectInviteRepository _projectInviteRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly UserManager<TaskManagerUser> _userManager;

    public InviteService(IProjectInviteRepository projectInviteRepository, IProjectRepository projectRepository,
        ICurrentUserService currentUserService, AppDbContext dbContext, UserManager<TaskManagerUser> userManager,
        IProjectMemberRepository projectMemberRepository)
    {
        _projectInviteRepository = projectInviteRepository;
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
        _dbContext = dbContext;
        _userManager = userManager;
        _projectMemberRepository = projectMemberRepository;
    }

    public async Task<Result<ProjectInvite>> CreateAsync(CreateInviteDto createInviteDto)
    {
        var project = await _projectRepository.FindByIdWithProjectMembersIncludedAsync(createInviteDto.ProjectId);

        if (project is null)
            return Result<ProjectInvite>.Failure(CreateInviteErrors.ProjectNotFound);

        var invitedByUserId = _currentUserService.UserId;

        if (invitedByUserId is null) return Result<ProjectInvite>.Failure(UseCaseErrors.Unauthenticated);

        var canCreateInvite = await IsUserProjectLeadOrManagerAsync(invitedByUserId, project);

        if (!canCreateInvite)
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

        if (currentUserId is null) return Result.Failure(UseCaseErrors.Unauthenticated);

        var canDeleteInvite = invite.InvitedByUserId == currentUserId;

        if (canDeleteInvite) return Result.Failure(DeleteInviteErrors.AccessDenied);

        _projectInviteRepository.Delete(invite);
        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<IEnumerable<ProjectInvite>>> GetPendingInvitesForCurrentUser()
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
            return Result<IEnumerable<ProjectInvite>>.Failure(UseCaseErrors.Unauthenticated);

        var pendingInvites = await _projectInviteRepository
            .GetPendingInvitesByInvitedUserIdAsync(currentUserId)
            .ToListAsync();

        return Result<IEnumerable<ProjectInvite>>.Success(pendingInvites);
    }

    public async Task<Result> AcceptInviteAsync(long inviteId)
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null) return Result.Failure(UseCaseErrors.Unauthenticated);

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

    public async Task<Result> DeclineInviteAsync(long inviteId)
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null) return Result.Failure(UseCaseErrors.Unauthenticated);

        var invite = await _projectInviteRepository.FindByIdAsync(inviteId);

        if (invite is null) return Result.Failure(DeclineInviteErrors.InviteNotFound);

        if (invite.Status == InviteStatus.Accepted) return Result.Failure(DeclineInviteErrors.InviteAlreadyAccepted);

        if (invite.Status == InviteStatus.Rejected) return Result.Failure(DeclineInviteErrors.InviteAlreadyRejected);

        if (invite.InvitedUserId != currentUserId) return Result.Failure(DeclineInviteErrors.AccessDenied);

        var project = await _projectRepository.FindByIdAsync(invite.ProjectId);

        if (project is null) return Result.Failure(DeclineInviteErrors.ProjectNotFound);

        invite.Status = InviteStatus.Rejected;
        _projectInviteRepository.Update(invite);
        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<IEnumerable<ProjectInvite>>> GetPendingProjectInvitesAsync(long projectId)
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null) return Result<IEnumerable<ProjectInvite>>.Failure(UseCaseErrors.Unauthenticated);

        var project = await _projectRepository.FindByIdWithInvitesIncludedAsync(projectId);

        if (project is null)
            return Result<IEnumerable<ProjectInvite>>.Failure(GetPendingInvitesForProjectErrors.ProjectNotFound);

        var currentUser = await _userManager.FindByIdAsync(currentUserId);

        if (currentUser is null) return Result<IEnumerable<ProjectInvite>>.Failure(UseCaseErrors.Unauthenticated);

        var invites = project.Invites;

        return Result<IEnumerable<ProjectInvite>>.Success(invites);
    }

    private async Task<bool> IsUserProjectLeadOrManagerAsync(string userId, ProjectEntity project)
    {
        var projectMember = await _projectMemberRepository.GetByProjectIdAndMemberIdAsync(project.Id, userId);

        var isUserProjectManager = projectMember is not null && projectMember.ProjectRole == ProjectRole.Manager;

        return userId == project.LeadUserId || isUserProjectManager;
    }
}