using FluentAssertions;
using StudyAI.Api.Controllers;

namespace StudyAI.Api.Tests;

public sealed class HealthControllerTests
{
    [Fact]
    public void HealthEndpoint_ShouldReturnOkPayload()
    {
        var result = new HealthController().Get();

        result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }
}
