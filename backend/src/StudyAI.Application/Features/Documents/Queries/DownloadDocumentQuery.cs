using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;

namespace StudyAI.Application.Features.Documents.Queries;

public sealed record DownloadDocumentQuery(Guid UserId, Guid DocumentId) : IRequest<DocumentDownload>;

public sealed record DocumentDownload(Stream Content, string FileName, string ContentType);

public sealed class DownloadDocumentQueryHandler : IRequestHandler<DownloadDocumentQuery, DocumentDownload>
{
    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;

    public DownloadDocumentQueryHandler(IApplicationDbContext db, IFileStorageService fileStorage)
    {
        _db = db;
        _fileStorage = fileStorage;
    }

    public async Task<DocumentDownload> Handle(DownloadDocumentQuery query, CancellationToken cancellationToken)
    {
        var document = await _db.Documents.AsNoTracking().SingleOrDefaultAsync(x => x.Id == query.DocumentId && x.UserId == query.UserId, cancellationToken)
            ?? throw new NotFoundException("Document was not found.");

        var content = await _fileStorage.OpenReadAsync(document.StoragePath, cancellationToken);
        var contentType = document.FileType switch
        {
            Domain.Enums.DocumentFileType.Pdf => "application/pdf",
            Domain.Enums.DocumentFileType.Docx => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "text/plain"
        };

        return new DocumentDownload(content, document.OriginalFileName, contentType);
    }
}
