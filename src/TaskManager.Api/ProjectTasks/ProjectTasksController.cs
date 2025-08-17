using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Core.TaskAggregate;
using TaskManager.ProjectTasks.Create;
using TaskManager.UseCases.Shared;
using TaskManager.UseCases.Tasks.Create;

namespace TaskManager.ProjectTasks;

[Authorize]
[Route("api/projects/{projectId:long}/tasks")]
[ApiController]
public class ProjectTasksController : ControllerBase
{
    private readonly ITaskCreationService _taskCreationService;

    public ProjectTasksController(ITaskCreationService taskCreationService)
    {
        _taskCreationService = taskCreationService;
    }

    [HttpPost]
    public async Task<ActionResult<CreateTaskResponse>> Create([FromRoute] long projectId,
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

        var response = TaskToCreateTaskResponse(result.Value);

        return CreatedAtAction(nameof(Create), response);
    }

    private CreateTaskResponse TaskToCreateTaskResponse(TaskEntity task)
    {
        return new CreateTaskResponse
        {
            Id = task.Id,
            ProjectId = task.ProjectId,
            CreatedByUserId = task.CreatedByUserId,
            AssigneeUserId = task.AssigneeUserId,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.ToString(),
            DueDate = task.DueDate
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
}