using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class EmailVerificationOtp : Entity
{
    private EmailVerificationOtp() { }

    public EmailVerificationOtp(Guid userId, string codeHash, DateTime expiresAtUtc)
    {
        UserId = userId;
        CodeHash = codeHash;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid UserId { get; private set; }
    public string CodeHash { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? UsedAtUtc { get; private set; }
    public int FailedAttempts { get; private set; }
    public User User { get; private set; } = null!;

    public bool IsUsable(DateTime utcNow) => UsedAtUtc is null && ExpiresAtUtc > utcNow && FailedAttempts < 5;

    public void MarkUsed(DateTime utcNow) => UsedAtUtc = utcNow;

    public void RecordFailedAttempt() => FailedAttempts++;
}
