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
using TaskManager.UseCases.Shared;

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
    public async Task<ActionResult<IEnumerable<GetProjectResponse>>> GetAll(
        [FromQuery] RoleQueryParameter? role = null)
    {
        var result = await _projectService.GetAllByUserAsync(RoleQueryParameterToDto(role));

        if (result.IsFailure)
        {
            var errorCode = result.Error.Code;
            var errorMessage = result.Error.Message;

            if (errorCode == UseCaseErrors.Unauthenticated.Code) return Unauthorized();
        }

        var userProjectsResponse = result.Value.Select(ProjectToGetProjectResponse);

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

            if (errorCode == UseCaseErrors.Unauthenticated.Code) return Unauthorized();

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

        if (createProjectResult.IsFailure)
        {
            var errorCode = createProjectResult.Error.Code;
            var errorMessage = createProjectResult.Error.Message;

            if (errorCode == UseCaseErrors.Unauthenticated.Code) return Unauthorized(errorMessage);

            if (errorCode == GetProjectErrors.NotFound().Code) return NotFound(errorMessage);

            if (errorCode == GetProjectErrors.AccessDenied.Code) return Forbid();
        }

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

            if (errorCode == UseCaseErrors.Unauthenticated.Code) return Unauthorized();

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

            if (errorCode == UseCaseErrors.Unauthenticated.Code) return Unauthorized();

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

    private RoleDto RoleQueryParameterToDto(RoleQueryParameter? role)
    {
        return role switch
        {
            RoleQueryParameter.Member => RoleDto.Member,
            RoleQueryParameter.Manager => RoleDto.Manager,
            RoleQueryParameter.Lead => RoleDto.Lead,
            null => RoleDto.Any,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
        };
    }
}