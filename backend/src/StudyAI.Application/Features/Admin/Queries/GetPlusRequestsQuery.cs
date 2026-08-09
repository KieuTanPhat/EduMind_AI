using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Contracts.Billing;

namespace StudyAI.Application.Features.Admin.Queries;

public sealed record GetPlusRequestsQuery : IRequest<IReadOnlyCollection<PlusRequestAdminResponse>>;

public sealed class GetPlusRequestsQueryHandler : IRequestHandler<GetPlusRequestsQuery, IReadOnlyCollection<PlusRequestAdminResponse>>
{
    private readonly IApplicationDbContext _db;
    public GetPlusRequestsQueryHandler(IApplicationDbContext db) => _db = db;
    public async Task<IReadOnlyCollection<PlusRequestAdminResponse>> Handle(GetPlusRequestsQuery query, CancellationToken cancellationToken) => await _db.PlusRequests.AsNoTracking().Include(x => x.User).OrderByDescending(x => x.CreatedAtUtc).Take(100).Select(x => new PlusRequestAdminResponse(x.Id, x.UserId, x.Email, x.User.FirstName + " " + x.User.LastName, x.AmountVnd, x.TransferContent, x.Status, x.Note, x.CreatedAtUtc, x.Plan)).ToListAsync(cancellationToken);
}
