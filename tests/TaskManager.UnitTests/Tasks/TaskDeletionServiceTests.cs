using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.TaskAggregate;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.UseCases.Tasks.Delete;

namespace TaskManager.UnitTests.Tasks;

public class TaskDeletionServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IProjectMemberRepository> _projectMemberRepositoryMock;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly TaskDeletionService _taskDeletionService;
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public TaskDeletionServiceTests()
    {
        _projectMemberRepositoryMock = new Mock<IProjectMemberRepository>();
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _loggerMock = new Mock<ILogger>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(CancellationToken.None))
            .ReturnsAsync(1);

        _taskDeletionService = new TaskDeletionService(
            _loggerMock.Object,
            _currentUserServiceMock.Object,
            _projectRepositoryMock.Object,
            _taskRepositoryMock.Object,
            _projectMemberRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task DeleteAsync_WhenProject_IsNonExistent_ReturnsFailure()
    {
        long projectId = 0;
        long taskId = 0;
        var currentUserId = "some valid id";

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync((ProjectEntity?)null);

        var result = await _taskDeletionService.DeleteAsync(projectId, taskId);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(DeleteTaskErrors.ProjectNotFound.Code);
    }

    [Fact]
    public async Task DeleteAsync_WhenTask_IsNonExistent_ReturnsFailure()
    {
        long projectId = 1;
        long taskId = 0;
        var currentUserId = "some valid id";

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity());
        _taskRepositoryMock
            .Setup(repository => repository.FindByIdAsync(taskId))
            .ReturnsAsync((TaskEntity?)null);

        var result = await _taskDeletionService.DeleteAsync(projectId, taskId);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(DeleteTaskErrors.TaskNotFound.Code);
    }

    [Fact]
    public async Task DeleteAsync_WhenTask_DoesNotBelongToTheProject_ReturnsFailure()
    {
        long projectId = 1;
        long taskId = 1;
        var currentUserId = "some valid id";

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                Id = projectId
            });
        _taskRepositoryMock
            .Setup(repository => repository.FindByIdAsync(taskId))
            .ReturnsAsync(new TaskEntity
            {
                Id = taskId,
                ProjectId = 999
            });

        var result = await _taskDeletionService.DeleteAsync(projectId, taskId);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(DeleteTaskErrors.TaskNotFound.Code);
    }

    [Fact]
    public async Task DeleteAsync_WhenCurrentUser_IsNotAProjectLeadOrManager_ReturnsFailure()
    {
        long projectId = 1;
        long taskId = 1;
        var currentUserId = "some valid id";

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                Id = projectId
            });
        _taskRepositoryMock
            .Setup(repository => repository.FindByIdAsync(taskId))
            .ReturnsAsync(new TaskEntity
            {
                Id = taskId,
                ProjectId = projectId
            });
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectManagerAsync(currentUserId, projectId))
            .ReturnsAsync(false);
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectLeadAsync(currentUserId, projectId))
            .ReturnsAsync(false);

        var result = await _taskDeletionService.DeleteAsync(projectId, taskId);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(DeleteTaskErrors.AccessDenied.Code);
    }

    [Fact]
    public async Task DeleteAsync_WhenCurrentUser_IsAProjectLead_ReturnsSuccess()
    {
        long projectId = 1;
        long taskId = 1;
        var currentUserId = "some valid id";

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                Id = projectId
            });
        _taskRepositoryMock
            .Setup(repository => repository.FindByIdAsync(taskId))
            .ReturnsAsync(new TaskEntity
            {
                Id = taskId,
                ProjectId = projectId
            });
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectManagerAsync(currentUserId, projectId))
            .ReturnsAsync(false);
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectLeadAsync(currentUserId, projectId))
            .ReturnsAsync(true);

        var result = await _taskDeletionService.DeleteAsync(projectId, taskId);

        result.IsSuccess.Should().Be(true);
    }

    [Fact]
    public async Task DeleteAsync_WhenCurrentUser_IsAProjectManager_ReturnsSuccess()
    {
        long projectId = 1;
        long taskId = 1;
        var currentUserId = "some valid id";

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                Id = projectId
            });
        _taskRepositoryMock
            .Setup(repository => repository.FindByIdAsync(taskId))
            .ReturnsAsync(new TaskEntity
            {
                Id = taskId,
                ProjectId = projectId
            });
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectManagerAsync(currentUserId, projectId))
            .ReturnsAsync(true);
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectLeadAsync(currentUserId, projectId))
            .ReturnsAsync(false);

        var result = await _taskDeletionService.DeleteAsync(projectId, taskId);

        result.IsSuccess.Should().Be(true);
    }
}