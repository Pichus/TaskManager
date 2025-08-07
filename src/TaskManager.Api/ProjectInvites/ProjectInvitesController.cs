using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.ProjectInvites.Create;
using TaskManager.UseCases.Invites;
using TaskManager.UseCases.Invites.Create;
using TaskManager.UseCases.Invites.Delete;

namespace TaskManager.ProjectInvites;

[Authorize]
[Route("api/projects/{projectId}/invites")]
[ApiController]
public class ProjectInvitesController : ControllerBase
{
    private readonly IInviteService _inviteService;

    public ProjectInvitesController(IInviteService inviteService)
    {
        _inviteService = inviteService;
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

            if (errorCode == CreateInviteErrors.InvitedUserNotFound.Code ||
                errorCode == CreateInviteErrors.ProjectNotFound.Code)
                return NotFound(errorMessage);

            if (errorCode == CreateInviteErrors.UserAlreadyInvited.Code) return BadRequest(errorMessage);
        }

        var response = InviteToInviteResponse(createInviteResult.Value);

        return response;
    }

    [HttpDelete("{inviteId}")]
    public async Task<ActionResult> DeleteInvite([FromRoute] long inviteId)
    {
        var deleteInviteResult = await _inviteService.DeleteAsync(inviteId);
        
        if (deleteInviteResult.IsFailure)
        {
            var errorCode = deleteInviteResult.Error.Code;
            var errorMessage = deleteInviteResult.Error.Message;

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

    private CreateInviteResponse InviteToInviteResponse(ProjectInvite invite)
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
}