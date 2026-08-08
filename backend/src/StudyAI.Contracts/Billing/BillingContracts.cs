namespace StudyAI.Contracts.Billing;

public sealed record PlusRequestRequest(string? Note);
public sealed record PlusRequestResponse(Guid Id, decimal AmountVnd, string TransferContent, string Status, DateTime CreatedAtUtc);
public sealed record PlusRequestAdminResponse(Guid Id, Guid UserId, string Email, string FullName, decimal AmountVnd, string TransferContent, string Status, string? Note, DateTime CreatedAtUtc);
public sealed record GrantPlusRequest(bool Approve, string? Note, int? DurationDays);
