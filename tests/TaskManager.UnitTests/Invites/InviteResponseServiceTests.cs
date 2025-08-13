using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.UseCases.Invites.Response;
using TaskManager.UseCases.Invites.Response.Accept;
using TaskManager.UseCases.Invites.Response.Decline;

namespace TaskManager.UnitTests.Invites;

public class InviteResponseServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    private readonly InviteResponseService _inviteResponseService;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IProjectInviteRepository> _projectInviteRepositoryMock;
    private readonly Mock<IProjectMemberRepository> _projectMemberRepositoryMock;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public InviteResponseServiceTests()
    {
        _projectInviteRepositoryMock = new Mock<IProjectInviteRepository>();
        _projectMemberRepositoryMock = new Mock<IProjectMemberRepository>();
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _loggerMock = new Mock<ILogger>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(work => work.SaveChangesAsync(CancellationToken.None))
            .ReturnsAsync(1);

        _inviteResponseService = new InviteResponseService(
            _loggerMock.Object,
            _currentUserServiceMock.Object,
            _projectInviteRepositoryMock.Object,
            _projectRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _projectMemberRepositoryMock.Object
        );
    }

    [Fact]
    public async Task AcceptInviteAsync_WhenInviteId_IsNonExistent_ReturnsFailure()
    {
        long inviteId = 0;

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(" ");
        _projectInviteRepositoryMock.Setup(repository => repository.FindByIdAsync(inviteId))
            .ReturnsAsync((ProjectInvite?)null);

        var result = await _inviteResponseService.AcceptInviteAsync(inviteId);

        result.IsFailure.Should().Be(true);
        result.Error.Should().Be(AcceptInviteErrors.InviteNotFound);
    }
    
    [Fact]
    public async Task AcceptInviteAsync_WhenInvite_IsAlreadyAccepted_ReturnsFailure()
    {
        long inviteId = 1;

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(" ");
        _projectInviteRepositoryMock.Setup(repository => repository.FindByIdAsync(inviteId))
            .ReturnsAsync(new ProjectInvite
            {
                Status = InviteStatus.Accepted
            });

        var result = await _inviteResponseService.AcceptInviteAsync(inviteId);

        result.IsFailure.Should().Be(true);
        result.Error.Should().Be(AcceptInviteErrors.InviteAlreadyAccepted);
    }
    
    [Fact]
    public async Task AcceptInviteAsync_WhenInvite_IsAlreadyRejected_ReturnsFailure()
    {
        long inviteId = 1;

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(" ");
        _projectInviteRepositoryMock.Setup(repository => repository.FindByIdAsync(inviteId))
            .ReturnsAsync(new ProjectInvite
            {
                Status = InviteStatus.Rejected
            });

        var result = await _inviteResponseService.AcceptInviteAsync(inviteId);

        result.IsFailure.Should().Be(true);
        result.Error.Should().Be(AcceptInviteErrors.InviteAlreadyRejected);
    }
    
    [Fact]
    public async Task AcceptInviteAsync_WhenCurrentUser_IsNotTheInvitedUser_ReturnsFailure()
    {
        var currentUserId = "some valid id";
        long inviteId = 1;

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectInviteRepositoryMock.Setup(repository => repository.FindByIdAsync(inviteId))
            .ReturnsAsync(new ProjectInvite
            {
                InvitedUserId = "some random id",
                Status = InviteStatus.Pending
            });

        var result = await _inviteResponseService.AcceptInviteAsync(inviteId);

        result.IsFailure.Should().Be(true);
        result.Error.Should().Be(AcceptInviteErrors.AccessDenied);
    }
    
    [Fact]
    public async Task AcceptInviteAsync_WhenInvitedUser_IsAlreadyAProjectMember_ReturnsFailure()
    {
        var currentUserId = "some valid id";
        long inviteId = 1;
        var projectId = 1;

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectInviteRepositoryMock.Setup(repository => repository.FindByIdAsync(inviteId))
            .ReturnsAsync(new ProjectInvite
            {
                ProjectId = projectId,
                InvitedUserId = currentUserId,
                Status = InviteStatus.Pending
            });
        _projectMemberRepositoryMock.Setup(repository =>
            repository.IsUserProjectParticipantAsync(currentUserId, inviteId))
            .ReturnsAsync(true);

        var result = await _inviteResponseService.AcceptInviteAsync(inviteId);

        result.IsFailure.Should().Be(true);
        result.Error.Should().Be(AcceptInviteErrors.InvitedUserAlreadyAMember);
    }
    
    [Fact]
    public async Task AcceptInviteAsync_WhenInviteProject_IsNonExistent_ReturnsFailure()
    {
        var currentUserId = "some valid id";
        long inviteId = 1;
        var projectId = 1;

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectInviteRepositoryMock.Setup(repository => repository.FindByIdAsync(inviteId))
            .ReturnsAsync(new ProjectInvite
            {
                Id = projectId,
                InvitedUserId = currentUserId,
                ProjectId = 999,
                Status = InviteStatus.Pending
            });
        _projectMemberRepositoryMock.Setup(repository =>
                repository.IsUserProjectParticipantAsync(currentUserId, inviteId))
            .ReturnsAsync(false);
        _projectRepositoryMock.Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync((ProjectEntity?)null);

        var result = await _inviteResponseService.AcceptInviteAsync(inviteId);

        result.IsFailure.Should().Be(true);
        result.Error.Should().Be(AcceptInviteErrors.ProjectNotFound);
    }
    
    [Fact]
    public async Task AcceptInviteAsync_WhenEveryConditionIsMet_ReturnsSuccess()
    {
        var currentUserId = "some valid id";
        long inviteId = 1;
        var projectId = 1;

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectInviteRepositoryMock.Setup(repository => repository.FindByIdAsync(inviteId))
            .ReturnsAsync(new ProjectInvite
            {
                Id = projectId,
                InvitedUserId = currentUserId,
                ProjectId = projectId,
                Status = InviteStatus.Pending
            });
        _projectMemberRepositoryMock.Setup(repository =>
                repository.IsUserProjectParticipantAsync(currentUserId, inviteId))
            .ReturnsAsync(false);
        _projectRepositoryMock.Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity());

        var result = await _inviteResponseService.AcceptInviteAsync(inviteId);

        result.IsSuccess.Should().Be(true);
    }
    
    [Fact]
    public async Task DeclineInviteAsync_WhenInviteId_IsNonExistent_ReturnsFailure()
    {
        long inviteId = 0;

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(" ");
        _projectInviteRepositoryMock.Setup(repository => repository.FindByIdAsync(inviteId))
            .ReturnsAsync((ProjectInvite?)null);

        var result = await _inviteResponseService.DeclineInviteAsync(inviteId);

        result.IsFailure.Should().Be(true);
        result.Error.Should().Be(DeclineInviteErrors.InviteNotFound);
    }
    
    [Fact]
    public async Task DeclineInviteAsync_WhenInvite_IsAlreadyAccepted_ReturnsFailure()
    {
        long inviteId = 1;

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(" ");
        _projectInviteRepositoryMock.Setup(repository => repository.FindByIdAsync(inviteId))
            .ReturnsAsync(new ProjectInvite
            {
                Status = InviteStatus.Accepted
            });

        var result = await _inviteResponseService.DeclineInviteAsync(inviteId);

        result.IsFailure.Should().Be(true);
        result.Error.Should().Be(DeclineInviteErrors.InviteAlreadyAccepted);
    }
    
    [Fact]
    public async Task DeclineInviteAsync_WhenInvite_IsAlreadyRejected_ReturnsFailure()
    {
        long inviteId = 1;

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(" ");
        _projectInviteRepositoryMock.Setup(repository => repository.FindByIdAsync(inviteId))
            .ReturnsAsync(new ProjectInvite
            {
                Status = InviteStatus.Rejected
            });

        var result = await _inviteResponseService.DeclineInviteAsync(inviteId);

        result.IsFailure.Should().Be(true);
        result.Error.Should().Be(DeclineInviteErrors.InviteAlreadyRejected);
    }
    
    [Fact]
    public async Task DeclineInviteAsync_WhenCurrentUser_IsNotTheInvitedUser_ReturnsFailure()
    {
        var currentUserId = "some valid id";
        long inviteId = 1;

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectInviteRepositoryMock.Setup(repository => repository.FindByIdAsync(inviteId))
            .ReturnsAsync(new ProjectInvite
            {
                InvitedUserId = "some random id",
                Status = InviteStatus.Pending
            });

        var result = await _inviteResponseService.DeclineInviteAsync(inviteId);

        result.IsFailure.Should().Be(true);
        result.Error.Should().Be(DeclineInviteErrors.AccessDenied);
    }
    
    [Fact]
    public async Task DeclineInviteAsync_WhenInviteProject_IsNonExistent_ReturnsFailure()
    {
        var currentUserId = "some valid id";
        long inviteId = 1;
        var projectId = 1;

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectInviteRepositoryMock.Setup(repository => repository.FindByIdAsync(inviteId))
            .ReturnsAsync(new ProjectInvite
            {
                Id = projectId,
                InvitedUserId = currentUserId,
                ProjectId = 999,
                Status = InviteStatus.Pending
            });
        _projectMemberRepositoryMock.Setup(repository =>
                repository.IsUserProjectParticipantAsync(currentUserId, inviteId))
            .ReturnsAsync(false);
        _projectRepositoryMock.Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync((ProjectEntity?)null);

        var result = await _inviteResponseService.DeclineInviteAsync(inviteId);

        result.IsFailure.Should().Be(true);
        result.Error.Should().Be(DeclineInviteErrors.ProjectNotFound);
    }
    
    [Fact]
    public async Task DeclineInviteAsync_WhenEveryConditionIsMet_ReturnsSuccess()
    {
        var currentUserId = "some valid id";
        long inviteId = 1;
        var projectId = 1;

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectInviteRepositoryMock.Setup(repository => repository.FindByIdAsync(inviteId))
            .ReturnsAsync(new ProjectInvite
            {
                Id = projectId,
                InvitedUserId = currentUserId,
                ProjectId = projectId,
                Status = InviteStatus.Pending
            });
        _projectMemberRepositoryMock.Setup(repository =>
                repository.IsUserProjectParticipantAsync(currentUserId, inviteId))
            .ReturnsAsync(false);
        _projectRepositoryMock.Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity());

        var result = await _inviteResponseService.AcceptInviteAsync(inviteId);

        result.IsSuccess.Should().Be(true);
    }
}