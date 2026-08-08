using StudyAI.Domain.Enums;

namespace StudyAI.Application.Abstractions;

public interface IDocumentTextExtractor
{
    bool CanHandle(DocumentFileType fileType);

    Task<string> ExtractAsync(Stream content, CancellationToken cancellationToken);
}
