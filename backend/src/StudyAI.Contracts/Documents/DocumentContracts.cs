namespace StudyAI.Contracts.Documents;

public sealed record UploadDocumentResponse(
    Guid Id,
    string OriginalFileName,
    string Status,
    DateTime CreatedAtUtc);

public sealed record DocumentListItemResponse(
    Guid Id,
    string OriginalFileName,
    string FileType,
    long FileSizeBytes,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record DocumentDetailResponse(
    Guid Id,
    string OriginalFileName,
    string FileType,
    long FileSizeBytes,
    string Status,
    string? ProcessingError,
    bool HasExtractedText,
    bool HasSummary,
    bool HasMindMap,
    int FlashcardCount,
    int QuizCount,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record DocumentStatusResponse(
    Guid Id,
    string Status,
    string? ProcessingError,
    bool HasExtractedText,
    DateTime? UpdatedAtUtc);

public sealed record PagedResponse<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
