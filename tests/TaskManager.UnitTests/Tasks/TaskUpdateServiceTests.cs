using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.TaskAggregate;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.UseCases.Tasks.Update;

namespace TaskManager.UnitTests.Tasks;

public class TaskUpdateServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IProjectMemberRepository> _projectMemberRepositoryMock;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly TaskUpdateService _taskUpdateService;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public TaskUpdateServiceTests()
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

        _taskUpdateService = new TaskUpdateService(
            _currentUserServiceMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _projectMemberRepositoryMock.Object,
            _projectRepositoryMock.Object,
            _taskRepositoryMock.Object
        );
    }

    [Fact]
    public async Task UpdateAsync_WhenProjectIsNonExistent_ReturnsFailure()
    {
        long projectId = 0;
        long taskId = 0;
        var updateTaskDto = new UpdateTaskDto
        {
            ProjectId = projectId,
            TaskId = taskId,
            Title = "title",
            Description = "description",
            DueDate = DateTime.UtcNow.AddDays(7)
        };
        var currentUserId = "some valid id";

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync((ProjectEntity?)null);

        var result = await _taskUpdateService.UpdateAsync(updateTaskDto);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(UpdateTaskErrors.ProjectNotFound.Code);
    }
    
    [Fact]
    public async Task UpdateAsync_WhenTaskIsNonExistent_ReturnsFailure()
    {
        long projectId = 0;
        long taskId = 0;
        var updateTaskDto = new UpdateTaskDto
        {
            ProjectId = projectId,
            TaskId = taskId,
            Title = "title",
            Description = "description",
            DueDate = DateTime.UtcNow.AddDays(7)
        };
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

        var result = await _taskUpdateService.UpdateAsync(updateTaskDto);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(UpdateTaskErrors.TaskNotFound.Code);
    }
    
    [Fact]
    public async Task UpdateAsync_WhenTheTask_DoesNotBelongToThisProject_ReturnsFailure()
    {
        long projectId = 0;
        long taskId = 0;
        var updateTaskDto = new UpdateTaskDto
        {
            ProjectId = projectId,
            TaskId = taskId,
            Title = "title",
            Description = "description",
            DueDate = DateTime.UtcNow.AddDays(7)
        };
        var currentUserId = "some valid id";

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity());
        _taskRepositoryMock
            .Setup(repository => repository.FindByIdAsync(taskId))
            .ReturnsAsync(new TaskEntity
            {
                Id = taskId,
                ProjectId = 999,
            });

        var result = await _taskUpdateService.UpdateAsync(updateTaskDto);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(UpdateTaskErrors.TaskNotFound.Code);
    }

    [Fact]
    public async Task UpdateAsync_WhenCurrentUser_IsNotAProjectLeadOrManager_ReturnsFailure()
    {
        long projectId = 0;
        long taskId = 0;
        var updateTaskDto = new UpdateTaskDto
        {
            ProjectId = projectId,
            TaskId = taskId,
            Title = "title",
            Description = "description",
            DueDate = DateTime.UtcNow.AddDays(7)
        };
        var currentUserId = "some valid id";

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                Id = projectId,
                LeadUserId = "random id",
            });
        _taskRepositoryMock
            .Setup(repository => repository.FindByIdAsync(taskId))
            .ReturnsAsync(new TaskEntity()
            {
                Id = taskId,
                ProjectId = projectId,
            });
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectMemberAsync(currentUserId, projectId))
            .ReturnsAsync(false);

        var result = await _taskUpdateService.UpdateAsync(updateTaskDto);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(UpdateTaskErrors.AccessDenied.Code);
    }

    [Fact]
    public async Task UpdateAsync_WhenCurrentUser_IsAProjectLead_ReturnsSuccess()
    {
        long projectId = 0;
        long taskId = 0;
        var updateTaskDto = new UpdateTaskDto
        {
            ProjectId = projectId,
            TaskId = taskId,
            Title = "title",
            Description = "description",
            DueDate = DateTime.UtcNow.AddDays(7)
        };
        var currentUserId = "some valid id";

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                Id = projectId,
                LeadUserId = currentUserId,
            });
        _taskRepositoryMock
            .Setup(repository => repository.FindByIdAsync(taskId))
            .ReturnsAsync(new TaskEntity()
            {
                Id = taskId,
                ProjectId = projectId,
            });
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectManagerAsync(currentUserId, projectId))
            .ReturnsAsync(false);

        var result = await _taskUpdateService.UpdateAsync(updateTaskDto);

        result.IsSuccess.Should().Be(true);
    }
    
    [Fact]
    public async Task UpdateAsync_WhenCurrentUser_IsAProjectManager_ReturnsSuccess()
    {
        long projectId = 0;
        long taskId = 0;
        var updateTaskDto = new UpdateTaskDto
        {
            ProjectId = projectId,
            TaskId = taskId,
            Title = "title",
            Description = "description",
            DueDate = DateTime.UtcNow.AddDays(7)
        };
        var currentUserId = "some valid id";

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                Id = projectId,
                LeadUserId = "random id",
            });
        _taskRepositoryMock
            .Setup(repository => repository.FindByIdAsync(taskId))
            .ReturnsAsync(new TaskEntity()
            {
                Id = taskId,
                ProjectId = projectId,
            });
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectManagerAsync(currentUserId, projectId))
            .ReturnsAsync(true);

        var result = await _taskUpdateService.UpdateAsync(updateTaskDto);

        result.IsSuccess.Should().Be(true);
    }
}