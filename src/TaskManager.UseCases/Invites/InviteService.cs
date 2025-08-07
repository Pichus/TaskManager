using Microsoft.AspNetCore.Identity;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.Infrastructure.Identity.User;
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
        var invitedByUserId = _currentUserService.UserId;

        var invitedUser = await _userManager.FindByIdAsync(createInviteDto.InvitedUserId);

        if (invitedUser is null)
            return Result<ProjectInvite>.Failure(CreateInviteErrors.InvitedUserNotFound);

        var project = await _projectRepository.FindByIdAsync(createInviteDto.ProjectId);

        if (project is null)
            return Result<ProjectInvite>.Failure(CreateInviteErrors.ProjectNotFound);

        var inviteExists = await _projectInviteRepository
            .AnyAsync(projectInvite => projectInvite.InvitedUserId == invitedUser.Id
                                       && projectInvite.ProjectId == project.Id);

        if (inviteExists)
            return Result<ProjectInvite>.Failure(CreateInviteErrors.UserAlreadyInvited);

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

        if (invite is null)
        {
            return Result.Failure(DeleteInviteErrors.InviteNotFound);
        }

        var currentUserId = _currentUserService.UserId;

        if (currentUserId != invite.InvitedByUserId)
        {
            return Result.Failure(DeleteInviteErrors.AccessDenied);
        }
        
        _projectInviteRepository.Delete(invite);
        await _dbContext.SaveChangesAsync();
        
        return Result.Success();
    }
}