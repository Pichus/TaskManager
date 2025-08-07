using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.ProjectInvites.Create;
using TaskManager.UseCases.Invites;
using TaskManager.UseCases.Invites.Create;

namespace TaskManager.ProjectInvites;

[Authorize]
[Route("api/projects/{id}/invites")]
[ApiController]
public class ProjectInvitesController : ControllerBase
{
    private readonly IInviteService _inviteService;

    public ProjectInvitesController(IInviteService inviteService)
    {
        _inviteService = inviteService;
    }

    [HttpPost]
    public async Task<ActionResult<CreateInviteResponse>> CreateInvite(CreateInviteRequest request)
    {
        var createInviteResult = await _inviteService.CreateAsync(CreateInviteRequestToDto(request));

        if (!createInviteResult.Success)
        {
            var errorCode = createInviteResult.Error.Code;
            var errorMessage = createInviteResult.Error.Message;

            if (errorCode == CreateInviteErrors.InvitedUserNotFound.Code ||
                errorCode == CreateInviteErrors.ProjectNotFound.Code)
                return NotFound(errorMessage);

            if (errorCode == CreateInviteErrors.UserAlreadyInvited.Code) return BadRequest(errorMessage);
        }

        var response = InviteToInviteResponse(createInviteResult.Invite);

        return response;
    }

    private CreateInviteDto CreateInviteRequestToDto(CreateInviteRequest request)
    {
        var dto = new CreateInviteDto
        {
            ProjectId = request.ProjectId,
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