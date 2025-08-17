using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.TaskAggregate;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.UseCases.Tasks.Create;

namespace TaskManager.UnitTests.Tasks;

public class TaskCreationServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<TaskCreationService>> _loggerMock;
    private readonly Mock<IProjectMemberRepository> _projectMemberRepositoryMock;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly TaskCreationService _taskCreationService;
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public TaskCreationServiceTests()
    {
        _projectMemberRepositoryMock = new Mock<IProjectMemberRepository>();
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _loggerMock = new Mock<ILogger<TaskCreationService>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(CancellationToken.None))
            .ReturnsAsync(1);

        _taskCreationService = new TaskCreationService(
            _loggerMock.Object,
            _currentUserServiceMock.Object,
            _projectMemberRepositoryMock.Object,
            _projectRepositoryMock.Object,
            _taskRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task CreateAsync_WhenProject_IsNonExistent_ReturnsFailure()
    {
        long projectId = 0;
        var currentUserId = "some valid current user id";
        var assigneeUserId = "some valid assignee user id";
        var createTaskDto = new CreateTaskDto
        {
            ProjectId = projectId,
            AssigneeUserId = assigneeUserId,
            Title = "title",
            Description = "description",
            DueDate = DateTime.UtcNow.AddDays(10)
        };

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync((ProjectEntity?)null);

        var result = await _taskCreationService.CreateAsync(createTaskDto);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(CreateTaskErrors.ProjectNotFound.Code);
    }

    [Fact]
    public async Task CreateAsync_WhenCurrentUser_IsNotAProjectManagerOrLead_ReturnsFailure()
    {
        long projectId = 0;
        var currentUserId = "some valid current user id";
        var assigneeUserId = "some valid assignee user id";
        var createTaskDto = new CreateTaskDto
        {
            ProjectId = projectId,
            AssigneeUserId = assigneeUserId,
            Title = "title",
            Description = "description",
            DueDate = DateTime.UtcNow.AddDays(10)
        };

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                CreatedAt = DateTime.UtcNow,
                Title = createTaskDto.Title,
                LeadUserId = "random id"
            });
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectLeadAsync(currentUserId, projectId))
            .ReturnsAsync(false);
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectManagerAsync(currentUserId, projectId))
            .ReturnsAsync(false);

        var result = await _taskCreationService.CreateAsync(createTaskDto);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(CreateTaskErrors.AccessDenied.Code);
    }

    [Fact]
    public async Task CreateAsync_WhenCurrentUser_IsAProjectManager_ReturnsSuccess()
    {
        long projectId = 0;
        var currentUserId = "some valid current user id";
        var assigneeUserId = "some valid assignee user id";
        var createTaskDto = new CreateTaskDto
        {
            ProjectId = projectId,
            AssigneeUserId = assigneeUserId,
            Title = "title",
            Description = "description",
            DueDate = DateTime.UtcNow.AddDays(10)
        };

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                CreatedAt = DateTime.UtcNow,
                Title = createTaskDto.Title,
                LeadUserId = currentUserId
            });
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectLeadAsync(currentUserId, projectId))
            .ReturnsAsync(false);
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectManagerAsync(currentUserId, projectId))
            .ReturnsAsync(true);

        var result = await _taskCreationService.CreateAsync(createTaskDto);

        result.IsSuccess.Should().Be(true);
    }

    [Fact]
    public async Task CreateAsync_WhenCurrentUser_IsAProjectLead_ReturnsSuccess()
    {
        long projectId = 0;
        var currentUserId = "some valid current user id";
        var assigneeUserId = "some valid assignee user id";
        var createTaskDto = new CreateTaskDto
        {
            ProjectId = projectId,
            AssigneeUserId = assigneeUserId,
            Title = "title",
            Description = "description",
            DueDate = DateTime.UtcNow.AddDays(10)
        };

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                CreatedAt = DateTime.UtcNow,
                Title = createTaskDto.Title,
                LeadUserId = "random id"
            });
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectLeadAsync(currentUserId, projectId))
            .ReturnsAsync(true);
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectManagerAsync(currentUserId, projectId))
            .ReturnsAsync(false);

        var result = await _taskCreationService.CreateAsync(createTaskDto);

        result.IsSuccess.Should().Be(true);
    }
}