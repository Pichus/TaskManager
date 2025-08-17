using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.Infrastructure.Identity.User;
using TaskManager.UseCases.ProjectMembers;
using TaskManager.UseCases.ProjectMembers.Get;
using TaskManager.UseCases.ProjectMembers.Update;

namespace TaskManager.UnitTests.ProjectMembers;

public class ProjectMemberServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<ProjectMemberService>> _loggerMock;
    private readonly Mock<IProjectMemberRepository> _projectMemberRepositoryMock;
    private readonly ProjectMemberService _projectMemberService;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<UserManager<TaskManagerUser>> _userManagerMock;

    public ProjectMemberServiceTests()
    {
        _projectMemberRepositoryMock = new Mock<IProjectMemberRepository>();
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _loggerMock = new Mock<ILogger<ProjectMemberService>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(work => work.SaveChangesAsync(CancellationToken.None))
            .ReturnsAsync(1);

        var userStoreMock = new Mock<IUserStore<TaskManagerUser>>();
        _userManagerMock =
            new Mock<UserManager<TaskManagerUser>>(userStoreMock.Object, null, null, null, null, null, null, null,
                null);

        _projectMemberService = new ProjectMemberService(
            _currentUserServiceMock.Object,
            _projectRepositoryMock.Object,
            _projectMemberRepositoryMock.Object,
            _userManagerMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GetProjectMembersAsync_WhenProjectId_IsNonExistent_ReturnsFailure()
    {
        var currentUserId = "some valid id";
        long projectId = 0;

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock.Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync((ProjectEntity?)null);

        var result = await _projectMemberService.GetProjectMembersAsync(projectId);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(GetProjectMembersErrors.ProjectNotFound.Code);
    }
    
    [Fact]
    public async Task GetProjectMembersAsync_WhenCurrentUser_IsNotAProjectParticipant_ReturnsFailure()
    {
        var currentUserId = "some valid id";
        long projectId = 1;

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity());
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectParticipantAsync(currentUserId, projectId))
            .ReturnsAsync(false);

        var result = await _projectMemberService.GetProjectMembersAsync(projectId);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(GetProjectMembersErrors.AccessDenied.Code);
    }
    
    [Fact]
    public async Task GetProjectMembersAsync_WhenAllConditions_AreMet_ReturnsSuccess()
    {
        var currentUserId = "some valid id";
        long projectId = 1;

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity());
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectParticipantAsync(currentUserId, projectId))
            .ReturnsAsync(true);

        var result = await _projectMemberService.GetProjectMembersAsync(projectId);

        result.IsSuccess.Should().Be(true);
    }
    
    [Fact]
    public async Task UpdateProjectMemberAsync_WhenProjectMember_IsNonExistent_ReturnsFailure()
    {
        var currentUserId = "some valid id";
        var memberId = "some invalid id";
        var projectRole = ProjectRole.Manager;
        long projectId = 1;

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _userManagerMock
            .Setup(manager => manager.FindByIdAsync(memberId))
            .ReturnsAsync((TaskManagerUser?)null);

        var result = await _projectMemberService.UpdateProjectMemberAsync(projectId, memberId, projectRole);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(UpdateProjectMemberErrors.MemberNotFound.Code);
    }
    
    [Fact]
    public async Task UpdateProjectMemberAsync_WhenProject_IsNonExistent_ReturnsFailure()
    {
        var currentUserId = "some valid id";
        var memberId = "some valid id";
        var projectRole = ProjectRole.Manager;
        long projectId = 1;

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _userManagerMock
            .Setup(manager => manager.FindByIdAsync(memberId))
            .ReturnsAsync(new TaskManagerUser());
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync((ProjectEntity?)null);

        var result = await _projectMemberService.UpdateProjectMemberAsync(projectId, memberId, projectRole);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(UpdateProjectMemberErrors.ProjectNotFound.Code);
    }
    
    [Fact]
    public async Task UpdateProjectMemberAsync_WhenCurrentUser_IsNotAProjectLead_ReturnsFailure()
    {
        var currentUserId = "some valid id";
        var memberId = "some valid id";
        var projectRole = ProjectRole.Manager;
        long projectId = 1;

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _userManagerMock
            .Setup(manager => manager.FindByIdAsync(memberId))
            .ReturnsAsync(new TaskManagerUser(){});
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                LeadUserId = "random id",
            });

        var result = await _projectMemberService.UpdateProjectMemberAsync(projectId, memberId, projectRole);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(UpdateProjectMemberErrors.AccessDenied.Code);
    }
    
    [Fact]
    public async Task UpdateProjectMemberAsync_WhenMemberUser_IsNotAProjectMember_ReturnsFailure()
    {
        var currentUserId = "some valid id";
        var memberId = "some valid id";
        var projectRole = ProjectRole.Manager;
        long projectId = 1;

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _userManagerMock
            .Setup(manager => manager.FindByIdAsync(memberId))
            .ReturnsAsync(new TaskManagerUser(){});
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                LeadUserId = currentUserId,
            });
        _projectMemberRepositoryMock
            .Setup(repository => repository.GetByProjectIdAndMemberIdAsync(projectId, memberId))
            .ReturnsAsync((ProjectMember?)null);

        var result = await _projectMemberService.UpdateProjectMemberAsync(projectId, memberId, projectRole);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(UpdateProjectMemberErrors.UserIsNotAProjectMember.Code);
    }
    
    [Fact]
    public async Task UpdateProjectMemberAsync_WhenSettingTheSameRole_ReturnsFailure()
    {
        var currentUserId = "some valid id";
        var memberId = "some valid id";
        var projectRole = ProjectRole.Manager;
        long projectId = 1;

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _userManagerMock
            .Setup(manager => manager.FindByIdAsync(memberId))
            .ReturnsAsync(new TaskManagerUser(){});
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                LeadUserId = currentUserId,
            });
        _projectMemberRepositoryMock
            .Setup(repository => repository.GetByProjectIdAndMemberIdAsync(projectId, memberId))
            .ReturnsAsync(new ProjectMember
            {
                ProjectRole = ProjectRole.Manager,
            });

        var result = await _projectMemberService.UpdateProjectMemberAsync(projectId, memberId, projectRole);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(UpdateProjectMemberErrors.MemberAlreadyHasThisRole.Code);
    }
    
    [Fact]
    public async Task UpdateProjectMemberAsync_WhenAllConditions_AreMet_ReturnsSuccess()
    {
        var currentUserId = "some valid id";
        var memberId = "some valid id";
        var projectRole = ProjectRole.Member;
        long projectId = 1;

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _userManagerMock
            .Setup(manager => manager.FindByIdAsync(memberId))
            .ReturnsAsync(new TaskManagerUser(){});
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                LeadUserId = currentUserId,
            });
        _projectMemberRepositoryMock
            .Setup(repository => repository.GetByProjectIdAndMemberIdAsync(projectId, memberId))
            .ReturnsAsync(new ProjectMember
            {
                ProjectRole = ProjectRole.Manager,
            });

        var result = await _projectMemberService.UpdateProjectMemberAsync(projectId, memberId, projectRole);

        result.IsSuccess.Should().Be(true);
    }
}