using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.ProjectInvites.Create;
using TaskManager.ProjectInvites.Get;
using TaskManager.UseCases.Invites;
using TaskManager.UseCases.Invites.Create;
using TaskManager.UseCases.Invites.Delete;
using TaskManager.UseCases.Invites.GetPendingForProject;
using TaskManager.UseCases.Shared;

namespace TaskManager.ProjectInvites;

[Authorize]
[Route("api/projects/{projectId:long}/invites")]
[ApiController]
public class ProjectInvitesController : ControllerBase
{
    private readonly IInviteService _inviteService;

    public ProjectInvitesController(IInviteService inviteService)
    {
        _inviteService = inviteService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetInviteResponse>>> GetInvites([FromRoute] long projectId)
    {
        var result = await _inviteService.GetPendingProjectInvitesAsync(projectId);

        if (result.IsFailure)
        {
            var errorCode = result.Error.Code;
            var errorMessage = result.Error.Message;

            if (errorCode == UseCaseErrors.Unauthenticated.Code) return Unauthorized();

            if (errorCode == GetPendingInvitesForProjectErrors.ProjectNotFound.Code)
                return NotFound(errorMessage);
        }

        var response = InvitesToInviteResponses(result.Value);

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CreateInviteResponse>> CreateInvite([FromRoute] long projectId,
        [FromBody] CreateInviteRequest request)
    {
        var createInviteResult = await _inviteService.CreateAsync(CreateInviteRequestToDto(projectId, request));

        if (createInviteResult.IsFailure)
        {
            var errorCode = createInviteResult.Error.Code;
            var errorMessage = createInviteResult.Error.Message;

            if (errorCode == UseCaseErrors.Unauthenticated.Code) return Unauthorized();

            if (errorCode == CreateInviteErrors.InvitedUserNotFound.Code ||
                errorCode == CreateInviteErrors.ProjectNotFound.Code)
                return NotFound(errorMessage);

            if (errorCode == CreateInviteErrors.UserAlreadyInvited.Code ||
                errorCode == CreateInviteErrors.InvitedUserAlreadyAMember.Code)
                return BadRequest(errorMessage);

            if (errorCode == CreateInviteErrors.AccessDenied.Code)
                return BadRequest(errorMessage);
        }

        var response = InviteToCreateInviteResponse(createInviteResult.Value);

        return Ok(response);
    }

    [HttpDelete("{inviteId:long}")]
    public async Task<ActionResult> DeleteInvite([FromRoute] long inviteId)
    {
        var deleteInviteResult = await _inviteService.DeleteAsync(inviteId);

        if (deleteInviteResult.IsFailure)
        {
            var errorCode = deleteInviteResult.Error.Code;
            var errorMessage = deleteInviteResult.Error.Message;

            if (errorCode == UseCaseErrors.Unauthenticated.Code)
                return Unauthorized();

            if (errorCode == DeleteInviteErrors.InviteNotFound.Code)
                return NotFound(errorMessage);

            if (errorCode == DeleteInviteErrors.AccessDenied.Code)
                return Forbid();
        }

        return Ok();
    }

    private CreateInviteDto CreateInviteRequestToDto(long projectId, CreateInviteRequest request)
    {
        var dto = new CreateInviteDto
        {
            ProjectId = projectId,
            InvitedUserId = request.InvitedUserId
        };

        return dto;
    }

    private CreateInviteResponse InviteToCreateInviteResponse(ProjectInvite invite)
    {
        var response = new CreateInviteResponse
        {
            InviteId = invite.Id,
            InvitedUserId = invite.InvitedUserId,
            InvitedByUserId = invite.InvitedByUserId,
            Status = invite.Status.ToString()
        };

        return response;
    }

    private IEnumerable<GetInviteResponse> InvitesToInviteResponses(IEnumerable<ProjectInvite> invites)
    {
        return invites.Select(invite => new GetInviteResponse
        {
            InviteId = invite.Id,
            ProjectId = invite.ProjectId,
            InvitedUserId = invite.InvitedUserId,
            InvitedByUserId = invite.InvitedByUserId,
            InviteStatus = invite.Status.ToString()
        });
    }
}