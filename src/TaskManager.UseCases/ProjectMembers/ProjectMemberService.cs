using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.Infrastructure.Identity.User;
using TaskManager.UseCases.ProjectMembers.Delete;
using TaskManager.UseCases.ProjectMembers.Get;
using TaskManager.UseCases.ProjectMembers.Update;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.ProjectMembers;

public class ProjectMemberService : IProjectMemberService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ProjectMemberService> _logger;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<TaskManagerUser> _userManager;

    public ProjectMemberService(ICurrentUserService currentUserService, IProjectRepository projectRepository,
        IProjectMemberRepository projectMemberRepository, UserManager<TaskManagerUser> userManager,
        IUnitOfWork unitOfWork, ILogger<ProjectMemberService> logger)
    {
        _currentUserService = currentUserService;
        _projectRepository = projectRepository;
        _projectMemberRepository = projectMemberRepository;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ProjectMemberWithUser>>> GetProjectMembersAsync(long projectId)
    {
        _logger.LogInformation("Getting Project: {ProjectId} members", projectId);

        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogInformation("Getting project members failed - user unauthenticated");
            return Result<IEnumerable<ProjectMemberWithUser>>.Failure(UseCaseErrors.Unauthenticated);
        }

        var project = await _projectRepository.FindByIdAsync(projectId);

        if (project is null)
        {
            _logger.LogInformation("Getting project members failed - project not found");
            return Result<IEnumerable<ProjectMemberWithUser>>.Failure(GetProjectMembersErrors.ProjectNotFound);
        }

        var canGetProjectMembers =
            await _projectMemberRepository.IsUserProjectParticipantAsync(currentUserId, projectId);

        if (!canGetProjectMembers)
        {
            _logger.LogInformation("Getting project members failed - access denied");
            return Result<IEnumerable<ProjectMemberWithUser>>.Failure(GetProjectMembersErrors.AccessDenied);
        }

        var projectMembers =
            await _projectMemberRepository.GetProjectMembersWithUsersAsync(projectId);

        _logger.LogInformation("Got project members successfully");
        return Result<IEnumerable<ProjectMemberWithUser>>.Success(projectMembers);
    }

    public async Task<Result> UpdateProjectMemberAsync(long projectId, string memberId, ProjectRole projectRole)
    {
        _logger.LogInformation("Updating Project: {ProjectId} Member: {MemberId}", projectId, memberId);

        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogInformation("Updating project member failed - user unauthenticated");
            return Result.Failure(UseCaseErrors.Unauthenticated);
        }

        var memberUser = await _userManager.FindByIdAsync(memberId);

        if (memberUser is null)
        {
            _logger.LogInformation("Updating project member failed - member not found");
            return Result.Failure(UpdateProjectMemberErrors.MemberNotFound);
        }

        var project = await _projectRepository.FindByIdAsync(projectId);

        if (project is null)
        {
            _logger.LogInformation("Updating project member failed - project not found");
            return Result.Failure(UpdateProjectMemberErrors.ProjectNotFound);
        }

        var isCurrentUserProjectLead = currentUserId == project.LeadUserId;

        if (!isCurrentUserProjectLead)
        {
            _logger.LogInformation("Updating project member failed - access denied");
            return Result.Failure(UpdateProjectMemberErrors.AccessDenied);
        }

        var projectMember = await _projectMemberRepository.GetByProjectIdAndMemberIdAsync(projectId, memberId);

        var isUserProjectMember = projectMember is not null;

        if (!isUserProjectMember)
        {
            _logger.LogInformation("Updating project member failed - user is not a member");
            return Result.Failure(UpdateProjectMemberErrors.UserIsNotAProjectMember);
        }

        if (projectMember.ProjectRole == projectRole)
        {
            _logger.LogInformation("Updating project member failed - member already has this role");
            return Result.Failure(UpdateProjectMemberErrors.MemberAlreadyHasThisRole);
        }

        projectMember.ProjectRole = projectRole;
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated project member successfully");
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(long projectId, string memberId)
    {
        _logger.LogInformation("Deleting Project: {ProjectId} Member: {MemberId}", projectId, memberId);

        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogInformation("Deleting project member failed - user unauthenticated");
            return Result.Failure(UseCaseErrors.Unauthenticated);
        }

        var project = await _projectRepository.FindByIdAsync(projectId);

        if (project is null)
        {
            _logger.LogInformation("Deleting project member failed - project not found");
            return Result.Failure(DeleteProjectMemberErrors.ProjectNotFound);
        }

        var isCurrentUserProjectLead = project.LeadUserId == currentUserId;

        if (!isCurrentUserProjectLead)
        {
            _logger.LogInformation("Deleting project member failed - access denied");
            return Result.Failure(DeleteProjectMemberErrors.AccessDenied);
        }

        var projectMember = await _projectMemberRepository.GetByProjectIdAndMemberIdAsync(projectId, memberId);

        var isUserProjectMember = projectMember is not null;

        if (!isUserProjectMember)
        {
            _logger.LogInformation("Deleting project member failed - member not found");
            return Result.Failure(DeleteProjectMemberErrors.ProjectMemberNotFound);
        }

        _projectMemberRepository.Remove(projectMember!);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Deleted project member successfully");
        return Result.Success();
    }
}