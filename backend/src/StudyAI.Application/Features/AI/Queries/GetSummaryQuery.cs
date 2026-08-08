using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.AI;

namespace StudyAI.Application.Features.AI.Queries;

public sealed record GetSummaryQuery(Guid UserId, Guid DocumentId) : IRequest<SummaryResponse>;

public sealed class GetSummaryQueryHandler : IRequestHandler<GetSummaryQuery, SummaryResponse>
{
    private readonly IApplicationDbContext _db;

    public GetSummaryQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<SummaryResponse> Handle(GetSummaryQuery query, CancellationToken cancellationToken)
    {
        var summary = await _db.Summaries.AsNoTracking()
            .Where(x => x.DocumentId == query.DocumentId && x.Document.UserId == query.UserId)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Summary was not found.");
        return new SummaryResponse(summary.Id, summary.DocumentId, summary.Content, summary.Model, summary.CreatedAtUtc, summary.UpdatedAtUtc);
    }
}
