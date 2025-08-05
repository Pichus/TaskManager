using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Projects.Create;
using TaskManager.Projects.Get;
using TaskManager.UseCases.Projects;
using TaskManager.UseCases.Projects.Create;
using TaskManager.UseCases.Projects.Get;

namespace TaskManager.Projects;

[Authorize]
[Route("api/projects")]
[ApiController]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<IEnumerable<GetProjectResponse>> GetAll()
    {
        var userProjects = await _projectService.GetAllByUserAsync();

        var userProjectsResponse = userProjects.Select(project => new GetProjectResponse
        {
            Title = project.Title,
            LeadUserId = project.LeadUserId
        });
        
        return userProjectsResponse;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetProjectResponse>> Get(int id)
    {
        var getProjectResult = await _projectService.GetByIdAsync(id);

        if (!getProjectResult.Success)
        {
            var errorCode = getProjectResult.Error.Code;
            var errorMessage = getProjectResult.Error.Message;
            
            if (errorCode == GetProjectErrors.NotFound().Code)
            {
                return NotFound(errorMessage);
            }

            if (errorCode == GetProjectErrors.AccessDenied.Code)
            {
                return Forbid(errorMessage);
            }
        }

        var response = new GetProjectResponse
        {
            Title = getProjectResult.Title,
            LeadUserId = getProjectResult.LeadUserId
        };

        return response;
    }

    [HttpPost]
    public async Task<ActionResult<CreateProjectResponse>> Create(CreateProjectRequest request)
    {
        var createProjectResult = await _projectService.CreateAsync(new CreateProjectDto
        {
            Title = request.Title
        });

        return new CreateProjectResponse
        {
            ProjectId = createProjectResult.ProjectId,
            Title = createProjectResult.Title,
            LeadUserId = createProjectResult.LeadUserId
        };
    }

    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}