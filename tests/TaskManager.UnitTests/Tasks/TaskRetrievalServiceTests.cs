using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.TaskAggregate;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.UseCases.Tasks.Delete;
using TaskManager.UseCases.Tasks.Retrieve;

namespace TaskManager.UnitTests.Tasks;

public class TaskRetrievalServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<TaskRetrievalService>> _loggerMock;
    private readonly Mock<IProjectMemberRepository> _projectMemberRepositoryMock;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly TaskRetrievalService _taskRetrievalService;
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public TaskRetrievalServiceTests()
    {
        _projectMemberRepositoryMock = new Mock<IProjectMemberRepository>();
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _loggerMock = new Mock<ILogger<TaskRetrievalService>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(CancellationToken.None))
            .ReturnsAsync(1);

        _taskRetrievalService = new TaskRetrievalService(
            _loggerMock.Object,
            _currentUserServiceMock.Object,
            _projectRepositoryMock.Object, 
            _projectMemberRepositoryMock.Object,
            _taskRepositoryMock.Object
        );
    }

    [Fact]
    public async Task RetrieveAllByProjectIdAndStatusAsync_WhenProjectIsNonExistent_ReturnsFailure()
    {
        long projectId = 0;
        var status = StatusDto.Any;
        var currentUserId = "some valid id";

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync((ProjectEntity?)null);

        var result = await _taskRetrievalService.RetrieveAllByProjectIdAndStatusAsync(projectId, status);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(RetrieveTaskErrors.ProjectNotFound.Code);
    }
    
    [Fact]
    public async Task RetrieveAllByProjectIdAndStatusAsync_WhenCurrentUser_IsNotAProjectParticipant_ReturnsFailure()
    {
        long projectId = 0;
        var status = StatusDto.Any;
        var currentUserId = "some valid id";

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity());
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectParticipantAsync(currentUserId, projectId))
            .ReturnsAsync(false);

        var result = await _taskRetrievalService.RetrieveAllByProjectIdAndStatusAsync(projectId, status);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(RetrieveTaskErrors.AccessDenied.Code);
    }

    [Fact]
    public async Task RetrieveAllByProjectIdAndStatusAsync_WhenCurrentUser_IsAProjectParticipant_ReturnsSuccess()
    {
        long projectId = 0;
        var status = StatusDto.Any;
        var currentUserId = "some valid id";

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity());
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectParticipantAsync(currentUserId, projectId))
            .ReturnsAsync(true);

        var result = await _taskRetrievalService.RetrieveAllByProjectIdAndStatusAsync(projectId, status);

        result.IsSuccess.Should().Be(true);
    }

    [Fact]
    public async Task RetrieveByProjectIdAndTaskIdAsync_WhenProjectIsNonExistent_ReturnsFailure()
    {
        long projectId = 0;
        var currentUserId = "some valid id";
        long taskId = 1;

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync((ProjectEntity?)null);

        var result = await _taskRetrievalService.RetrieveByProjectIdAndTaskIdAsync(projectId, taskId);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(RetrieveTaskErrors.ProjectNotFound.Code);
    }
    
    [Fact]
    public async Task RetrieveByProjectIdAndTaskIdAsync_WhenCurrentUser_IsNotAProjectParticipant_ReturnsFailure()
    {
        long projectId = 0;
        var currentUserId = "some valid id";
        long taskId = 1;

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity());
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectParticipantAsync(currentUserId, projectId))
            .ReturnsAsync(false);

        var result = await _taskRetrievalService.RetrieveByProjectIdAndTaskIdAsync(projectId, taskId);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(RetrieveTaskErrors.AccessDenied.Code);
    }
    
    [Fact]
    public async Task RetrieveByProjectIdAndTaskIdAsync_WhenTask_IsNonExistent_ReturnsFailure()
    {
        long projectId = 0;
        var currentUserId = "some valid id";
        long taskId = 1;

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity());
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectParticipantAsync(currentUserId, projectId))
            .ReturnsAsync(true);
        _taskRepositoryMock
            .Setup(repository => repository.FindByIdAsync(taskId))
            .ReturnsAsync((TaskEntity?)null);

        var result = await _taskRetrievalService.RetrieveByProjectIdAndTaskIdAsync(projectId, taskId);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(RetrieveTaskErrors.TaskNotFound.Code);
    }

    [Fact]
    public async Task RetrieveByProjectIdAndTaskIdAsync_WhenCurrentUser_IsAProjectParticipant_ReturnsSuccess()
    {
        long projectId = 0;
        var currentUserId = "some valid id";
        long taskId = 1;

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity());
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectParticipantAsync(currentUserId, projectId))
            .ReturnsAsync(true);
        _taskRepositoryMock
            .Setup(repository => repository.FindByIdAsync(taskId))
            .ReturnsAsync(new TaskEntity());

        var result = await _taskRetrievalService.RetrieveByProjectIdAndTaskIdAsync(projectId, taskId);

        result.IsSuccess.Should().Be(true); 
    }
}