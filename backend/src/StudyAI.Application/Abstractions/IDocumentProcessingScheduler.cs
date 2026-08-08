namespace StudyAI.Application.Abstractions;

public interface IDocumentProcessingScheduler
{
    Task EnqueueAsync(Guid documentId, CancellationToken cancellationToken);
}
