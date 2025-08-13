using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.Infrastructure.Identity.User;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Invites.Create;

public class InviteCreationService : IInviteCreationService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly AppDbContext _dbContext;
    private readonly ILogger _logger;
    private readonly IProjectInviteRepository _projectInviteRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly UserManager<TaskManagerUser> _userManager;

    public InviteCreationService(ILogger logger, ICurrentUserService currentUserService,
        IProjectRepository projectRepository, UserManager<TaskManagerUser> userManager,
        IProjectInviteRepository projectInviteRepository, AppDbContext dbContext,
        IProjectMemberRepository projectMemberRepository)
    {
        _logger = logger;
        _currentUserService = currentUserService;
        _projectRepository = projectRepository;
        _userManager = userManager;
        _projectInviteRepository = projectInviteRepository;
        _dbContext = dbContext;
        _projectMemberRepository = projectMemberRepository;
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

        var canCreateInvite =
            await _projectMemberRepository.IsUserProjectLeadAsync(invitedByUserId, createInviteDto.ProjectId) ||
            await _projectMemberRepository.IsUserProjectManagerAsync(invitedByUserId, createInviteDto.ProjectId);

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

        var inviteExists = await _projectInviteRepository.InviteExistsAsync(invitedUser.Id, project.Id);

        if (inviteExists)
        {
            _logger.LogWarning("Creating invite failed - User: {UserId} already invited",
                createInviteDto.InvitedUserId);
            return Result<ProjectInvite>.Failure(CreateInviteErrors.UserAlreadyInvited);
        }

        var isInvitedUserProjectParticipant =
            await _projectMemberRepository.IsUserProjectParticipantAsync(invitedUser.Id, project.Id);

        if (isInvitedUserProjectParticipant)
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
}