using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.Documents;

namespace StudyAI.Application.Features.Documents.Queries;

public sealed record GetDocumentQuery(Guid UserId, Guid DocumentId) : IRequest<DocumentDetailResponse>;

public sealed class GetDocumentQueryHandler : IRequestHandler<GetDocumentQuery, DocumentDetailResponse>
{
    private readonly IApplicationDbContext _db;

    public GetDocumentQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<DocumentDetailResponse> Handle(GetDocumentQuery query, CancellationToken cancellationToken)
    {
        var document = await _db.Documents.AsNoTracking()
            .Where(x => x.Id == query.DocumentId && x.UserId == query.UserId)
            .Select(x => new DocumentDetailResponse(
                x.Id,
                x.OriginalFileName,
                x.FileType.ToString(),
                x.FileSizeBytes,
                x.Status.ToString(),
                x.ProcessingError,
                x.ExtractedText != null,
                x.Summary != null,
                x.MindMap != null,
                x.Flashcards.Count,
                x.Quizzes.Count,
                x.CreatedAtUtc,
                x.UpdatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);

        return document ?? throw new NotFoundException("Document was not found.");
    }
}
