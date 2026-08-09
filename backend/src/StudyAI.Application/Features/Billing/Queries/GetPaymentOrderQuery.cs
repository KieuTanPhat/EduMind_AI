using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.Billing;

namespace StudyAI.Application.Features.Billing.Queries;

public sealed record GetPaymentOrderQuery(Guid UserId, Guid RequestId) : IRequest<PlusRequestResponse>;

public sealed class GetPaymentOrderQueryHandler : IRequestHandler<GetPaymentOrderQuery, PlusRequestResponse>
{
    private readonly IApplicationDbContext _db;

    public GetPaymentOrderQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PlusRequestResponse> Handle(GetPaymentOrderQuery query, CancellationToken cancellationToken)
    {
        var request = await _db.PlusRequests.AsTracking().SingleOrDefaultAsync(x => x.Id == query.RequestId && x.UserId == query.UserId, cancellationToken)
            ?? throw new NotFoundException("Payment order was not found.");

        if (request.IsExpired(DateTime.UtcNow))
        {
            request.Expire(DateTime.UtcNow);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return new PlusRequestResponse(request.Id, request.AmountVnd, request.TransferContent, request.Status, request.CreatedAtUtc, request.Plan, request.EffectiveExpiresAtUtc, request.PaidAtUtc);
    }
}
