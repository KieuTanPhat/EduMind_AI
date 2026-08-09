namespace StudyAI.Contracts.Support;

public sealed record CreateSupportTicketRequest(string Subject, string Message);
public sealed record SupportTicketResponse(Guid Id, string Subject, string Message, string Status, string? AdminReply, DateTime CreatedAtUtc, DateTime? ResolvedAtUtc, string? UserEmail = null, string? UserFullName = null, Guid? UserId = null);
