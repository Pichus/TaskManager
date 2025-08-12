using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger _logger;
    private readonly IProjectInviteRepository _projectInviteRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly UserManager<TaskManagerUser> _userManager;

    public InviteService(IProjectInviteRepository projectInviteRepository, IProjectRepository projectRepository,
        ICurrentUserService currentUserService, AppDbContext dbContext, UserManager<TaskManagerUser> userManager,
        IProjectMemberRepository projectMemberRepository, ILogger logger)
    {
        _projectInviteRepository = projectInviteRepository;
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
        _dbContext = dbContext;
        _userManager = userManager;
        _projectMemberRepository = projectMemberRepository;
        _logger = logger;
    }

    public async Task<Result<ProjectInvite>> CreateAsync(CreateInviteDto createInviteDto)
    {
        _logger.LogInformation("Creating invite");

        var invitedByUserId = _currentUserService.UserId;

        if (invitedByUserId is null)
        {
            _logger.LogWarning("Creating invite failed - user unauthenticated");
            return Result<ProjectInvite>.Failure(UseCaseErrors.Unauthenticated);
        }

        var project = await _projectRepository.FindByIdWithProjectMembersIncludedAsync(createInviteDto.ProjectId);

        if (project is null)
        {
            _logger.LogWarning("Creating invite failed - Project: {ProjectId} not found", createInviteDto.ProjectId);
            return Result<ProjectInvite>.Failure(CreateInviteErrors.ProjectNotFound);
        }

        var canCreateInvite = await IsUserProjectLeadOrManagerAsync(invitedByUserId, project);

        if (!canCreateInvite)
        {
            _logger.LogWarning("Creating invite failed - access denied");
            return Result<ProjectInvite>.Failure(CreateInviteErrors.AccessDenied);
        }

        var invitedUser = await _userManager.FindByIdAsync(createInviteDto.InvitedUserId);

        if (invitedUser is null)
        {
            _logger.LogWarning("Creating invite failed - invited User: {UserId} not found",
                createInviteDto.InvitedUserId);
            return Result<ProjectInvite>.Failure(CreateInviteErrors.InvitedUserNotFound);
        }

        var inviteExists = await _projectInviteRepository
            .AnyAsync(projectInvite => projectInvite.InvitedUserId == invitedUser.Id
                                       && projectInvite.ProjectId == project.Id);

        if (inviteExists)
        {
            _logger.LogWarning("Creating invite failed - User: {UserId} already invited",
                createInviteDto.InvitedUserId);
            return Result<ProjectInvite>.Failure(CreateInviteErrors.UserAlreadyInvited);
        }

        var isInvitedUserProjectMember = project.Members.Any(member => member.MemberId == invitedUser.Id);

        if (isInvitedUserProjectMember)
        {
            _logger.LogWarning("Creating invite failed - User: {UserId} is already a member",
                createInviteDto.InvitedUserId);
            return Result<ProjectInvite>.Failure(CreateInviteErrors.InvitedUserAlreadyAMember);
        }

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

        _logger.LogInformation("Created invite successfully");
        return Result<ProjectInvite>.Success(invite);
    }

    public async Task<Result> DeleteAsync(long inviteId)
    {
        _logger.LogInformation("Deleting Invite: {InvitedId}", inviteId);

        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogWarning("Deleting invite failed - user unauthenticated");
            return Result.Failure(UseCaseErrors.Unauthenticated);
        }
        
        var invite = await _projectInviteRepository.FindByIdAsync(inviteId);

        if (invite is null)
        {
            _logger.LogWarning("Deleting invite failed - invite not found");
            return Result.Failure(DeleteInviteErrors.InviteNotFound);
        }

        var canDeleteInvite = invite.InvitedByUserId == currentUserId;

        if (canDeleteInvite)
        {
            _logger.LogWarning("Deleting invite failed - access denied");
            return Result.Failure(DeleteInviteErrors.AccessDenied);
        }

        _projectInviteRepository.Delete(invite);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Deleted invite successfully");
        return Result.Success();
    }

    public async Task<Result<IEnumerable<ProjectInvite>>> GetPendingInvitesForCurrentUser()
    {
        _logger.LogInformation("Getting pending invites for current user");
        
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogWarning("Getting pending invites for current user failed - user unauthenticated");
            return Result<IEnumerable<ProjectInvite>>.Failure(UseCaseErrors.Unauthenticated);
        }

        var pendingInvites = await _projectInviteRepository
            .GetPendingInvitesByInvitedUserIdAsync(currentUserId)
            .ToListAsync();

        _logger.LogInformation("Got pending invites for current user successfully");
        return Result<IEnumerable<ProjectInvite>>.Success(pendingInvites);
    }

    public async Task<Result> AcceptInviteAsync(long inviteId)
    {
        _logger.LogInformation("Accepting Invite: {InviteId}", inviteId);
        
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogWarning("Accepting invite failed - user unauthenticated");
            return Result.Failure(UseCaseErrors.Unauthenticated);
        }

        var invite = await _projectInviteRepository.FindByIdAsync(inviteId);

        if (invite is null)
        {
            _logger.LogWarning("Accepting invite failed - invite not found");
            return Result.Failure(AcceptInviteErrors.InviteNotFound);
        }

        if (invite.Status == InviteStatus.Accepted)
        {
            _logger.LogWarning("Accepting invite failed - invite already accepted");
            return Result.Failure(AcceptInviteErrors.InviteAlreadyAccepted);
        }

        if (invite.Status == InviteStatus.Rejected)
        {
            _logger.LogWarning("Accepting invite failed - invite already rejected");   
            return Result.Failure(AcceptInviteErrors.InviteAlreadyRejected);
        }

        if (invite.InvitedUserId != currentUserId)
        {
            _logger.LogWarning("Accepting invite failed - access denied");
            return Result.Failure(AcceptInviteErrors.AccessDenied);
        }

        var isInvitedUserProjectMember =
            await _projectRepository.IsUserProjectMemberAsync(currentUserId, invite.ProjectId);

        if (isInvitedUserProjectMember)
        {
            _logger.LogWarning("Accepting invite failed - invited user is already a member");
            return Result.Failure(AcceptInviteErrors.InvitedUserAlreadyAMember);
        }

        var project = await _projectRepository.FindByIdAsync(invite.ProjectId);

        if (project is null)
        {
            _logger.LogWarning("Accepting invite failed - project not found");
            return Result.Failure(AcceptInviteErrors.ProjectNotFound);
        }

        _projectRepository.AddMember(project, currentUserId);
        invite.Status = InviteStatus.Accepted;
        _projectInviteRepository.Update(invite);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Accepted invite successfully");
        return Result.Success();
    }

    public async Task<Result> DeclineInviteAsync(long inviteId)
    {
        _logger.LogInformation("Declining Invite: {InviteId}", inviteId);
        
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogWarning("Declining invite failed - user unauthenticated");
            return Result.Failure(UseCaseErrors.Unauthenticated);
        }

        var invite = await _projectInviteRepository.FindByIdAsync(inviteId);

        if (invite is null)
        {
            _logger.LogWarning("Declining invite failed - invite not found");
            return Result.Failure(DeclineInviteErrors.InviteNotFound);
        }

        if (invite.Status == InviteStatus.Accepted)
        {
            _logger.LogWarning("Declining invite failed - invite already accepted");
            return Result.Failure(DeclineInviteErrors.InviteAlreadyAccepted);
        }

        if (invite.Status == InviteStatus.Rejected)
        {
            _logger.LogWarning("Declining invite failed - invite already rejected");
            return Result.Failure(DeclineInviteErrors.InviteAlreadyRejected);
        }

        if (invite.InvitedUserId != currentUserId)
        {
            _logger.LogWarning("Declining invite failed - access denied");
            return Result.Failure(DeclineInviteErrors.AccessDenied);
        }

        var project = await _projectRepository.FindByIdAsync(invite.ProjectId);

        if (project is null)
        {
            _logger.LogWarning("Declining invite failed - Project: {ProjectId} not found", invite.ProjectId);
            return Result.Failure(DeclineInviteErrors.ProjectNotFound);
        }

        invite.Status = InviteStatus.Rejected;
        _projectInviteRepository.Update(invite);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Declined invite successfully");
        return Result.Success();
    }

    public async Task<Result<IEnumerable<ProjectInvite>>> GetPendingProjectInvitesAsync(long projectId)
    {
        _logger.LogInformation("Getting pending invites for Project: {ProjectId}", projectId);
        
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogWarning("Getting pending invites for project failed - user unauthenticated");
            return Result<IEnumerable<ProjectInvite>>.Failure(UseCaseErrors.Unauthenticated);
        }

        var project = await _projectRepository.FindByIdWithInvitesIncludedAsync(projectId);

        if (project is null)
        {
            _logger.LogWarning("Getting pending invites for project failed - project not found");
            return Result<IEnumerable<ProjectInvite>>.Failure(GetPendingInvitesForProjectErrors.ProjectNotFound);
        }

        var invites = project.Invites;

        _logger.LogWarning("Got pending invites for project successfully");
        return Result<IEnumerable<ProjectInvite>>.Success(invites);
    }

    private async Task<bool> IsUserProjectLeadOrManagerAsync(string userId, ProjectEntity project)
    {
        var projectMember = await _projectMemberRepository.GetByProjectIdAndMemberIdAsync(project.Id, userId);

        var isUserProjectManager = projectMember is not null && projectMember.ProjectRole == ProjectRole.Manager;

        return userId == project.LeadUserId || isUserProjectManager;
    }
}