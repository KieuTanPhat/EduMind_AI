using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;

namespace StudyAI.Application.Features.Admin.Commands;

public sealed record ProcessPlusRequestCommand(Guid AdminUserId, Guid RequestId, bool Approve, string? Note, int? DurationDays) : IRequest;

public sealed class ProcessPlusRequestCommandHandler : IRequestHandler<ProcessPlusRequestCommand>
{
    private readonly IApplicationDbContext _db;
    public ProcessPlusRequestCommandHandler(IApplicationDbContext db) => _db = db;
    public async Task Handle(ProcessPlusRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await _db.PlusRequests.Include(x => x.User).SingleOrDefaultAsync(x => x.Id == command.RequestId, cancellationToken) ?? throw new NotFoundException("Plus request was not found.");
        if (request.Status != "Pending") throw new BadRequestException("This Plus request has already been processed.");
        var now = DateTime.UtcNow;
        if (command.Approve)
        {
            var expires = command.DurationDays is > 0 ? now.AddDays(command.DurationDays.Value) : (DateTime?)null;
            if (string.Equals(request.Plan, "Pro", StringComparison.OrdinalIgnoreCase)) request.User.GrantPro(now, expires);
            else request.User.GrantPlus(now, expires);
            request.Approve(command.AdminUserId, now, command.Note);
        }
        else request.Reject(command.AdminUserId, now, command.Note);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
