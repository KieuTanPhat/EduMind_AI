namespace StudyAI.Contracts.Auth;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    Guid CaptchaId = default,
    string CaptchaAnswer = "");

public sealed record LoginRequest(string Email, string Password);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record LogoutRequest(string RefreshToken);

public sealed record AuthResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyCollection<string> Roles,
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);

public sealed record RegisterResponse(Guid RegistrationId, string Email, bool RequiresEmailVerification, DateTime VerificationExpiresAtUtc);

public sealed record CaptchaResponse(Guid Id, string ImageDataUrl, DateTime ExpiresAtUtc);

public sealed record ResendEmailVerificationRequest(string Email);

public sealed record CurrentUserResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyCollection<string> Roles,
    bool IsEmailVerified,
    bool IsPlus,
    string Plan,
    DateTime? PlusExpiresAtUtc);
