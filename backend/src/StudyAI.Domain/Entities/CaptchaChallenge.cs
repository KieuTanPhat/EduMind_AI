using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class CaptchaChallenge : Entity
{
    private CaptchaChallenge() { }

    public CaptchaChallenge(string question, string answerHash, DateTime expiresAtUtc)
    {
        Question = question;
        AnswerHash = answerHash;
        ExpiresAtUtc = expiresAtUtc;
    }

    public string Question { get; private set; } = null!;
    public string AnswerHash { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? UsedAtUtc { get; private set; }

    public bool IsUsable(DateTime utcNow) => UsedAtUtc is null && ExpiresAtUtc > utcNow;

    public void MarkUsed(DateTime utcNow) => UsedAtUtc = utcNow;
}
