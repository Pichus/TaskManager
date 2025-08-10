using Microsoft.AspNetCore.Mvc;
using TaskManager.ProjectMembers.Update;
using TaskManager.UseCases.Projects;
using TaskManager.UseCases.Projects.GetMembers;
using TaskManager.UseCases.Shared;

namespace TaskManager.ProjectMembers;

[Route("api/projects/{projectId:long}/members")]
[ApiController]
public class ProjectMembersController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectMembersController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet("{memberId:long}")]
    public async Task<ActionResult> Get([FromRoute] long projectId, [FromRoute] long memberId)
    {
        // get project member
        return Ok();
    }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromRoute] long projectId)
    {
        var result = await _projectService.GetProjectMembersAsync(projectId);

        if (result.IsFailure)
        {
            var errorCode = result.Error.Code;
            var errorMessage = result.Error.Message;

            if (errorCode == UseCaseErrors.Unauthenticated.Code) return Unauthorized();

            if (errorCode == GetProjectMembersErrors.ProjectNotFound.Code) return NotFound(errorMessage);

            if (errorCode == GetProjectMembersErrors.AccessDenied.Code) return Forbid();
        }

        return Ok(result.Value);
    }

    [HttpPut("{memberId:long}")]
    public async Task<ActionResult> Update([FromRoute] long projectId, [FromRoute] long memberId,
        [FromBody] UpdateProjectMemberRequest request)
    {
        // update member's role
        return Ok();
    }

    [HttpDelete("{memberId:long}")]
    public async Task<ActionResult> Delete([FromRoute] long projectId, [FromRoute] long memberId)
    {
        // delete member
        return Ok();
    }
}