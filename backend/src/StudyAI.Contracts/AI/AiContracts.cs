namespace StudyAI.Contracts.AI;

public sealed record GenerateAiRequest(bool ForceRegenerate = false);

public sealed record SummaryResponse(
    Guid Id,
    Guid DocumentId,
    string Content,
    string Model,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record MindMapNodeResponse(
    Guid Id,
    Guid? ParentNodeId,
    string Label,
    string? Description,
    int Depth,
    double PositionX,
    double PositionY);

public sealed record MindMapResponse(
    Guid Id,
    Guid DocumentId,
    string Title,
    string Model,
    IReadOnlyCollection<MindMapNodeResponse> Nodes,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record FlashcardResponse(
    Guid Id,
    Guid DocumentId,
    string Question,
    string Answer,
    string? Explanation,
    DateTime CreatedAtUtc);

public sealed record FlashcardsResponse(
    Guid DocumentId,
    IReadOnlyCollection<FlashcardResponse> Items);

public sealed record QuizOptionResponse(Guid Id, string Text, bool IsCorrect);

public sealed record QuizQuestionResponse(
    Guid Id,
    string Content,
    string Explanation,
    IReadOnlyCollection<QuizOptionResponse> Options);

public sealed record QuizResponse(
    Guid Id,
    Guid DocumentId,
    string Title,
    IReadOnlyCollection<QuizQuestionResponse> Questions,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record QuizAnswerRequest(Guid QuestionId, Guid? SelectedOptionId);

public sealed record SubmitQuizRequest(IReadOnlyCollection<QuizAnswerRequest> Answers);

public sealed record QuizResultResponse(
    Guid AttemptId,
    Guid QuizId,
    int Score,
    int TotalQuestions,
    decimal Percentage,
    DateTime CompletedAtUtc);

public sealed record CreateChatSessionRequest(string? Title);

public sealed record ChatSessionResponse(Guid Id, Guid DocumentId, string Title, DateTime CreatedAtUtc);

public sealed record SendChatMessageRequest(string Content);

public sealed record ChatMessageResponse(Guid Id, Guid SessionId, string Role, string Content, DateTime CreatedAtUtc);
