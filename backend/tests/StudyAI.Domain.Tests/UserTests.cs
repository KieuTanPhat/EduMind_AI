using FluentAssertions;
using StudyAI.Domain.Entities;

namespace StudyAI.Domain.Tests;

public sealed class UserTests
{
    [Fact]
    public void UpdateProfile_ShouldChangeNamesAndUpdatedTimestamp()
    {
        var user = new User("user@example.com", "USER@EXAMPLE.COM", "hash", "Old", "Name");

        user.UpdateProfile("New", "Name");

        user.FirstName.Should().Be("New");
        user.LastName.Should().Be("Name");
        user.UpdatedAtUtc.Should().NotBeNull();
        user.CreatedAtUtc.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Deactivate_ShouldPreventActiveAccount()
    {
        var user = new User("user@example.com", "USER@EXAMPLE.COM", "hash", "Test", "User");

        user.Deactivate();

        user.IsActive.Should().BeFalse();
        user.UpdatedAtUtc.Should().NotBeNull();
    }
}
