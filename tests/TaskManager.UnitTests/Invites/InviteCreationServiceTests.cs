using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.Infrastructure.Identity.CurrentUser;
using TaskManager.UseCases.Invites.Create;

namespace TaskManager.UnitTests.Invites;

public class InviteCreationServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly InviteCreationService _inviteCreationService;

    private readonly Mock<ILogger> _loggerMock;
    // private readonly Mock<AppDbContext>

    public InviteCreationServiceTests()
    {
        _loggerMock = new Mock<ILogger>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        // _inviteCreationService = new InviteCreationService();
    }
}