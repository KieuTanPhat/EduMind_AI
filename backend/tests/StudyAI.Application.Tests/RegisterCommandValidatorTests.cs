using FluentAssertions;
using StudyAI.Application.Features.Auth.Commands;
using StudyAI.Contracts.Auth;

namespace StudyAI.Application.Tests;

public sealed class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldRejectWeakPassword()
    {
        var result = await _validator.ValidateAsync(new RegisterCommand(
            new RegisterRequest("student@example.com", "password", "Student", "User", Guid.NewGuid(), "12")));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "Request.Password");
    }

    [Fact]
    public async Task Validate_ShouldAcceptValidRegistration()
    {
        var result = await _validator.ValidateAsync(new RegisterCommand(
            new RegisterRequest("student@example.com", "StrongPass1", "Student", "User", Guid.NewGuid(), "12")));

        result.IsValid.Should().BeTrue();
    }
}
