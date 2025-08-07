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

        var userProjectsResponse = userProjects.Select(project => new GetProjectResponse
        {
            Id = project.Id,
            Title = project.Title,
            LeadUserId = project.LeadUserId
        });

        return Ok(userProjectsResponse);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetProjectResponse>> Get(long id)
    {
        var getProjectResult = await _projectService.GetByIdAsync(id);

        if (!getProjectResult.Success)
        {
            var errorCode = getProjectResult.Error.Code;
            var errorMessage = getProjectResult.Error.Message;

            if (errorCode == GetProjectErrors.NotFound().Code) return NotFound(errorMessage);

            if (errorCode == GetProjectErrors.AccessDenied.Code) return Forbid();
        }

        var response = new GetProjectResponse
        {
            Id = getProjectResult.Project.Id,
            Title = getProjectResult.Project.Title,
            LeadUserId = getProjectResult.Project.LeadUserId
        };

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CreateProjectResponse>> Create(CreateProjectRequest request)
    {
        var createProjectResult = await _projectService.CreateAsync(new CreateProjectDto
        {
            Title = request.Title
        });

        var response = new CreateProjectResponse
        {
            ProjectId = createProjectResult.Project.Id,
            Title = createProjectResult.Project.Title,
            LeadUserId = createProjectResult.Project.LeadUserId
        };
        
        return CreatedAtAction(nameof(Create), response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UpdateProjectResponse>> Put([FromRoute] long id,
        [FromBody] UpdateProjectRequest request)
    {
        var updateProjectResult = await _projectService.UpdateAsync(UpdateProjectRequestToDto(id, request));

        if (!updateProjectResult.Success)
        {
            var errorCode = updateProjectResult.Error.Code;
            var errorMessage = updateProjectResult.Error.Message;

            if (errorCode == UpdateProjectErrors.NotFound().Code) return NotFound(errorMessage);

            if (errorCode == UpdateProjectErrors.AccessDenied.Code) return Forbid();
        }

        var response = ProjectToProjectResponse(updateProjectResult.Project);

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(long id)
    {
        var deleteProjectResult = await _projectService.DeleteAsync(id);
        
        if (!deleteProjectResult.Success)
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
        var dto = new UpdateProjectDto
        {
            ProjectId = projectId,
            ProjectTitle = request.Title
        };

        return dto;
    }
    
    private UpdateProjectResponse ProjectToProjectResponse(ProjectEntity project)
    {
        var response = new UpdateProjectResponse
        {
            Id = project.Id,
            Title = project.Title,
            LeadUserId = project.LeadUserId
        };

        return response;
    }
}