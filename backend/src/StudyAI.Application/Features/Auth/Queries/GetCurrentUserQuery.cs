using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.Auth;

namespace StudyAI.Application.Features.Auth.Queries;

public sealed record GetCurrentUserQuery(Guid UserId) : IRequest<CurrentUserResponse>;

public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    private readonly IApplicationDbContext _db;

    public GetCurrentUserQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<CurrentUserResponse> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(x => x.Id == query.UserId && x.IsActive, cancellationToken)
            ?? throw new NotFoundException("User was not found.");

        var hasActivePlus = user.HasActivePlus(DateTime.UtcNow);
        var currentPlan = hasActivePlus ? user.Plan : "Free";
        return new CurrentUserResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.UserRoles.Select(x => x.Role.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            user.IsEmailVerified,
            hasActivePlus,
            currentPlan,
            user.PlusExpiresAtUtc);
    }
}
