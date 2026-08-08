using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class PlusRequest : Entity
{
    private PlusRequest() { }

    public PlusRequest(Guid userId, string email, decimal amountVnd, string transferContent)
    {
        UserId = userId;
        Email = email;
        AmountVnd = amountVnd;
        TransferContent = transferContent;
        Status = "Pending";
    }

    public Guid UserId { get; private set; }
    public string Email { get; private set; } = null!;
    public decimal AmountVnd { get; private set; }
    public string TransferContent { get; private set; } = null!;
    public string Status { get; private set; } = null!;
    public string? Note { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }
    public Guid? ProcessedByUserId { get; private set; }
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

    public void SetNote(string note) => Note = note;
}
