using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;

namespace StudyAI.Application.Features.Billing.Commands;

public sealed record CancelPlusRequestCommand(Guid UserId, Guid RequestId) : IRequest;

public sealed class CancelPlusRequestCommandHandler : IRequestHandler<CancelPlusRequestCommand>
{
    private readonly IApplicationDbContext _db;

    public CancelPlusRequestCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(CancelPlusRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await _db.PlusRequests.SingleOrDefaultAsync(
            x => x.Id == command.RequestId && x.UserId == command.UserId,
            cancellationToken)
            ?? throw new NotFoundException("Payment order was not found.");

        request.Expire(DateTime.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
