using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.Documents;

namespace StudyAI.Application.Features.Documents.Queries;

public sealed record GetDocumentStatusQuery(Guid UserId, Guid DocumentId) : IRequest<DocumentStatusResponse>;

public sealed class GetDocumentStatusQueryHandler : IRequestHandler<GetDocumentStatusQuery, DocumentStatusResponse>
{
    private readonly IApplicationDbContext _db;

    public GetDocumentStatusQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<DocumentStatusResponse> Handle(GetDocumentStatusQuery query, CancellationToken cancellationToken)
    {
        var document = await _db.Documents.AsNoTracking()
            .Where(x => x.Id == query.DocumentId && x.UserId == query.UserId)
            .Select(x => new DocumentStatusResponse(x.Id, x.Status.ToString(), x.ProcessingError, x.ExtractedText != null, x.UpdatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);

        return document ?? throw new NotFoundException("Document was not found.");
    }
}
