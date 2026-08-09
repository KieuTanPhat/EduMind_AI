using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.Admin;
using StudyAI.Domain.Entities;

namespace StudyAI.Application.Features.Admin.Commands;

public sealed record UpdatePlanPolicyCommand(string Plan, UpdatePlanPolicyRequest Request) : IRequest<PlanPolicyResponse>;

public sealed class UpdatePlanPolicyCommandHandler : IRequestHandler<UpdatePlanPolicyCommand, PlanPolicyResponse>
{
    private readonly IApplicationDbContext _db;
    public UpdatePlanPolicyCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<PlanPolicyResponse> Handle(UpdatePlanPolicyCommand command, CancellationToken cancellationToken)
    {
        var plan = command.Plan.Trim();
        if (plan is not ("Free" or "Plus" or "Pro")) throw new BadRequestException("Plan must be Free, Plus or Pro.");
        var request = command.Request;
        if (request.MaxUploadSizeMb < 1 || request.MaxUploadSizeMb > 200) throw new BadRequestException("Upload size must be between 1 and 200 MB.");
        if (request.DailyDocumentLimit is < 0 || request.DailyTokenLimit is < 0) throw new BadRequestException("Limits cannot be negative.");
        var policy = await _db.PlanPolicies.SingleOrDefaultAsync(x => x.Plan == plan, cancellationToken);
        if (policy is null)
        {
            policy = new PlanPolicy(plan, request.MaxUploadSizeMb, request.DailyDocumentLimit, request.DailyTokenLimit);
            _db.PlanPolicies.Add(policy);
        }
        else policy.Update(request.MaxUploadSizeMb, request.DailyDocumentLimit, request.DailyTokenLimit, DateTime.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
        return new PlanPolicyResponse(policy.Plan, policy.MaxUploadSizeMb, policy.DailyDocumentLimit, policy.DailyTokenLimit);
    }
}
