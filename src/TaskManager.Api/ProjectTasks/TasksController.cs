using Microsoft.AspNetCore.Mvc;
using TaskManager.Core.TaskAggregate;
using TaskManager.ProjectTasks.Create;
using TaskManager.ProjectTasks.Get;
using TaskManager.ProjectTasks.Update;
using TaskManager.UseCases.Shared;
using TaskManager.UseCases.Tasks.Create;
using TaskManager.UseCases.Tasks.Delete;
using TaskManager.UseCases.Tasks.Retrieve;
using TaskManager.UseCases.Tasks.Update;

namespace TaskManager.ProjectTasks;

[Route("api/projects/{projectId:long}/tasks")]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly ITaskCreationService _taskCreationService;
    private readonly ITaskDeletionService _taskDeletionService;
    private readonly ITaskRetrievalService _taskRetrievalService;
    private readonly ITaskUpdateService _taskUpdateService;

    public TasksController(ITaskRetrievalService taskRetrievalService, ITaskCreationService taskCreationService,
        ITaskDeletionService taskDeletionService, ITaskUpdateService taskUpdateService)
    {
        _taskRetrievalService = taskRetrievalService;
        _taskCreationService = taskCreationService;
        _taskDeletionService = taskDeletionService;
        _taskUpdateService = taskUpdateService;
    }

    [HttpGet("{taskId:long}")]
    public async Task<ActionResult<GetTaskResponse>> Get([FromRoute] long projectId, [FromRoute] long taskId)
    {
        var result = await _taskRetrievalService.RetrieveByProjectIdAndTaskIdAsync(projectId, taskId);

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
    public async Task<ActionResult<IEnumerable<GetTaskResponse>>> Get([FromRoute] long projectId,
        [FromQuery] Status? status)
    {
        var result = await _taskRetrievalService.RetrieveAllByProjectIdAndStatusAsync(projectId, StatusToDto(status));

        if (result.IsFailure)
        {
            var errorCode = result.Error.Code;
            var errorMessage = result.Error.Message;

            if (errorCode == UseCaseErrors.Unauthenticated.Code)
                return Unauthorized();

            if (errorCode == RetrieveTaskErrors.ProjectNotFound.Code)
                return NotFound(errorMessage);

            if (errorCode == RetrieveTaskErrors.AccessDenied.Code)
                return Forbid();
        }

        var response = TasksToResponses(result.Value);

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<GetTaskResponse>> Create([FromRoute] long projectId,
        [FromBody] CreateTaskRequest request)
    {
        var result = await _taskCreationService.CreateAsync(CreateTaskRequestToDto(projectId, request));

        if (result.IsFailure)
        {
            var errorCode = result.Error.Code;
            var errorMessage = result.Error.Message;

            if (errorCode == UseCaseErrors.Unauthenticated.Code)
                return Unauthorized();

            if (errorCode == CreateTaskErrors.ProjectNotFound.Code)
                return NotFound(errorMessage);

            if (errorCode == CreateTaskErrors.AccessDenied.Code)
                return Forbid();
        }

        var response = TaskToGetTaskResponse(result.Value);

        return CreatedAtAction(nameof(Create), response);
    }

    [HttpPut("{taskId:long}")]
    public async Task<ActionResult> Update([FromRoute] long projectId, [FromRoute] long taskId,
        [FromBody] UpdateTaskRequest request)
    {
        var result = await _taskUpdateService.UpdateAsync(UpdateTaskRequestToDto(projectId, taskId, request));

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

    [HttpPut("{taskId:long}/status")]
    public async Task<ActionResult> UpdateStatus([FromRoute] long projectId, [FromRoute] long taskId,
        [FromQuery] Status status)
    {
        var result = await _taskUpdateService.UpdateStatusAsync(projectId, taskId, status);

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

    private StatusDto StatusToDto(Status? status)
    {
        return status switch
        {
            Status.ToDo => StatusDto.ToDo,
            Status.InProgress => StatusDto.InProgress,
            Status.Complete => StatusDto.Complete,
            null => StatusDto.Any,
            _ => StatusDto.Any
        };
    }

    private CreateTaskDto CreateTaskRequestToDto(long projectId, CreateTaskRequest request)
    {
        return new CreateTaskDto
        {
            ProjectId = projectId,
            AssigneeUserId = request.AssigneeUserId,
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate
        };
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

    private IEnumerable<GetTaskResponse> TasksToResponses(IEnumerable<TaskEntity> tasks)
    {
        return tasks.Select(TaskToGetTaskResponse);
    }
}