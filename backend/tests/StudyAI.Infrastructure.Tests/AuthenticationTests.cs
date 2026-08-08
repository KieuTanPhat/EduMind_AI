using FluentAssertions;
using Microsoft.Extensions.Options;
using StudyAI.Domain.Entities;
using StudyAI.Infrastructure.Authentication;

namespace StudyAI.Infrastructure.Tests;

public sealed class AuthenticationTests
{
    private static JwtTokenService CreateTokenService() => new(Options.Create(new JwtOptions
    {
        Issuer = "EduMind.Tests",
        Audience = "EduMind.Tests.Client",
        Secret = "a-development-only-secret-with-at-least-32-bytes",
        AccessTokenMinutes = 15,
        RefreshTokenDays = 7
    }));

    [Fact]
    public void BCryptPasswordHasher_ShouldHashAndVerifyPassword()
    {
        var hasher = new BCryptPasswordHasher();
        var hash = hasher.Hash("StrongPass1");

        hash.Should().NotBe("StrongPass1");
        hasher.Verify("StrongPass1", hash).Should().BeTrue();
        hasher.Verify("WrongPass1", hash).Should().BeFalse();
    }

    [Fact]
    public void JwtTokenService_ShouldRoundTripUserClaims()
    {
        var service = CreateTokenService();
        var user = new User("student@example.com", "STUDENT@EXAMPLE.COM", "hash", "Student", "User");
        var token = service.GenerateAccessToken(user, new[] { "User" });

        var principal = service.GetPrincipalFromExpiredAccessToken(token);

        principal.Identity!.IsAuthenticated.Should().BeTrue();
        principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value.Should().Be(user.Id.ToString());
        principal.IsInRole("User").Should().BeTrue();
    }

    [Fact]
    public void RefreshTokenHash_ShouldBeDeterministicAndOpaque()
    {
        var service = CreateTokenService();
        var refreshToken = service.GenerateRefreshToken();

        var firstHash = service.HashRefreshToken(refreshToken.Token);
        var secondHash = service.HashRefreshToken(refreshToken.Token);

        firstHash.Should().Be(secondHash);
        firstHash.Should().NotBe(refreshToken.Token);
    }
}
