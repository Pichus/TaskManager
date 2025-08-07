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

    [HttpGet("{id}")]
    public async Task<ActionResult<GetProjectResponse>> Get(long id)
    {
        var getProjectResult = await _projectService.GetByIdAsync(id);

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

    [HttpPost]
    public async Task<ActionResult<CreateProjectResponse>> Create(CreateProjectRequest request)
    {
        var createProjectResult = await _projectService.CreateAsync(CreateProjectRequestToDto(request));

        var response = ProjectToCreateProjectResponse(createProjectResult.Value);

        return CreatedAtAction(nameof(Create), response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UpdateProjectResponse>> Put([FromRoute] long id,
        [FromBody] UpdateProjectRequest request)
    {
        var updateProjectResult = await _projectService.UpdateAsync(UpdateProjectRequestToDto(id, request));

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

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(long id)
    {
        var deleteProjectResult = await _projectService.DeleteAsync(id);

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