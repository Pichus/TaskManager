using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Core.ProjectAggregate;
using TaskManager.ProjectMembers.Get;
using TaskManager.ProjectMembers.Update;
using TaskManager.UseCases.ProjectMembers;
using TaskManager.UseCases.ProjectMembers.Delete;
using TaskManager.UseCases.ProjectMembers.Get;
using TaskManager.UseCases.ProjectMembers.Update;
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
    public async Task<ActionResult<IEnumerable<GetProjectMemberResponse>>> GetAll([FromRoute] long projectId)
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

        var response = 
            ProjectMembersWithUsersToGetProjectMembersResponses(result.Value);

        return Ok(response);
    }

    private IEnumerable<GetProjectMemberResponse> ProjectMembersWithUsersToGetProjectMembersResponses(
        IEnumerable<ProjectMemberWithUser> projectMembersWithUsers)
    {
        return projectMembersWithUsers.Select(ProjectMemberWithUserToGetProjectMemberResponse);
    }

    private GetProjectMemberResponse ProjectMemberWithUserToGetProjectMemberResponse(
        ProjectMemberWithUser projectMemberWithUser)
    {
        return new GetProjectMemberResponse
        {
            UserId = projectMemberWithUser.UserId,
            UserName = projectMemberWithUser.UserName,
            Email = projectMemberWithUser.Email,
            ProjectRole = projectMemberWithUser.ProjectRole.ToString()
        };
    }

    [AllowAnonymous]
    [HttpPut("{memberId:guid}")]
    public async Task<ActionResult> Update([FromRoute] long projectId, [FromRoute] string memberId,
        [FromBody] UpdateProjectMemberRequest request)
    {
        var result = await _projectMemberService.UpdateProjectMemberAsync(projectId, memberId, request.ProjectRole);

        if (result.IsFailure)
        {
            var errorCode = result.Error.Code;
            var errorMessage = result.Error.Message;

            if (errorCode == UseCaseErrors.Unauthenticated.Code) return Unauthorized();

            if (errorCode == UpdateProjectMemberErrors.MemberNotFound.Code
                || errorCode == UpdateProjectMemberErrors.ProjectNotFound.Code)
                return NotFound(errorMessage);

            if (errorCode == UpdateProjectMemberErrors.AccessDenied.Code) return Forbid();

            if (errorCode == UpdateProjectMemberErrors.UserIsNotAProjectMember.Code
                || errorCode == UpdateProjectMemberErrors.MemberAlreadyHasThisRole.Code)
                return BadRequest(errorMessage);
        }

        return Ok();
    }

    [HttpDelete("{memberId:guid}")]
    public async Task<ActionResult> Delete([FromRoute] long projectId, [FromRoute] string memberId)
    {
        var result = await _projectMemberService.DeleteAsync(projectId, memberId);

        if (result.IsFailure)
        {
            var errorCode = result.Error.Code;
            var errorMessage = result.Error.Message;

            if (errorCode == UseCaseErrors.Unauthenticated.Code) return Unauthorized();

            if (errorCode == DeleteProjectMemberErrors.ProjectNotFound.Code
                || errorCode == DeleteProjectMemberErrors.ProjectNotFound.Code)
                return NotFound(errorMessage);

            if (errorCode == DeleteProjectMemberErrors.AccessDenied.Code) return Forbid();
        }

        return Ok();
    }
}