using System.Text.Json.Serialization;

namespace StudyAI.Contracts.Admin;

public sealed record AdminUserResponse(Guid Id, string Email, string FirstName, string LastName, bool IsActive, bool IsPlus, string Plan, DateTime? PlusExpiresAtUtc, [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] long? AiTokenLimitPerDay, long AiTokensUsedToday, IReadOnlyCollection<string> Roles, DateTime CreatedAtUtc);

public sealed record GrantPlusUserRequest(int? DurationDays);

public sealed record SetAiQuotaRequest(long? TokenLimitPerDay);
public sealed record SetUserPlanRequest(string Plan, int? DurationDays);

public sealed record PlanPolicyResponse(string Plan, int MaxUploadSizeMb, int? DailyDocumentLimit, long? DailyTokenLimit);
public sealed record UpdatePlanPolicyRequest(int MaxUploadSizeMb, int? DailyDocumentLimit, long? DailyTokenLimit);

public sealed record AdminDocumentResponse(Guid Id, string OriginalFileName, string OwnerEmail, string FileType, string Status, long FileSizeBytes, DateTime CreatedAtUtc);

public sealed record AiUsageSummaryResponse(string Operation, int RequestCount, long InputTokens, long OutputTokens);

public sealed record AdminStatsResponse(
    int TotalUsers,
    int ActiveUsers,
    int TotalDocuments,
    int ProcessedDocuments,
    long StorageBytes,
    int AiRequestCount,
    long AiInputTokens,
    long AiOutputTokens);
