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
}