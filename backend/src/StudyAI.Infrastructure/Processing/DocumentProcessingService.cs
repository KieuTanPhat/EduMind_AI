using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Domain.Entities;

namespace StudyAI.Infrastructure.Processing;

public sealed class DocumentProcessingService : IDocumentProcessingService
{
    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;
    private readonly IEnumerable<IDocumentTextExtractor> _extractors;
    private readonly ITextProcessingService _textProcessing;

    public DocumentProcessingService(
        IApplicationDbContext db,
        IFileStorageService fileStorage,
        IEnumerable<IDocumentTextExtractor> extractors,
        ITextProcessingService textProcessing)
    {
        _db = db;
        _fileStorage = fileStorage;
        _extractors = extractors;
        _textProcessing = textProcessing;
    }

    public async Task ProcessAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var document = await _db.Documents.SingleOrDefaultAsync(x => x.Id == documentId, cancellationToken)
            ?? throw new NotFoundException("Document was not found.");

        try
        {
            document.MarkProcessing();
            await _db.SaveChangesAsync(cancellationToken);

            var extractor = _extractors.SingleOrDefault(x => x.CanHandle(document.FileType))
                ?? throw new InvalidOperationException($"No extractor is registered for {document.FileType}.");
            await using var content = await _fileStorage.OpenReadAsync(document.StoragePath, cancellationToken);
            var rawText = await extractor.ExtractAsync(content, cancellationToken);
            var cleanedText = _textProcessing.Clean(rawText);
            if (string.IsNullOrWhiteSpace(cleanedText))
            {
                throw new InvalidOperationException("The document did not contain extractable text.");
            }

            document.MarkProcessed(cleanedText);
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            document.MarkFailed(exception.Message);
            await _db.SaveChangesAsync(CancellationToken.None);
            throw;
        }
    }
}
