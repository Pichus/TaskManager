using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Infrastructure.Identity.User;
using TaskManager.Profile.GetInvite;
using TaskManager.Profile.GetProfileDetails;
using TaskManager.UseCases.Invites;
using TaskManager.UseCases.Invites.Accept;
using TaskManager.UseCases.Profile.ProfileDetails;

namespace TaskManager.Profile;

[Authorize]
[Route("api/profile")]
[ApiController]
public class ProfileController : ControllerBase
{
    private readonly IInviteService _inviteService;
    private readonly IProfileDetailsService _profileDetailsService;

    public ProfileController(IInviteService inviteService, IProfileDetailsService profileDetailsService)
    {
        _inviteService = inviteService;
        _profileDetailsService = profileDetailsService;
    }

    [HttpGet]
    public async Task<ActionResult<GetProfileDetailsResponse>> GetProfileDetails()
    {
        var result = await _profileDetailsService.GetCurrentUserProfileDetailsAsync();

        if (result.IsFailure)
        {
            var errorCode = result.Error.Code;
            var errorMessage = result.Error.Message;

            return BadRequest(errorMessage);
        }

        var response = UserToGetProfileDetailsResponse(result.Value);

        return Ok(response);
    }

    [HttpGet("invites/pending")]
    public async Task<ActionResult<IEnumerable<GetInviteResponse>>> GetPendingInvites()
    {
        var result = await _inviteService.GetPendingInvitesForCurrentUser();

        if (result.IsFailure)
        {
            var errorCode = result.Error.Code;
            var errorMessage = result.Error.Message;

            return BadRequest(errorMessage);
        }

        var response = InvitesToGetInviteResponses(result.Value);

        return Ok(response);
    }

    [HttpPost("invites/{inviteId}/accept")]
    public async Task<IActionResult> AcceptInvite(long inviteId)
    {
        var result = await _inviteService.AcceptInviteAsync(inviteId);

        if (result.IsFailure)
        {
            var errorCode = result.Error.Code;
            var errorMessage = result.Error.Message;

            if (errorCode == AcceptInviteErrors.Unauthenticated.Code)
                return Unauthorized();
            
            if (errorCode == AcceptInviteErrors.InviteNotFound.Code ||
                errorCode == AcceptInviteErrors.ProjectNotFound.Code)
                return NotFound(errorMessage);

            if (errorCode == AcceptInviteErrors.InviteAlreadyAccepted.Code ||
                errorCode == AcceptInviteErrors.InviteAlreadyRejected.Code ||
                errorCode == AcceptInviteErrors.InvitedUserAlreadyAMember.Code)
                return BadRequest(errorMessage);

            if (errorCode == AcceptInviteErrors.AccessDenied.Code)
                return Forbid(errorMessage);
        }

        return Ok();
    }

    [HttpPost("invites/{inviteId}/decline")]
    public async Task<IActionResult> DeclineInvite(long inviteId)
    {
        var result = await _inviteService.DeclineInviteAsync(inviteId);

        if (result.IsFailure)
        {
            var errorCode = result.Error.Code;
            var errorMessage = result.Error.Message;

            return BadRequest(errorMessage);
        }

        var response = "";
        return Ok(response);
    }

    [HttpGet("invites/history")]
    public async Task<IActionResult> GetInviteHistory()
    {
        return Ok();
    }

    private GetProfileDetailsResponse UserToGetProfileDetailsResponse(TaskManagerUser user)
    {
        return new GetProfileDetailsResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email
        };
    }

    private IEnumerable<GetInviteResponse> InvitesToGetInviteResponses(IEnumerable<ProjectInvite> invites)
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