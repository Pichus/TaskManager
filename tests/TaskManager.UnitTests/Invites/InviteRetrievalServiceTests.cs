using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Core.Shared;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.UseCases.Invites.Retrieve;

namespace TaskManager.UnitTests.Invites;

public class InviteRetrievalServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    private readonly InviteRetrievalService _inviteRetrievalService;
    private readonly Mock<ILogger<InviteRetrievalService>> _loggerMock;
    private readonly Mock<IProjectInviteRepository> _projectInviteRepositoryMock;
    private readonly Mock<IProjectMemberRepository> _projectMemberRepositoryMock;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public InviteRetrievalServiceTests()
    {
        _projectInviteRepositoryMock = new Mock<IProjectInviteRepository>();
        _projectMemberRepositoryMock = new Mock<IProjectMemberRepository>();
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _loggerMock = new Mock<ILogger<InviteRetrievalService>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(work => work.SaveChangesAsync(CancellationToken.None))
            .ReturnsAsync(1);

        _inviteRetrievalService = new InviteRetrievalService(
            _loggerMock.Object,
            _currentUserServiceMock.Object,
            _projectInviteRepositoryMock.Object,
            _projectMemberRepositoryMock.Object,
            _projectRepositoryMock.Object
        );
    }

    [Fact]
    public async Task RetrievePendingInvitesForCurrentUser_ReturnsSuccess()
    {
        var retrievePendingInvitesDto = new RetrievePendingInvitesDto
        {
            PageNumber = 1,
            PageSize = 25
        };

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(" ");

        var result = await _inviteRetrievalService.RetrievePendingInvitesForCurrentUserAsync(retrievePendingInvitesDto);

        result.IsSuccess.Should().Be(true);
    }

    [Fact]
    public async Task RetrievePendingProjectInvitesAsync_WhenProjectId_IsNonExistent_ReturnsFailure()
    {
        var currentUserId = "some valid id";
        long projectId = 0;
        var retrievePendingProjectInvitesDto = new RetrievePendingProjectInvitesDto
        {
            ProjectId = projectId,
            PageNumber = 1,
            PageSize = 1
        };

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdWithInvitesIncludedAsync(projectId))
            .ReturnsAsync((ProjectEntity?)null);

        var result = await _inviteRetrievalService.RetrievePendingProjectInvitesAsync(retrievePendingProjectInvitesDto);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(RetrieveInvitesErrors.ProjectNotFound.Code);
    }

    [Fact]
    public async Task RetrievePendingProjectInvitesAsync_WhenCurrentUser_IsNotAProjectManagerOrLead_ReturnsFailure()
    {
        var currentUserId = "some valid id";
        long projectId = 1;
        var retrievePendingProjectInvitesDto = new RetrievePendingProjectInvitesDto
        {
            ProjectId = projectId,
            PageNumber = 1,
            PageSize = 1
        };

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                Id = projectId,
                LeadUserId = "some random id"
            });
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectLeadAsync(currentUserId, projectId))
            .ReturnsAsync(false);
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectManagerAsync(currentUserId, projectId))
            .ReturnsAsync(false);

        var result = await _inviteRetrievalService.RetrievePendingProjectInvitesAsync(retrievePendingProjectInvitesDto);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(RetrieveInvitesErrors.AccessDenied.Code);
    }

    [Fact]
    public async Task RetrievePendingProjectInvitesAsync_WhenCurrentUser_IsAProjectManager_ReturnsSuccess()
    {
        var currentUserId = "some valid id";
        long projectId = 1;
        var retrievePendingProjectInvitesDto = new RetrievePendingProjectInvitesDto
        {
            ProjectId = projectId,
            PageNumber = 1,
            PageSize = 1
        };

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                Id = projectId,
                LeadUserId = "some random id"
            });
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectLeadAsync(currentUserId, projectId))
            .ReturnsAsync(false);
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectManagerAsync(currentUserId, projectId))
            .ReturnsAsync(true);

        var result = await _inviteRetrievalService.RetrievePendingProjectInvitesAsync(retrievePendingProjectInvitesDto);

        result.IsSuccess.Should().Be(true);
    }

    [Fact]
    public async Task RetrievePendingProjectInvitesAsync_WhenCurrentUser_IsAProjectLead_ReturnsSuccess()
    {
        var currentUserId = "some valid id";
        long projectId = 1;
        var retrievePendingProjectInvitesDto = new RetrievePendingProjectInvitesDto
        {
            ProjectId = projectId,
            PageNumber = 1,
            PageSize = 1
        };

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                Id = projectId,
                LeadUserId = currentUserId
            });
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectLeadAsync(currentUserId, projectId))
            .ReturnsAsync(true);
        _projectMemberRepositoryMock
            .Setup(repository => repository.IsUserProjectManagerAsync(currentUserId, projectId))
            .ReturnsAsync(false);
        _projectInviteRepositoryMock
            .Setup(repository => repository.GetPendingInvitesByProjectIdAsync(projectId,
                retrievePendingProjectInvitesDto.PageNumber, retrievePendingProjectInvitesDto.PageSize))
            .ReturnsAsync(new PagedData<ProjectInvite>(new List<ProjectInvite>(), 1, 1, 1));

        var result = await _inviteRetrievalService.RetrievePendingProjectInvitesAsync(retrievePendingProjectInvitesDto);

        result.IsSuccess.Should().Be(true);
    }
}