using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Projects.CreateProject;
using TaskManager.UseCases.Projects;

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
    public IEnumerable<string> Get()
    {
        return ["hello", "hiii"];
    }

    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "hello world";
    }

    [HttpPost]
    public async Task<ActionResult<CreateProjectResponse>> Post(CreateProjectRequest request)
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