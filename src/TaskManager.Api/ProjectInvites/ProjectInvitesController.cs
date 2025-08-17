using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Core.Shared;
using TaskManager.ProjectInvites.Create;
using TaskManager.ProjectInvites.Get;
using TaskManager.Shared;
using TaskManager.UseCases.Invites.Create;
using TaskManager.UseCases.Invites.Delete;
using TaskManager.UseCases.Invites.Retrieve;
using TaskManager.UseCases.Shared;

namespace TaskManager.ProjectInvites;

[Authorize]
[Route("api/projects/{projectId:long}/invites")]
[ApiController]
public class ProjectInvitesController : ControllerBase
{
    private readonly IInviteCreationService _inviteCreationService;
    private readonly IInviteDeletionService _inviteDeletionService;
    private readonly IInviteRetrievalService _inviteRetrievalService;

    public ProjectInvitesController(IInviteCreationService inviteCreationService,
        IInviteDeletionService inviteDeletionService, IInviteRetrievalService inviteRetrievalService)
    {
        _inviteCreationService = inviteCreationService;
        _inviteDeletionService = inviteDeletionService;
        _inviteRetrievalService = inviteRetrievalService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetInviteResponse>>> GetInvites([FromRoute] long projectId,
        [FromQuery] PaginationQuery paginationQuery)
    {
        var result =
            await _inviteRetrievalService.RetrievePendingProjectInvitesAsync(
                GetProjectInvitesRequestToDto(projectId, paginationQuery));

        if (result.IsFailure)
        {
            var errorCode = result.Error.Code;
            var errorMessage = result.Error.Message;

            if (errorCode == UseCaseErrors.Unauthenticated.Code) return Unauthorized();

            if (errorCode == RetrieveInvitesErrors.ProjectNotFound.Code)
                return NotFound(errorMessage);
        }

        var response = InvitesToInviteResponses(result.Value);

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CreateInviteResponse>> CreateInvite([FromRoute] long projectId,
        [FromBody] CreateInviteRequest request)
    {
        var createInviteResult = await _inviteCreationService.CreateAsync(CreateInviteRequestToDto(projectId, request));

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
                return Forbid();
        }

        var response = InviteToCreateInviteResponse(createInviteResult.Value);

        return Ok(response);
    }

    [HttpDelete("{inviteId:long}")]
    public async Task<ActionResult> DeleteInvite([FromRoute] long inviteId)
    {
        var deleteInviteResult = await _inviteDeletionService.DeleteAsync(inviteId);

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

    private PagedResponse<GetInviteResponse> InvitesToInviteResponses(PagedData<ProjectInvite> pagedInvites)
    {
        var inviteResponses = pagedInvites.Data.Select(invite => new GetInviteResponse
        {
            InviteId = invite.Id,
            ProjectId = invite.ProjectId,
            InvitedUserId = invite.InvitedUserId,
            InvitedByUserId = invite.InvitedByUserId,
            InviteStatus = invite.Status.ToString()
        });

        return new PagedResponse<GetInviteResponse>
        {
            PageNumber = pagedInvites.PageNumber,
            PageSize = pagedInvites.PageSize,
            TotalPages = pagedInvites.TotalPages,
            TotalRecords = pagedInvites.TotalRecords,
            Data = inviteResponses
        };
    }

    private RetrievePendingProjectInvitesDto GetProjectInvitesRequestToDto(long projectId,
        PaginationQuery paginationQuery)
    {
        return new RetrievePendingProjectInvitesDto
        {
            ProjectId = projectId,
            PageNumber = paginationQuery.PageNumber,
            PageSize = paginationQuery.PageSize
        };
    }
}