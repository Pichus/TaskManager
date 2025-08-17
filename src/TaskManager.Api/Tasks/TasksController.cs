using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Core.Shared;
using TaskManager.Core.TaskAggregate;
using TaskManager.Shared;
using TaskManager.Tasks.Get;
using TaskManager.Tasks.Update;
using TaskManager.UseCases.Shared;
using TaskManager.UseCases.Tasks.Delete;
using TaskManager.UseCases.Tasks.Retrieve;
using TaskManager.UseCases.Tasks.Update;

namespace TaskManager.Tasks;

[Authorize]
[Route("api/tasks")]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly ITaskDeletionService _taskDeletionService;
    private readonly ITaskRetrievalService _taskRetrievalService;
    private readonly ITaskUpdateService _taskUpdateService;

    public TasksController(ITaskRetrievalService taskRetrievalService, ITaskUpdateService taskUpdateService,
        ITaskDeletionService taskDeletionService)
    {
        _taskRetrievalService = taskRetrievalService;
        _taskUpdateService = taskUpdateService;
        _taskDeletionService = taskDeletionService;
    }

    [HttpGet("{taskId:long}")]
    public async Task<ActionResult<GetTaskResponse>> Get(long taskId)
    {
        var result = await _taskRetrievalService.RetrieveByProjectIdAndTaskIdAsync(0, taskId);

        if (result.IsFailure)
        {
            var errorCode = result.Error.Code;
            var errorMessage = result.Error.Message;

            if (errorCode == UseCaseErrors.Unauthenticated.Code)
                return Unauthorized();

            if (errorCode == RetrieveTaskErrors.ProjectNotFound.Code
                || errorCode == RetrieveTaskErrors.TaskNotFound.Code)
                return NotFound(errorMessage);

            if (errorCode == RetrieveTaskErrors.AccessDenied.Code)
                return Forbid();
        }

        var response = TaskToGetTaskResponse(result.Value);

        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<GetTaskResponse>>> GetAll(
        [FromQuery] long? projectId = null,
        [FromQuery] string? assigneeUserId = null,
        [FromQuery] Status? status = null,
        [FromQuery] SortQueryParameter? sort = SortQueryParameter.Id,
        [FromQuery] OrderQueryParameter? order = OrderQueryParameter.Asc,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25)
    {
        if (projectId is null && assigneeUserId is null)
            return BadRequest();

        var dto = RetrieveAllTasksRequestToDto(projectId, assigneeUserId, status, sort, order, pageNumber, pageSize);

        var result = await _taskRetrievalService.RetrieveAll(dto);

        if (result.IsFailure)
        {
            var errorCode = result.Error.Code;
            var errorMessage = result.Error.Message;

            if (errorCode == UseCaseErrors.Unauthenticated.Code)
                return Unauthorized();

            if (errorCode == RetrieveTaskErrors.ProjectNotFound.Code
                || errorCode == RetrieveTaskErrors.AssigneeUserNotFound.Code)
                return NotFound(errorMessage);

            if (errorCode == RetrieveTaskErrors.AccessDenied.Code)
                return Forbid();
        }

        var response = PagedTasksToResponse(result.Value);
        
        return Ok(response);
    }

    [HttpPut("{taskId:long}")]
    public async Task<ActionResult> Update([FromRoute] long taskId, [FromBody] UpdateTaskRequest request)
    {
        var result = await _taskUpdateService.UpdateAsync(UpdateTaskRequestToDto(0, taskId, request));

        if (result.IsFailure)
        {
            var errorCode = result.Error.Code;
            var errorMessage = result.Error.Message;

            if (errorCode == UseCaseErrors.Unauthenticated.Code)
                return Unauthorized();

            if (errorCode == UpdateTaskErrors.ProjectNotFound.Code
                || errorCode == UpdateTaskErrors.TaskNotFound.Code)
                return NotFound(errorMessage);

            if (errorCode == UpdateTaskErrors.AccessDenied.Code)
                return Forbid();
        }

        return Ok();
    }

    [HttpDelete("{taskId:long}")]
    public async Task<ActionResult> Delete([FromRoute] long projectId, [FromRoute] long taskId)
    {
        var result = await _taskDeletionService.DeleteAsync(projectId, taskId);

        if (result.IsFailure)
        {
            var errorCode = result.Error.Code;
            var errorMessage = result.Error.Message;

            if (errorCode == UseCaseErrors.Unauthenticated.Code)
                return Unauthorized();

            if (errorCode == DeleteTaskErrors.ProjectNotFound.Code
                || errorCode == DeleteTaskErrors.TaskNotFound.Code)
                return NotFound(errorMessage);

            if (errorCode == DeleteTaskErrors.AccessDenied.Code)
                return Forbid();
        }

        return Ok();
    }

    [HttpPut("{taskId:long}/status")]
    public async Task<ActionResult> UpdateStatus([FromRoute] long taskId, [FromQuery] Status status)
    {
        var result = await _taskUpdateService.UpdateStatusAsync(0, taskId, status);

        if (result.IsFailure)
        {
            var errorCode = result.Error.Code;
            var errorMessage = result.Error.Message;

            if (errorCode == UseCaseErrors.Unauthenticated.Code)
                return Unauthorized();

            if (errorCode == UpdateTaskErrors.ProjectNotFound.Code
                || errorCode == UpdateTaskErrors.TaskNotFound.Code)
                return NotFound(errorMessage);

            if (errorCode == UpdateTaskErrors.AccessDenied.Code)
                return Forbid();

            if (errorCode == UpdateTaskErrors.StatusAlreadySet.Code)
                return BadRequest(errorMessage);
        }

        return Ok();
    }

    private GetTaskResponse TaskToGetTaskResponse(TaskEntity task)
    {
        return new GetTaskResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.ToString(),
            DueDate = task.DueDate,
            CreatedByUserId = task.CreatedByUserId,
            AssigneeUserId = task.AssigneeUserId,
            ProjectId = task.ProjectId
        };
    }

    private UpdateTaskDto UpdateTaskRequestToDto(long projectId, long taskId, UpdateTaskRequest request)
    {
        return new UpdateTaskDto
        {
            ProjectId = projectId,
            TaskId = taskId,
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate
        };
    }

    private RetrieveAllTasksDto RetrieveAllTasksRequestToDto(long? projectId, string? assigneeUserId, Status? status,
        SortQueryParameter? sortQuery, OrderQueryParameter? orderQuery, int pageNumber, int pageSize)
    {
        var sortDto = sortQuery switch
        {
            SortQueryParameter.Id => SortDto.Id,
            SortQueryParameter.Title => SortDto.Title,
            SortQueryParameter.DueDate => SortDto.Title,
            _ => SortDto.Default
        };

        var orderDto = orderQuery switch
        {
            OrderQueryParameter.Asc => OrderDto.Asc,
            OrderQueryParameter.Desc => OrderDto.Desc,
            _ => OrderDto.Default
        };

        return new RetrieveAllTasksDto
        {
            ProjectId = projectId,
            AssigneeUserId = assigneeUserId,
            Status = status,
            Sort = sortDto,
            Order = orderDto,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
    
    private PagedResponse<GetTaskResponse> PagedTasksToResponse(PagedData<TaskEntity> pagedTasks)
    {
        var taskResponses = pagedTasks.Data.Select(task => new GetTaskResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.ToString(),
            DueDate = task.DueDate,
            CreatedByUserId = task.CreatedByUserId,
            AssigneeUserId = task.AssigneeUserId,
            ProjectId = task.ProjectId
        });

        return new PagedResponse<GetTaskResponse>
        {
            PageNumber = pagedTasks.PageNumber,
            PageSize = pagedTasks.PageSize,
            TotalPages = pagedTasks.TotalPages,
            TotalRecords = pagedTasks.TotalRecords,
            Data = taskResponses
        };
    }
}