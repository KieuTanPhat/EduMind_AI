using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Contracts.Admin;
using StudyAI.Contracts.Documents;

namespace StudyAI.Application.Features.Admin.Queries;

public sealed record GetAdminUsersQuery(string? Search, int Page, int PageSize) : IRequest<PagedResponse<AdminUserResponse>>;

public sealed class GetAdminUsersQueryHandler : IRequestHandler<GetAdminUsersQuery, PagedResponse<AdminUserResponse>>
{
    private readonly IApplicationDbContext _db;

    public GetAdminUsersQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResponse<AdminUserResponse>> Handle(GetAdminUsersQuery query, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var users = _db.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            users = users.Where(x => x.Email.Contains(search) || x.FirstName.Contains(search) || x.LastName.Contains(search));
        }

        var total = await users.CountAsync(cancellationToken);
        var items = await users.OrderByDescending(x => x.CreatedAtUtc).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new AdminUserResponse(x.Id, x.Email, x.FirstName, x.LastName, x.IsActive, x.IsPlus, x.PlusExpiresAtUtc, x.UserRoles.Select(role => role.Role.Name).ToArray(), x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
        return new PagedResponse<AdminUserResponse>(items, page, pageSize, total, (int)Math.Ceiling(total / (double)pageSize));
    }
}
