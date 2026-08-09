namespace StudyAI.Contracts.Billing;

public sealed record PlusRequestRequest(string? Note, string? Plan = "Plus");
public sealed record PlusRequestResponse(Guid Id, decimal AmountVnd, string TransferContent, string Status, DateTime CreatedAtUtc, string Plan = "Plus", DateTime? ExpiresAtUtc = null, DateTime? PaidAtUtc = null);
public sealed record PlusRequestAdminResponse(Guid Id, Guid UserId, string Email, string FullName, decimal AmountVnd, string TransferContent, string Status, string? Note, DateTime CreatedAtUtc, string Plan = "Plus");
public sealed record GrantPlusRequest(bool Approve, string? Note, int? DurationDays);
public sealed record SepayWebhookPayload(long Id, string? Gateway, string? TransactionDate, string? AccountNumber, string? Code, string? Content, string? TransferType, decimal TransferAmount, string? ReferenceCode);
public sealed record SepayWebhookResult(bool Processed, string Message);
