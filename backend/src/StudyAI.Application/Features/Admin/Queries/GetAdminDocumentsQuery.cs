using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Contracts.Admin;
using StudyAI.Contracts.Documents;

namespace StudyAI.Application.Features.Admin.Queries;

public sealed record GetAdminDocumentsQuery(string? Search, int Page, int PageSize) : IRequest<PagedResponse<AdminDocumentResponse>>;

public sealed class GetAdminDocumentsQueryHandler : IRequestHandler<GetAdminDocumentsQuery, PagedResponse<AdminDocumentResponse>>
{
    private readonly IApplicationDbContext _db;

    public GetAdminDocumentsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResponse<AdminDocumentResponse>> Handle(GetAdminDocumentsQuery query, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var documents = _db.Documents.Include(x => x.User).AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            documents = documents.Where(x => x.OriginalFileName.Contains(search) || x.User.Email.Contains(search));
        }

        var total = await documents.CountAsync(cancellationToken);
        var items = await documents.OrderByDescending(x => x.CreatedAtUtc).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new AdminDocumentResponse(x.Id, x.OriginalFileName, x.User.Email, x.FileType.ToString(), x.Status.ToString(), x.FileSizeBytes, x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
        return new PagedResponse<AdminDocumentResponse>(items, page, pageSize, total, (int)Math.Ceiling(total / (double)pageSize));
    }
}
