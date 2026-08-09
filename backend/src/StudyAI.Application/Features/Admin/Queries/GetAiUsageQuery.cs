using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Contracts.Admin;

namespace StudyAI.Application.Features.Admin.Queries;

public sealed record GetAiUsageQuery() : IRequest<IReadOnlyCollection<AiUsageSummaryResponse>>;

public sealed class GetAiUsageQueryHandler : IRequestHandler<GetAiUsageQuery, IReadOnlyCollection<AiUsageSummaryResponse>>
{
    private readonly IApplicationDbContext _db;

    public GetAiUsageQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyCollection<AiUsageSummaryResponse>> Handle(GetAiUsageQuery query, CancellationToken cancellationToken)
    {
        var logs = await _db.AiUsageLogs.AsNoTracking().ToListAsync(cancellationToken);
        return logs.GroupBy(x => x.Operation)
            .Select(group => new AiUsageSummaryResponse(group.Key, group.Count(), group.Sum(x => x.InputTokens), group.Sum(x => x.OutputTokens)))
            .OrderByDescending(x => x.RequestCount)
            .ToArray();
    }
}
