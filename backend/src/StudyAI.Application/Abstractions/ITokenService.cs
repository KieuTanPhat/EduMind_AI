using System.Security.Claims;
using StudyAI.Domain.Entities;

namespace StudyAI.Application.Abstractions;

public sealed record RefreshTokenValue(string Token, DateTime ExpiresAtUtc);

public interface ITokenService
{
    string GenerateAccessToken(User user, IReadOnlyCollection<string> roles);

    DateTime GetAccessTokenExpiresAtUtc();

    RefreshTokenValue GenerateRefreshToken();

    string HashRefreshToken(string token);

    ClaimsPrincipal GetPrincipalFromExpiredAccessToken(string accessToken);
}
