using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.Admin;

namespace StudyAI.Application.Features.Admin.Commands;

public sealed record SetUserPlanCommand(Guid UserId, SetUserPlanRequest Request) : IRequest;

public sealed class SetUserPlanCommandHandler : IRequestHandler<SetUserPlanCommand>
{
    private readonly IApplicationDbContext _db;
    public SetUserPlanCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(SetUserPlanCommand command, CancellationToken cancellationToken)
    {
        var plan = command.Request.Plan.Trim();
        if (plan is not ("Free" or "Plus" or "Pro")) throw new BadRequestException("Plan must be Free, Plus or Pro.");
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Id == command.UserId, cancellationToken) ?? throw new NotFoundException("User was not found.");
        var now = DateTime.UtcNow;
        DateTime? expires = plan == "Free" ? null : command.Request.DurationDays is > 0 ? now.AddDays(command.Request.DurationDays.Value) : null;
        user.SetPlan(plan, now, expires);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
