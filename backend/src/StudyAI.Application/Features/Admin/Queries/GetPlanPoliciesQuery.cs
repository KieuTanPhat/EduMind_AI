using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Contracts.Admin;

namespace StudyAI.Application.Features.Admin.Queries;

public sealed record GetPlanPoliciesQuery : IRequest<IReadOnlyCollection<PlanPolicyResponse>>;

public sealed class GetPlanPoliciesQueryHandler : IRequestHandler<GetPlanPoliciesQuery, IReadOnlyCollection<PlanPolicyResponse>>
{
    private readonly IApplicationDbContext _db;
    public GetPlanPoliciesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyCollection<PlanPolicyResponse>> Handle(GetPlanPoliciesQuery query, CancellationToken cancellationToken)
    {
        var existing = await _db.PlanPolicies.AsNoTracking().ToListAsync(cancellationToken);
        return new[] { "Free", "Plus", "Pro" }.Select(plan =>
        {
            var policy = existing.SingleOrDefault(x => x.Plan == plan);
            return policy is null
                ? new PlanPolicyResponse(plan, plan == "Pro" ? 50 : 25, plan == "Free" ? 2 : null, plan == "Free" ? null : null)
                : new PlanPolicyResponse(policy.Plan, policy.MaxUploadSizeMb, policy.DailyDocumentLimit, policy.DailyTokenLimit);
        }).ToArray();
    }
}
