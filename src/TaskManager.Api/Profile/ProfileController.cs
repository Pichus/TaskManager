using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Infrastructure.Identity.User;
using TaskManager.Profile.GetInvite;
using TaskManager.Profile.GetProfileDetails;
using TaskManager.UseCases.Invites.Response;
using TaskManager.UseCases.Invites.Response.Accept;
using TaskManager.UseCases.Invites.Response.Decline;
using TaskManager.UseCases.Invites.Retrieve;
using TaskManager.UseCases.ProfileDetails;
using TaskManager.UseCases.Shared;

namespace TaskManager.Profile;

[Authorize]
[Route("api/profile")]
[ApiController]
public class ProfileController : ControllerBase
{
    private readonly IInviteResponseService _inviteResponseService;
    private readonly IInviteRetrievalService _inviteRetrievalService;
    private readonly IProfileDetailsService _profileDetailsService;

    public ProfileController(IProfileDetailsService profileDetailsService,
        IInviteResponseService inviteResponseService, IInviteRetrievalService inviteRetrievalService)
    {
        _profileDetailsService = profileDetailsService;
        _inviteResponseService = inviteResponseService;
        _inviteRetrievalService = inviteRetrievalService;
    }

    [HttpGet("/me")]
    public async Task<ActionResult<GetProfileDetailsResponse>> GetProfileDetails()
    {
        var result = await _profileDetailsService.GetCurrentUserProfileDetailsAsync();

        if (result.IsFailure)
        {
            var errorCode = result.Error.Code;
            var errorMessage = result.Error.Message;

            if (errorCode == UseCaseErrors.Unauthenticated.Code)
                return Unauthorized();
        }

        var response = UserToGetProfileDetailsResponse(result.Value);

        return Ok(response);
    }

    [HttpGet("invites/pending")]
    public async Task<ActionResult<IEnumerable<GetInviteResponse>>> GetPendingInvites()
    {
        var result = await _inviteRetrievalService.RetrievePendingInvitesForCurrentUserAsync();

        if (result.IsFailure)
        {
            var errorCode = result.Error.Code;
            var errorMessage = result.Error.Message;

            if (errorCode == UseCaseErrors.Unauthenticated.Code)
                return Unauthorized();
        }

        var response = InvitesToGetInviteResponses(result.Value);

        return Ok(response);
    }

    [HttpPut("invites/{inviteId:long}/accept")]
    public async Task<ActionResult> AcceptInvite(long inviteId)
    {
        var result = await _inviteResponseService.AcceptInviteAsync(inviteId);

        if (result.IsFailure)
        {
            var errorCode = result.Error.Code;
            var errorMessage = result.Error.Message;

            if (errorCode == UseCaseErrors.Unauthenticated.Code)
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

    [HttpPut("invites/{inviteId:long}/decline")]
    public async Task<ActionResult> DeclineInvite(long inviteId)
    {
        var result = await _inviteResponseService.DeclineInviteAsync(inviteId);

        if (result.IsFailure)
        {
            var errorCode = result.Error.Code;
            var errorMessage = result.Error.Message;

            if (errorCode == UseCaseErrors.Unauthenticated.Code)
                return Unauthorized();

            if (errorCode == DeclineInviteErrors.InviteNotFound.Code ||
                errorCode == DeclineInviteErrors.ProjectNotFound.Code)
                return NotFound(errorMessage);

            if (errorCode == DeclineInviteErrors.InviteAlreadyAccepted.Code ||
                errorCode == DeclineInviteErrors.InviteAlreadyRejected.Code)
                return BadRequest(errorMessage);

            if (errorCode == DeclineInviteErrors.AccessDenied.Code)
                return Forbid(errorMessage);
        }

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