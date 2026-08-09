using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class PendingRegistration : Entity
{
    private PendingRegistration() { }

    public PendingRegistration(string email, string normalizedEmail, string passwordHash, string firstName, string lastName, string tokenHash, DateTime expiresAtUtc)
    {
        Email = email;
        NormalizedEmail = normalizedEmail;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
    }

    public string Email { get; private set; } = null!;
    public string NormalizedEmail { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }

    public bool IsUsable(DateTime utcNow) => ExpiresAtUtc > utcNow;
}
