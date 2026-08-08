using StudyAI.Application.Abstractions;

namespace StudyAI.Infrastructure.Processing;

public sealed class DocumentProcessingJob : IDocumentProcessingJob
{
    private readonly IDocumentProcessingService _processingService;

    public DocumentProcessingJob(IDocumentProcessingService processingService)
    {
        _processingService = processingService;
    }

    public Task ProcessAsync(Guid documentId) => _processingService.ProcessAsync(documentId, CancellationToken.None);
}
