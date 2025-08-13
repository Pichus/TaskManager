using Microsoft.Extensions.Logging;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.TaskAggregate;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.UseCases.Shared;

namespace TaskManager.UseCases.Tasks.Create;

public class TaskCreationService : ITaskCreationService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly AppDbContext _dbContext;
    private readonly ILogger _logger;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ITaskRepository _taskRepository;

    public TaskCreationService(ILogger logger, ICurrentUserService currentUserService,
        IProjectMemberRepository projectMemberRepository, IProjectRepository projectRepository,
        ITaskRepository taskRepository, AppDbContext dbContext)
    {
        _logger = logger;
        _currentUserService = currentUserService;
        _projectMemberRepository = projectMemberRepository;
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _dbContext = dbContext;
    }

    public async Task<Result<TaskEntity>> CreateAsync(CreateTaskDto createTaskDto)
    {
        _logger.LogInformation("Creating a task for Project: {ProjectId}", createTaskDto.ProjectId);

        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            _logger.LogWarning("Creating a task failed - user not authenticated for ProjectId: {ProjectId}",
                createTaskDto.ProjectId);
            return Result<TaskEntity>.Failure(UseCaseErrors.Unauthenticated);
        }

        var project = await _projectRepository.FindByIdAsync(createTaskDto.ProjectId);

        if (project is null)
        {
            _logger.LogWarning("Creating a task failed - Project: {ProjectId} not found", createTaskDto.ProjectId);
            return Result<TaskEntity>.Failure(CreateTaskErrors.ProjectNotFound);
        }

        var canCurrentUserCanCreateTask =
            await _projectMemberRepository.IsUserProjectLeadAsync(currentUserId, project.Id) ||
            await _projectMemberRepository.IsUserProjectManagerAsync(currentUserId,
                project.Id);

        if (!canCurrentUserCanCreateTask)
        {
            _logger.LogWarning("Creating a task failed - access denied");
            return Result<TaskEntity>.Failure(CreateTaskErrors.AccessDenied);
        }

        var task = new TaskEntity
        {
            CreatedAt = DateTime.UtcNow,
            Title = createTaskDto.Title,
            Description = createTaskDto.Description,
            Status = Status.ToDo,
            DueDate = createTaskDto.DueDate,
            CreatedByUserId = currentUserId,
            AssigneeUserId = createTaskDto.AssigneeUserId,
            ProjectId = project.Id
        };

        _taskRepository.Create(task);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Successfully created a task for Project: {ProjectId}", createTaskDto.ProjectId);
        return Result<TaskEntity>.Success(task);
    }
}