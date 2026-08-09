using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class SupportTicket : Entity
{
    private SupportTicket() { }

    public SupportTicket(Guid userId, string subject, string message)
    {
        UserId = userId;
        Subject = subject;
        Message = message;
        Status = "Open";
    }

    public Guid UserId { get; private set; }
    public string Subject { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public string Status { get; private set; } = null!;
    public string? AdminReply { get; private set; }
    public DateTime? ResolvedAtUtc { get; private set; }
    public User User { get; private set; } = null!;

    public void Resolve(DateTime utcNow, string reply)
    {
        Status = "Resolved";
        AdminReply = reply;
        ResolvedAtUtc = utcNow;
        Touch(utcNow);
    }

    public void Reply(DateTime utcNow, string reply)
    {
        AdminReply = string.IsNullOrWhiteSpace(AdminReply) ? reply : $"{AdminReply}\n{reply}";
        Status = "Answered";
        ResolvedAtUtc = null;
        Touch(utcNow);
    }

    public void MarkRead(DateTime utcNow)
    {
        if (Status == "Open")
        {
            Status = "Read";
            Touch(utcNow);
        }
    }
}
