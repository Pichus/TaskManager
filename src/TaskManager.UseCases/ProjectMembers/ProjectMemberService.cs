using Microsoft.AspNetCore.Identity;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Infrastructure.Data;
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
    private readonly AppDbContext _dbContext;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly UserManager<TaskManagerUser> _userManager;

    public ProjectMemberService(ICurrentUserService currentUserService, IProjectRepository projectRepository,
        IProjectMemberRepository projectMemberRepository, UserManager<TaskManagerUser> userManager,
        AppDbContext dbContext)
    {
        _currentUserService = currentUserService;
        _projectRepository = projectRepository;
        _projectMemberRepository = projectMemberRepository;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<Result<IEnumerable<ProjectMemberWithUser>>> GetProjectMembersAsync(long projectId)
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
            return Result<IEnumerable<ProjectMemberWithUser>>.Failure(UseCaseErrors.Unauthenticated);

        var project = await _projectRepository.FindByIdAsync(projectId);

        if (project is null)
            return Result<IEnumerable<ProjectMemberWithUser>>.Failure(GetProjectMembersErrors.ProjectNotFound);

        var canGetProjectMembers = project.LeadUserId == currentUserId ||
                                   await _projectMemberRepository.IsUserProjectMember(currentUserId, projectId);

        if (!canGetProjectMembers)
            return Result<IEnumerable<ProjectMemberWithUser>>.Failure(GetProjectMembersErrors.AccessDenied);

        var projectMembers =
            await _projectMemberRepository.GetProjectMembersWithUsersAsync(projectId);

        return Result<IEnumerable<ProjectMemberWithUser>>.Success(projectMembers);
    }

    public async Task<Result> UpdateProjectMemberAsync(long projectId, string memberId, ProjectRole projectRole)
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
            return Result.Failure(UseCaseErrors.Unauthenticated);

        var memberUser = await _userManager.FindByIdAsync(memberId);

        if (memberUser is null)
            return Result.Failure(UpdateProjectMemberErrors.MemberNotFound);

        var project = await _projectRepository.FindByIdAsync(projectId);

        if (project is null)
            return Result.Failure(UpdateProjectMemberErrors.ProjectNotFound);

        var isCurrentUserProjectLead = currentUserId == project.LeadUserId;

        if (!isCurrentUserProjectLead)
            return Result.Failure(UpdateProjectMemberErrors.AccessDenied);

        var projectMember = await _projectMemberRepository.GetByProjectIdAndMemberIdAsync(projectId, memberId);

        var isUserProjectMember = projectMember is not null;

        if (!isUserProjectMember)
            return Result.Failure(UpdateProjectMemberErrors.UserIsNotAProjectMember);

        if (projectMember.ProjectRole == projectRole)
            return Result.Failure(UpdateProjectMemberErrors.MemberAlreadyHasThisRole);

        projectMember.ProjectRole = projectRole;
        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(long projectId, string memberId)
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
            return Result.Failure(UseCaseErrors.Unauthenticated);

        var project = await _projectRepository.FindByIdAsync(projectId);

        if (project is null)
            return Result.Failure(DeleteProjectMemberErrors.ProjectNotFound);

        var isCurrentUserProjectLead = project.LeadUserId == currentUserId;

        if (!isCurrentUserProjectLead)
            return Result.Failure(DeleteProjectMemberErrors.AccessDenied);

        var projectMember = await _projectMemberRepository.GetByProjectIdAndMemberIdAsync(projectId, memberId);

        var isUserProjectMember = projectMember is not null;

        if (!isUserProjectMember)
            return Result.Failure(DeleteProjectMemberErrors.ProjectMemberNotFound);

        _projectMemberRepository.Delete(projectMember);
        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }
}