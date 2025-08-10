using TaskManager.Core.ProjectAggregate;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.UseCases.ProjectMembers.Get;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.ProjectMembers;

public class ProjectMemberService : IProjectMemberService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IProjectRepository _projectRepository;

    public ProjectMemberService(ICurrentUserService currentUserService, IProjectRepository projectRepository,
        IProjectMemberRepository projectMemberRepository)
    {
        _currentUserService = currentUserService;
        _projectRepository = projectRepository;
        _projectMemberRepository = projectMemberRepository;
    }

    public async Task<Result<IEnumerable<ProjectMemberWithUser>>> GetProjectMembersAsync(long projectId)
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
            return Result<IEnumerable<ProjectMemberWithUser>>.Failure(UseCaseErrors.Unauthenticated);

        var project = await _projectRepository.FindByIdAsync(projectId);

        if (project is null)
            return Result<IEnumerable<ProjectMemberWithUser>>.Failure(GetProjectMembersErrors.ProjectNotFound);

        var canGetProjectMembers = await _projectMemberRepository.IsUserProjectMember(currentUserId, projectId);

        if (!canGetProjectMembers)
            return Result<IEnumerable<ProjectMemberWithUser>>.Failure(GetProjectMembersErrors.AccessDenied);

        var projectMembers =
            await _projectMemberRepository.GetProjectMembersWithUsersAsync(projectId);

        return Result<IEnumerable<ProjectMemberWithUser>>.Success(projectMembers);
    }
}