using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;

namespace StudyAI.Application.Features.Admin.Commands;

public sealed record ActivateUserCommand(Guid UserId) : IRequest;

public sealed class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand>
{
    private readonly IApplicationDbContext _db;

    public ActivateUserCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(ActivateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Id == command.UserId, cancellationToken)
            ?? throw new NotFoundException("User was not found.");
        user.Activate();
        await _db.SaveChangesAsync(cancellationToken);
    }
}
