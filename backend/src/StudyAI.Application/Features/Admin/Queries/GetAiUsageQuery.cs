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
        => await _db.AiUsageLogs.GroupBy(x => x.Operation).Select(x => new AiUsageSummaryResponse(x.Key, x.Count(), x.Sum(item => item.InputTokens), x.Sum(item => item.OutputTokens))).OrderByDescending(x => x.RequestCount).ToArrayAsync(cancellationToken);
}
