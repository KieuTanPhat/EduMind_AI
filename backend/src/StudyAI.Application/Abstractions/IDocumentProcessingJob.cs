namespace StudyAI.Application.Abstractions;

public interface IDocumentProcessingJob
{
    Task ProcessAsync(Guid documentId);
}
