using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class EmailVerificationToken : Entity
{
    private EmailVerificationToken() { }

    public EmailVerificationToken(Guid userId, string tokenHash, DateTime expiresAtUtc)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? UsedAtUtc { get; private set; }
    public User User { get; private set; } = null!;

    public bool IsUsable(DateTime utcNow) => UsedAtUtc is null && ExpiresAtUtc > utcNow;
    public void MarkUsed(DateTime utcNow) => UsedAtUtc = utcNow;
}
