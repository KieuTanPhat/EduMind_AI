using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Contracts.Support;

namespace StudyAI.Application.Features.Admin.Queries;

public sealed record GetSupportTicketsQuery : IRequest<IReadOnlyCollection<SupportTicketResponse>>;
public sealed class GetSupportTicketsQueryHandler : IRequestHandler<GetSupportTicketsQuery, IReadOnlyCollection<SupportTicketResponse>>
{
    private readonly IApplicationDbContext _db;
    public GetSupportTicketsQueryHandler(IApplicationDbContext db) => _db = db;
    public async Task<IReadOnlyCollection<SupportTicketResponse>> Handle(GetSupportTicketsQuery query, CancellationToken cancellationToken) => await _db.SupportTickets.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc).Take(100).Select(x => new SupportTicketResponse(x.Id, x.Subject, x.Message, x.Status, x.AdminReply, x.CreatedAtUtc, x.ResolvedAtUtc)).ToListAsync(cancellationToken);
}
