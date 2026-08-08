using StudyAI.Domain.Common;
using StudyAI.Domain.Enums;

namespace StudyAI.Domain.Entities;

public sealed class Document : Entity
{
    private Document() { }

    public Document(Guid userId, string originalFileName, string storedFileName, string storagePath, DocumentFileType fileType, long fileSizeBytes)
    {
        UserId = userId;
        OriginalFileName = originalFileName;
        StoredFileName = storedFileName;
        StoragePath = storagePath;
        FileType = fileType;
        FileSizeBytes = fileSizeBytes;
    }

    public Guid UserId { get; private set; }

    public Guid? CategoryId { get; private set; }

    public string OriginalFileName { get; private set; } = null!;

    public string StoredFileName { get; private set; } = null!;

    public string StoragePath { get; private set; } = null!;

    public DocumentFileType FileType { get; private set; }

    public long FileSizeBytes { get; private set; }

    public DocumentStatus Status { get; private set; } = DocumentStatus.Uploaded;

    public string? ExtractedText { get; private set; }

    public string? ProcessingError { get; private set; }

    public User User { get; private set; } = null!;

    public DocumentCategory? Category { get; private set; }

    public Summary? Summary { get; private set; }

    public MindMap? MindMap { get; private set; }

    public ICollection<Flashcard> Flashcards { get; private set; } = new List<Flashcard>();

    public ICollection<Quiz> Quizzes { get; private set; } = new List<Quiz>();

    public void SetSummary(Summary summary) => Summary = summary;

    public void SetMindMap(MindMap mindMap) => MindMap = mindMap;

    public void MarkProcessing()
    {
        Status = DocumentStatus.Processing;
        ProcessingError = null;
        Touch(DateTime.UtcNow);
    }

    public void MarkProcessed(string extractedText)
    {
        Status = DocumentStatus.Processed;
        ExtractedText = extractedText;
        ProcessingError = null;
        Touch(DateTime.UtcNow);
    }

    public void MarkFailed(string error)
    {
        Status = DocumentStatus.Failed;
        ProcessingError = error;
        Touch(DateTime.UtcNow);
    }
}
