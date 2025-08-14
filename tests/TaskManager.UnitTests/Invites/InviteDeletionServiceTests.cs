using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.UseCases.Invites.Delete;

namespace TaskManager.UnitTests.Invites;

public class InviteDeletionServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    private readonly InviteDeletionService _inviteDeletionService;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IProjectInviteRepository> _projectInviteRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public InviteDeletionServiceTests()
    {
        _projectInviteRepositoryMock = new Mock<IProjectInviteRepository>();
        _loggerMock = new Mock<ILogger>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(work => work.SaveChangesAsync(CancellationToken.None))
            .ReturnsAsync(1);

        _inviteDeletionService = new InviteDeletionService(
            _loggerMock.Object,
            _currentUserServiceMock.Object,
            _projectInviteRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task DeleteAsync_WhenInviteId_IsNonExistent_ReturnsFailure()
    {
        long inviteId = 0;

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(" ");
        _projectInviteRepositoryMock
            .Setup(repository => repository.FindByIdAsync(inviteId))
            .ReturnsAsync((ProjectInvite?)null);

        var result = await _inviteDeletionService.DeleteAsync(inviteId);

        result.IsFailure.Should().Be(true);
        result.Error.Should().Be(DeleteInviteErrors.InviteNotFound);
    }

    [Fact]
    public async Task DeleteAsync_WhenCurrentUser_IsNotTheOne_Who_CreatedTheInvite_ReturnsFailure()
    {
        var currentUserId = "some id";

        long inviteId = 1;

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectInviteRepositoryMock
            .Setup(repository => repository.FindByIdAsync(inviteId))
            .ReturnsAsync(new ProjectInvite
            {
                InvitedByUserId = "random user id"
            });

        var result = await _inviteDeletionService.DeleteAsync(inviteId);

        result.IsFailure.Should().Be(true);
        result.Error.Should().Be(DeleteInviteErrors.AccessDenied);
    }

    [Fact]
    public async Task DeleteAsync_WhenCurrentUser_IsTheOne_Who_CreatedTheInvite_ReturnsSuccess()
    {
        var currentUserId = "some id";

        long inviteId = 1;

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectInviteRepositoryMock
            .Setup(repository => repository.FindByIdAsync(inviteId))
            .ReturnsAsync(new ProjectInvite
            {
                InvitedByUserId = currentUserId
            });

        var result = await _inviteDeletionService.DeleteAsync(inviteId);

        result.IsSuccess.Should().Be(true);
    }
}