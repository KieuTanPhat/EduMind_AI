using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Contracts.Support;

namespace StudyAI.Application.Features.Support.Queries;

public sealed record GetMySupportTicketsQuery(Guid UserId) : IRequest<IReadOnlyCollection<SupportTicketResponse>>;

public sealed class GetMySupportTicketsQueryHandler : IRequestHandler<GetMySupportTicketsQuery, IReadOnlyCollection<SupportTicketResponse>>
{
    private readonly IApplicationDbContext _db;

    public GetMySupportTicketsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyCollection<SupportTicketResponse>> Handle(GetMySupportTicketsQuery query, CancellationToken cancellationToken)
        => await _db.SupportTickets.AsNoTracking().Where(x => x.UserId == query.UserId).OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new SupportTicketResponse(x.Id, x.Subject, x.Message, x.Status, x.AdminReply, x.CreatedAtUtc, x.ResolvedAtUtc, null, null, null))
            .ToArrayAsync(cancellationToken);
}
