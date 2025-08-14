using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.UseCases.Projects;
using TaskManager.UseCases.Projects.Delete;
using TaskManager.UseCases.Projects.Get;
using TaskManager.UseCases.Projects.Update;

namespace TaskManager.UnitTests.Projects;

public class ProjectServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IProjectMemberRepository> _projectMemberRepositoryMock;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly ProjectService _projectService;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public ProjectServiceTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _loggerMock = new Mock<ILogger>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _projectMemberRepositoryMock = new Mock<IProjectMemberRepository>();
        _unitOfWorkMock
            .Setup(work => work.SaveChangesAsync(CancellationToken.None))
            .ReturnsAsync(1);

        _projectService = new ProjectService(
            _projectRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _loggerMock.Object,
            _projectMemberRepositoryMock.Object
        );
    }

    [Fact]
    public async Task GetByIdAsync_WhenProject_IsNonExistent_ReturnsFailure()
    {
        long projectId = 0;
        var currentUserId = "some valid id";

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync((ProjectEntity?)null);

        var result = await _projectService.GetByIdAsync(projectId);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(GetProjectErrors.NotFound().Code);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCurrentUser_IsNotAProjectParticipant_ReturnsFailure()
    {
        long projectId = 1;
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

        var result = await _projectService.GetByIdAsync(projectId);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(GetProjectErrors.AccessDenied.Code);
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenCurrentUser_IsAProjectParticipant_ReturnsSuccess()
    {
        long projectId = 1;
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

        var result = await _projectService.GetByIdAsync(projectId);

        result.IsSuccess.Should().Be(true);
    }

    [Fact]
    public async Task UpdateAsync_WhenTheProject_IsNonExistent_ReturnsFailure()
    {
        long projectId = 1;
        var updateProjectDto = new UpdateProjectDto
        {
            ProjectId = projectId,
            ProjectTitle = "some title"
        };
        var currentUserId = "some valid id";
        
        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync((ProjectEntity?)null);

        var result = await _projectService.UpdateAsync(updateProjectDto);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(UpdateProjectErrors.NotFound().Code);
    }
    
    [Fact]
    public async Task UpdateAsync_WhenCurrentUser_IsNotAProjectLead_ReturnsFailure()
    {
        long projectId = 1;
        var updateProjectDto = new UpdateProjectDto
        {
            ProjectId = projectId,
            ProjectTitle = "some title"
        };
        var currentUserId = "some valid id";
        
        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                LeadUserId = "some random id",
            });

        var result = await _projectService.UpdateAsync(updateProjectDto);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(UpdateProjectErrors.AccessDenied.Code);
    }
    
    [Fact]
    public async Task UpdateAsync_WhenCurrentUser_IsAProjectLead_ReturnsSuccess()
    {
        long projectId = 1;
        var updateProjectDto = new UpdateProjectDto
        {
            ProjectId = projectId,
            ProjectTitle = "some title"
        };
        var currentUserId = "some valid id";
        
        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                LeadUserId = currentUserId,
            });

        var result = await _projectService.UpdateAsync(updateProjectDto);

        result.IsSuccess.Should().Be(true);
    }

    [Fact]
    public async Task DeleteAsync_WhenProject_IsNonExistent_ReturnsFailure()
    {
        long projectId = 1;
        var currentUserId = "some valid id";
        
        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync((ProjectEntity?)null);

        var result = await _projectService.DeleteAsync(projectId);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(DeleteProjectErrors.NotFound().Code);
    }
    
    [Fact]
    public async Task DeleteAsync_WhenCurrentUser_IsNotAProjectLead_ReturnsFailure()
    {
        long projectId = 1;
        var currentUserId = "some valid id";
        
        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                LeadUserId = "random id",
            });

        var result = await _projectService.DeleteAsync(projectId);

        result.IsFailure.Should().Be(true);
        result.Error.Code.Should().Be(DeleteProjectErrors.AccessDenied.Code);
    }
    
    [Fact]
    public async Task DeleteAsync_WhenCurrentUser_IsAProjectLead_ReturnsSuccess()
    {
        long projectId = 1;
        var currentUserId = "some valid id";
        
        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(currentUserId);
        _projectRepositoryMock
            .Setup(repository => repository.FindByIdAsync(projectId))
            .ReturnsAsync(new ProjectEntity
            {
                LeadUserId = currentUserId,
            });

        var result = await _projectService.DeleteAsync(projectId);

        result.IsSuccess.Should().Be(true);
    }
}