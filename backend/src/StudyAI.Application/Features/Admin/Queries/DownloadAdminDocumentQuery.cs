using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Application.Features.Documents.Queries;

namespace StudyAI.Application.Features.Admin.Queries;

public sealed record DownloadAdminDocumentQuery(Guid DocumentId) : IRequest<DocumentDownload>;

public sealed class DownloadAdminDocumentQueryHandler : IRequestHandler<DownloadAdminDocumentQuery, DocumentDownload>
{
    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;

    public DownloadAdminDocumentQueryHandler(IApplicationDbContext db, IFileStorageService fileStorage)
    {
        _db = db;
        _fileStorage = fileStorage;
    }

    public async Task<DocumentDownload> Handle(DownloadAdminDocumentQuery query, CancellationToken cancellationToken)
    {
        var document = await _db.Documents.AsNoTracking().SingleOrDefaultAsync(x => x.Id == query.DocumentId, cancellationToken)
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
