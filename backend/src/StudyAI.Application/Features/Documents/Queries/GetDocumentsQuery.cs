using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Contracts.Documents;

namespace StudyAI.Application.Features.Documents.Queries;

public sealed record GetDocumentsQuery(Guid UserId, string? Search, int Page, int PageSize) : IRequest<PagedResponse<DocumentListItemResponse>>;

public sealed class GetDocumentsQueryHandler : IRequestHandler<GetDocumentsQuery, PagedResponse<DocumentListItemResponse>>
{
    private readonly IApplicationDbContext _db;

    public GetDocumentsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResponse<DocumentListItemResponse>> Handle(GetDocumentsQuery query, CancellationToken cancellationToken)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var documents = _db.Documents.AsNoTracking().Where(x => x.UserId == query.UserId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            documents = documents.Where(x => x.OriginalFileName.Contains(search));
        }

        var totalCount = await documents.CountAsync(cancellationToken);
        var items = await documents
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DocumentListItemResponse(
                x.Id,
                x.OriginalFileName,
                x.FileType.ToString(),
                x.FileSizeBytes,
                x.Status.ToString(),
                x.CreatedAtUtc,
                x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResponse<DocumentListItemResponse>(items, page, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize));
    }
}
