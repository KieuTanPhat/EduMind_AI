namespace StudyAI.Contracts.Learning;

public sealed record DashboardResponse(
    int TotalDocuments,
    int ProcessedDocuments,
    int InProgressDocuments,
    int CompletedQuizzes,
    decimal AverageQuizPercentage,
    int FlashcardsReviewed,
    IReadOnlyCollection<DashboardDocumentResponse> RecentDocuments,
    IReadOnlyCollection<RecommendationResponse> Recommendations);

public sealed record DashboardDocumentResponse(
    Guid Id,
    string OriginalFileName,
    string FileType,
    string Status,
    long FileSizeBytes,
    DateTime CreatedAtUtc);

public sealed record ProgressResponse(
    int TotalDocuments,
    int CompletedDocuments,
    int TotalStudyMinutes,
    int AverageCompletionPercentage,
    int QuizAttempts,
    decimal QuizAveragePercentage,
    int FlashcardsReviewed,
    IReadOnlyCollection<ProgressDocumentResponse> Documents);

public sealed record ProgressDocumentResponse(
    Guid DocumentId,
    string OriginalFileName,
    int CompletionPercentage,
    int StudyMinutes,
    DateTime UpdatedAtUtc);

public sealed record RecommendationResponse(
    Guid Id,
    string Title,
    string Description,
    bool IsCompleted,
    DateTime? ExpiresAtUtc,
    DateTime CreatedAtUtc);

public sealed record UpdateProgressRequest(int CompletionPercentage, int StudyMinutes);

public sealed record ReviewFlashcardRequest(string Status);

public sealed record UserPreferenceResponse(string LearningLevel, string LearningGoal, string PreferredLanguage);

public sealed record UpdateUserPreferenceRequest(string LearningLevel, string LearningGoal, string PreferredLanguage);
