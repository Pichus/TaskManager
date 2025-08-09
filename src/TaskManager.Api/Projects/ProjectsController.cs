using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Projects.Create;
using TaskManager.Projects.Get;
using TaskManager.Projects.Update;
using TaskManager.UseCases.Projects;
using TaskManager.UseCases.Projects.Create;
using TaskManager.UseCases.Projects.Delete;
using TaskManager.UseCases.Projects.Get;
using TaskManager.UseCases.Projects.GetMembers;
using TaskManager.UseCases.Projects.Update;

namespace TaskManager.Projects;

[Authorize]
[Route("api/projects")]
[ApiController]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetProjectResponse>>> GetAll()
    {
        var userProjects = await _projectService.GetAllByUserAsync();

        var userProjectsResponse = userProjects.Select(ProjectToGetProjectResponse);

        return Ok(userProjectsResponse);
    }

    [HttpGet("{projectId:long}")]
    public async Task<ActionResult<GetProjectResponse>> Get(long projectId)
    {
        var getProjectResult = await _projectService.GetByIdAsync(projectId);

        if (getProjectResult.IsFailure)
        {
            var errorCode = getProjectResult.Error.Code;
            var errorMessage = getProjectResult.Error.Message;

            if (errorCode == GetProjectErrors.NotFound().Code) return NotFound(errorMessage);

            if (errorCode == GetProjectErrors.AccessDenied.Code) return Forbid();
        }

        var project = getProjectResult.Value;

        var response = ProjectToGetProjectResponse(project);

        return Ok(response);
    }

    [HttpGet("{projectId:long}/members")]
    public async Task<ActionResult<IEnumerable<string>>> GetMembers(long projectId)
    {
        var result = await _projectService.GetProjectMembersAsync(projectId);

        if (result.IsFailure)
        {
            var errorCode = result.Error.Code;
            var errorMessage = result.Error.Message;

            if (errorCode == GetProjectMembersErrors.Unauthenticated.Code) return Unauthorized();
            
            if (errorCode == GetProjectMembersErrors.ProjectNotFound.Code) return NotFound(errorMessage);

            if (errorCode == GetProjectMembersErrors.AccessDenied.Code) return Forbid();
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<CreateProjectResponse>> Create(CreateProjectRequest request)
    {
        var createProjectResult = await _projectService.CreateAsync(CreateProjectRequestToDto(request));

        var response = ProjectToCreateProjectResponse(createProjectResult.Value);

        return CreatedAtAction(nameof(Create), response);
    }

    [HttpPut("{projectId:long}")]
    public async Task<ActionResult<UpdateProjectResponse>> Put([FromRoute] long projectId,
        [FromBody] UpdateProjectRequest request)
    {
        var updateProjectResult = await _projectService.UpdateAsync(UpdateProjectRequestToDto(projectId, request));

        if (updateProjectResult.IsFailure)
        {
            var errorCode = updateProjectResult.Error.Code;
            var errorMessage = updateProjectResult.Error.Message;

            if (errorCode == UpdateProjectErrors.NotFound().Code) return NotFound(errorMessage);

            if (errorCode == UpdateProjectErrors.AccessDenied.Code) return Forbid();
        }

        var response = ProjectToUpdateProjectResponse(updateProjectResult.Value);

        return Ok(response);
    }

    [HttpDelete("{projectId:long}")]
    public async Task<ActionResult> Delete(long projectId)
    {
        var deleteProjectResult = await _projectService.DeleteAsync(projectId);

        if (deleteProjectResult.IsFailure)
        {
            var errorCode = deleteProjectResult.Error.Code;
            var errorMessage = deleteProjectResult.Error.Message;

            if (errorCode == DeleteProjectErrors.NotFound().Code) return NotFound(errorMessage);

            if (errorCode == DeleteProjectErrors.AccessDenied.Code) return Forbid();
        }

        return Ok();
    }

    private UpdateProjectDto UpdateProjectRequestToDto(long projectId, UpdateProjectRequest request)
    {
        return new UpdateProjectDto
        {
            ProjectId = projectId,
            ProjectTitle = request.Title
        };
    }

    private UpdateProjectResponse ProjectToUpdateProjectResponse(ProjectEntity project)
    {
        return new UpdateProjectResponse
        {
            Id = project.Id,
            Title = project.Title,
            LeadUserId = project.LeadUserId
        };
    }

    private GetProjectResponse ProjectToGetProjectResponse(ProjectEntity project)
    {
        return new GetProjectResponse
        {
            Id = project.Id,
            Title = project.Title,
            LeadUserId = project.LeadUserId
        };
    }

    private CreateProjectResponse ProjectToCreateProjectResponse(ProjectEntity project)
    {
        return new CreateProjectResponse
        {
            ProjectId = project.Id,
            Title = project.Title,
            LeadUserId = project.LeadUserId
        };
    }

    private CreateProjectDto CreateProjectRequestToDto(CreateProjectRequest request)
    {
        return new CreateProjectDto
        {
            Title = request.Title
        };
    }
}