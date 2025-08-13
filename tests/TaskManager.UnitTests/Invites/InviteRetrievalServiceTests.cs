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
using TaskManager.UseCases.Invites.Retrieve;

namespace TaskManager.UnitTests.Invites;

public class InviteRetrievalServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IProjectInviteRepository> _projectInviteRepositoryMock;
    private readonly Mock<IProjectMemberRepository> _projectMemberRepositoryMock;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    private readonly InviteRetrievalService _inviteRetrievalService;
    
    public InviteRetrievalServiceTests()
    {
        _projectInviteRepositoryMock = new Mock<IProjectInviteRepository>();
        _projectMemberRepositoryMock = new Mock<IProjectMemberRepository>();
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _loggerMock = new Mock<ILogger>();
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
        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(" ");
        
        var result = await _inviteRetrievalService.RetrievePendingInvitesForCurrentUserAsync();

        result.IsSuccess.Should().Be(true);
    }

    [Fact]
    public async Task RetrievePendingProjectInvitesAsync_WhenProjectId_IsNonExistent_ReturnsFailure()
    {
        var currentUserId = "some valid id";
        long projectId = 0;
        
        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock.Setup(repository => repository.FindByIdWithInvitesIncludedAsync(projectId))
            .ReturnsAsync((ProjectEntity?)null);

        var result = await _inviteRetrievalService.RetrievePendingProjectInvitesAsync(projectId);

        result.IsFailure.Should().Be(true);
        result.Error.Should().Be(RetrieveInvitesErrors.ProjectNotFound);
    }
    
    [Fact]
    public async Task RetrievePendingProjectInvitesAsync_WhenCurrentUser_IsNotAProjectManagerOrLead_ReturnsFailure()
    {
        var currentUserId = "some valid id";
        long projectId = 1;
        
        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock.Setup(repository => repository.FindByIdWithInvitesIncludedAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                Id = projectId,
                LeadUserId = "some random id",
            });
        _projectMemberRepositoryMock.Setup(repository => repository.IsUserProjectLeadAsync(currentUserId, projectId))
            .ReturnsAsync(false);
        _projectMemberRepositoryMock.Setup(repository => repository.IsUserProjectManagerAsync(currentUserId, projectId))
            .ReturnsAsync(false);

        var result = await _inviteRetrievalService.RetrievePendingProjectInvitesAsync(projectId);

        result.IsFailure.Should().Be(true);
        result.Error.Should().Be(RetrieveInvitesErrors.AccessDenied);
    }
    
    [Fact]
    public async Task RetrievePendingProjectInvitesAsync_WhenCurrentUser_IsAProjectManager_ReturnsSuccess()
    {
        var currentUserId = "some valid id";
        long projectId = 1;
        
        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock.Setup(repository => repository.FindByIdWithInvitesIncludedAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                Id = projectId,
                LeadUserId = "some random id",
            });
        _projectMemberRepositoryMock.Setup(repository => repository.IsUserProjectLeadAsync(currentUserId, projectId))
            .ReturnsAsync(false);
        _projectMemberRepositoryMock.Setup(repository => repository.IsUserProjectManagerAsync(currentUserId, projectId))
            .ReturnsAsync(true);

        var result = await _inviteRetrievalService.RetrievePendingProjectInvitesAsync(projectId);

        result.IsSuccess.Should().Be(true);
    }
    
    [Fact]
    public async Task RetrievePendingProjectInvitesAsync_WhenCurrentUser_IsAProjectLead_ReturnsSuccess()
    {
        var currentUserId = "some valid id";
        long projectId = 1;
        
        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock.Setup(repository => repository.FindByIdWithInvitesIncludedAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                Id = projectId,
                LeadUserId = currentUserId,
            });
        _projectMemberRepositoryMock.Setup(repository => repository.IsUserProjectLeadAsync(currentUserId, projectId))
            .ReturnsAsync(true);
        _projectMemberRepositoryMock.Setup(repository => repository.IsUserProjectManagerAsync(currentUserId, projectId))
            .ReturnsAsync(false);

        var result = await _inviteRetrievalService.RetrievePendingProjectInvitesAsync(projectId);

        result.IsSuccess.Should().Be(true);
    }
}