using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;

namespace StudyAI.Application.Features.Admin.Commands;

public sealed record DeactivateUserCommand(Guid UserId) : IRequest;

public sealed class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand>
{
    private readonly IApplicationDbContext _db;

    public DeactivateUserCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeactivateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Id == command.UserId, cancellationToken)
            ?? throw new NotFoundException("User was not found.");
        user.Deactivate();
        await _db.SaveChangesAsync(cancellationToken);
    }
}
