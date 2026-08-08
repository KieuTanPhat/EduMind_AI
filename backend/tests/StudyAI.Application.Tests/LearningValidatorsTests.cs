using FluentAssertions;
using StudyAI.Application.Features.Learning.Commands;
using StudyAI.Contracts.Learning;

namespace StudyAI.Application.Tests;

public sealed class LearningValidatorsTests
{
    [Fact]
    public async Task UpdateProgress_ShouldRejectValuesOutsideRange()
    {
        var validator = new UpdateProgressCommandValidator();

        var result = await validator.ValidateAsync(new UpdateProgressCommand(Guid.NewGuid(), Guid.NewGuid(), new UpdateProgressRequest(101, -1)));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName.Contains("CompletionPercentage"));
        result.Errors.Should().Contain(error => error.PropertyName.Contains("StudyMinutes"));
    }

    [Theory]
    [InlineData("known")]
    [InlineData("review")]
    [InlineData("unknown")]
    public async Task ReviewFlashcard_ShouldAcceptSupportedStatuses(string status)
    {
        var validator = new ReviewFlashcardCommandValidator();

        var result = await validator.ValidateAsync(new ReviewFlashcardCommand(Guid.NewGuid(), Guid.NewGuid(), new ReviewFlashcardRequest(status)));

        result.IsValid.Should().BeTrue();
    }
}
