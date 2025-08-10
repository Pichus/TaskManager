using Microsoft.AspNetCore.Mvc;
using TaskManager.Core.ProjectAggregate;
using TaskManager.ProjectMembers.Update;
using TaskManager.UseCases.ProjectMembers;
using TaskManager.UseCases.ProjectMembers.Get;
using TaskManager.UseCases.Shared;

namespace TaskManager.ProjectMembers;

[Route("api/projects/{projectId:long}/members")]
[ApiController]
public class ProjectMembersController : ControllerBase
{
    private readonly IProjectMemberService _projectMemberService;

    public ProjectMembersController(IProjectMemberService projectMemberService)
    {
        _projectMemberService = projectMemberService;
    }

    [HttpGet]
    public async Task<ActionResult<ProjectMemberWithUser>> GetAll([FromRoute] long projectId)
    {
        var result = await _projectMemberService.GetProjectMembersAsync(projectId);

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
        var result = await _projectMemberService.GetProjectMembersAsync(projectId);

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

    [HttpDelete("{memberId:long}")]
    public async Task<ActionResult> Delete([FromRoute] long projectId, [FromRoute] long memberId)
    {
        // delete member
        return Ok();
    }
}