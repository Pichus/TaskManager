using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.Infrastructure.Identity.User;
using TaskManager.UseCases.Invites.Create;

namespace TaskManager.UnitTests.Invites;

public class InviteCreationServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly InviteCreationService _inviteCreationService;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IProjectInviteRepository> _projectInviteRepositoryMock;
    private readonly Mock<IProjectMemberRepository> _projectMemberRepositoryMock;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<UserManager<TaskManagerUser>> _userManagerMock;

    public InviteCreationServiceTests()
    {
        _projectInviteRepositoryMock = new Mock<IProjectInviteRepository>();
        _projectMemberRepositoryMock = new Mock<IProjectMemberRepository>();
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _loggerMock = new Mock<ILogger>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(work => work.SaveChangesAsync(CancellationToken.None))
            .ReturnsAsync(1);

        var userStoreMock = new Mock<IUserStore<TaskManagerUser>>();
        _userManagerMock =
            new Mock<UserManager<TaskManagerUser>>(userStoreMock.Object, null, null, null, null, null, null, null,
                null);

        _inviteCreationService = new InviteCreationService(
            _loggerMock.Object,
            _currentUserServiceMock.Object,
            _projectRepositoryMock.Object,
            _userManagerMock.Object,
            _projectInviteRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _projectMemberRepositoryMock.Object
        );
    }

    [Fact]
    public async Task CreateInviteAsync_InvokedByProjectLead_ReturnsSuccess()
    {
        var currentUserId = "some valid current user id";

        var createInviteDto = new CreateInviteDto
        {
            ProjectId = 1,
            InvitedUserId = "some valid invited user id"
        };

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock.Setup(repo => repo.FindByIdAsync(createInviteDto.ProjectId))
            .ReturnsAsync(new ProjectEntity());
        _projectMemberRepositoryMock.Setup(repository =>
                repository.IsUserProjectLeadAsync(currentUserId, createInviteDto.ProjectId))
            .ReturnsAsync(true);
        _userManagerMock.Setup(manager => manager.FindByIdAsync(createInviteDto.InvitedUserId))
            .ReturnsAsync(new TaskManagerUser());
        _projectInviteRepositoryMock.Setup(repository =>
                repository.InviteExistsAsync(createInviteDto.InvitedUserId, createInviteDto.ProjectId))
            .ReturnsAsync(false);
        _projectMemberRepositoryMock.Setup(repository =>
                repository.IsUserProjectParticipantAsync(createInviteDto.InvitedUserId, createInviteDto.ProjectId))
            .ReturnsAsync(false);

        var result = await _inviteCreationService.CreateAsync(createInviteDto);

        result.IsSuccess.Should().Be(true);
    }

    [Fact]
    public async Task CreateInviteAsync_InvokedByProjectManager_ReturnsSuccess()
    {
        var currentUserId = "some valid current user id";

        var createInviteDto = new CreateInviteDto
        {
            ProjectId = 1,
            InvitedUserId = "some valid invited user id"
        };

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock.Setup(repo => repo.FindByIdAsync(createInviteDto.ProjectId))
            .ReturnsAsync(new ProjectEntity());
        _projectMemberRepositoryMock.Setup(repository =>
                repository.IsUserProjectManagerAsync(currentUserId, createInviteDto.ProjectId))
            .ReturnsAsync(true);
        _userManagerMock.Setup(manager => manager.FindByIdAsync(createInviteDto.InvitedUserId))
            .ReturnsAsync(new TaskManagerUser());
        _projectInviteRepositoryMock.Setup(repository =>
                repository.InviteExistsAsync(createInviteDto.InvitedUserId, createInviteDto.ProjectId))
            .ReturnsAsync(false);
        _projectMemberRepositoryMock.Setup(repository =>
                repository.IsUserProjectParticipantAsync(createInviteDto.InvitedUserId, createInviteDto.ProjectId))
            .ReturnsAsync(false);

        var result = await _inviteCreationService.CreateAsync(createInviteDto);

        result.IsSuccess.Should().Be(true);
    }


    [Fact]
    public async Task CreateInviteAsync_WithNonExistingProjectId_ReturnsFailureResult()
    {
        var createInviteDto = new CreateInviteDto
        {
            ProjectId = 0,
            InvitedUserId = "some valid invited user id"
        };

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns("");
        _projectRepositoryMock.Setup(repo => repo.FindByIdAsync(createInviteDto.ProjectId))
            .ReturnsAsync((ProjectEntity?)null);

        var result = await _inviteCreationService.CreateAsync(createInviteDto);

        result.IsFailure.Should().Be(true);
        result.Error.Should().Be(CreateInviteErrors.ProjectNotFound);
    }

    [Fact]
    public async Task CreateInviteAsync_WhenInvitedUser_IsNotFound_ReturnsFailureResult()
    {
        var currentUserId = "some valid current user id";

        var createInviteDto = new CreateInviteDto
        {
            ProjectId = 1,
            InvitedUserId = "some valid invited user id"
        };

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock.Setup(repo => repo.FindByIdAsync(createInviteDto.ProjectId))
            .ReturnsAsync(new ProjectEntity());
        _projectMemberRepositoryMock.Setup(repository =>
                repository.IsUserProjectManagerAsync(currentUserId, createInviteDto.ProjectId))
            .ReturnsAsync(true);
        _userManagerMock.Setup(manager => manager.FindByIdAsync(createInviteDto.InvitedUserId))
            .ReturnsAsync((TaskManagerUser?)null);

        var result = await _inviteCreationService.CreateAsync(createInviteDto);

        result.IsFailure.Should().Be(true);
        result.Error.Should().Be(CreateInviteErrors.InvitedUserNotFound);
    }

    [Fact]
    public async Task CreateInviteAsync_WhenInvitedUser_IsAlreadyInvited_ReturnsFailureResult()
    {
        var currentUserId = "some valid current user id";

        var createInviteDto = new CreateInviteDto
        {
            ProjectId = 1,
            InvitedUserId = "some valid invited user id"
        };

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock.Setup(repo => repo.FindByIdAsync(createInviteDto.ProjectId))
            .ReturnsAsync(new ProjectEntity());
        _projectMemberRepositoryMock.Setup(repository =>
                repository.IsUserProjectManagerAsync(currentUserId, createInviteDto.ProjectId))
            .ReturnsAsync(true);
        _userManagerMock.Setup(manager => manager.FindByIdAsync(createInviteDto.InvitedUserId))
            .ReturnsAsync(new TaskManagerUser());
        _projectInviteRepositoryMock.Setup(repository =>
                repository.InviteExistsAsync(createInviteDto.InvitedUserId, createInviteDto.ProjectId))
            .ReturnsAsync(true);

        var result = await _inviteCreationService.CreateAsync(createInviteDto);

        result.IsFailure.Should().Be(true);
        result.Error.Should().Be(CreateInviteErrors.UserAlreadyInvited);
    }

    [Fact]
    public async Task CreateInviteAsync_WhenInvitedUser_IsAlreadyAMember_ReturnsFailureResult()
    {
        var currentUserId = "some valid current user id";

        var createInviteDto = new CreateInviteDto
        {
            ProjectId = 1,
            InvitedUserId = "some valid invited user id"
        };

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock.Setup(repo => repo.FindByIdAsync(createInviteDto.ProjectId))
            .ReturnsAsync(new ProjectEntity());
        _projectMemberRepositoryMock.Setup(repository =>
                repository.IsUserProjectManagerAsync(currentUserId, createInviteDto.ProjectId))
            .ReturnsAsync(true);
        _userManagerMock.Setup(manager => manager.FindByIdAsync(createInviteDto.InvitedUserId))
            .ReturnsAsync(new TaskManagerUser());
        _projectInviteRepositoryMock.Setup(repository =>
                repository.InviteExistsAsync(createInviteDto.InvitedUserId, createInviteDto.ProjectId))
            .ReturnsAsync(false);
        _projectMemberRepositoryMock.Setup(repository =>
                repository.IsUserProjectParticipantAsync(createInviteDto.InvitedUserId, createInviteDto.ProjectId))
            .ReturnsAsync(true);

        var result = await _inviteCreationService.CreateAsync(createInviteDto);

        result.IsFailure.Should().Be(true);
        result.Error.Should().Be(CreateInviteErrors.InvitedUserAlreadyAMember);
    }
}