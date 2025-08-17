using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Core.TaskAggregate;
using TaskManager.ProjectTasks.Create;
using TaskManager.ProjectTasks.Get;
using TaskManager.UseCases.Shared;
using TaskManager.UseCases.Tasks.Create;
using TaskManager.UseCases.Tasks.Delete;
using TaskManager.UseCases.Tasks.Retrieve;
using TaskManager.UseCases.Tasks.Update;

namespace TaskManager.ProjectTasks;

[Authorize]
[Route("api/projects/{projectId:long}/tasks")]
[ApiController]
public class ProjectTasksController : ControllerBase
{
    private readonly ITaskCreationService _taskCreationService;
    private readonly ITaskDeletionService _taskDeletionService;
    private readonly ITaskRetrievalService _taskRetrievalService;
    private readonly ITaskUpdateService _taskUpdateService;

    public ProjectTasksController(ITaskRetrievalService taskRetrievalService, ITaskCreationService taskCreationService,
        ITaskDeletionService taskDeletionService, ITaskUpdateService taskUpdateService)
    {
        _taskRetrievalService = taskRetrievalService;
        _taskCreationService = taskCreationService;
        _taskDeletionService = taskDeletionService;
        _taskUpdateService = taskUpdateService;
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

    private IEnumerable<GetTaskResponse> TasksToResponses(IEnumerable<TaskEntity> tasks)
    {
        return tasks.Select(TaskToGetTaskResponse);
    }
}