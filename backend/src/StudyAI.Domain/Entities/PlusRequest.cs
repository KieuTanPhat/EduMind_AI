using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class PlusRequest : Entity
{
    private PlusRequest() { }

    public PlusRequest(Guid userId, string email, decimal amountVnd, string transferContent, string plan = "Plus", DateTime? expiresAtUtc = null)
    {
        UserId = userId;
        Email = email;
        AmountVnd = amountVnd;
        TransferContent = transferContent;
        Plan = string.Equals(plan, "Pro", StringComparison.OrdinalIgnoreCase) ? "Pro" : "Plus";
        Status = "Pending";
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid UserId { get; private set; }
    public string Email { get; private set; } = null!;
    public decimal AmountVnd { get; private set; }
    public string TransferContent { get; private set; } = null!;
    public string Plan { get; private set; } = "Plus";
    public string Status { get; private set; } = null!;
    public string? Note { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }
    public Guid? ProcessedByUserId { get; private set; }
    public string? SepayTransactionId { get; private set; }
    public DateTime? PaidAtUtc { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public User User { get; private set; } = null!;

    public void Approve(Guid adminId, DateTime utcNow, string? note)
    {
        Status = "Approved";
        Note = note;
        ProcessedAtUtc = utcNow;
        ProcessedByUserId = adminId;
        Touch(utcNow);
    }

    public void Reject(Guid adminId, DateTime utcNow, string? note)
    {
        Status = "Rejected";
        Note = note;
        ProcessedAtUtc = utcNow;
        ProcessedByUserId = adminId;
        Touch(utcNow);
    }

    public void ApproveAutomatically(DateTime utcNow, string sepayTransactionId, string? note)
    {
        Status = "Approved";
        Note = note;
        ProcessedAtUtc = utcNow;
        SepayTransactionId = sepayTransactionId;
        PaidAtUtc = utcNow;
        Touch(utcNow);
    }

    public DateTime EffectiveExpiresAtUtc => ExpiresAtUtc ?? CreatedAtUtc.AddMinutes(10);

    public bool IsExpired(DateTime utcNow) => Status == "Pending" && EffectiveExpiresAtUtc <= utcNow;

    public void Expire(DateTime utcNow)
    {
        if (Status != "Pending") return;
        Status = "Expired";
        Touch(utcNow);
    }

    public void SetNote(string note) => Note = note;
}
