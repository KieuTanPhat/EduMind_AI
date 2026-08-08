using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;

namespace StudyAI.Application.Features.Admin.Commands;

public sealed record GrantPlusToUserCommand(Guid UserId, int? DurationDays) : IRequest;

public sealed class GrantPlusToUserCommandHandler : IRequestHandler<GrantPlusToUserCommand>
{
    private readonly IApplicationDbContext _db;

    public GrantPlusToUserCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(GrantPlusToUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Id == command.UserId, cancellationToken)
            ?? throw new NotFoundException("User was not found.");
        var now = DateTime.UtcNow;
        DateTime? expiresAtUtc = command.DurationDays is > 0 ? now.AddDays(command.DurationDays.Value) : null;
        user.GrantPlus(now, expiresAtUtc);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
