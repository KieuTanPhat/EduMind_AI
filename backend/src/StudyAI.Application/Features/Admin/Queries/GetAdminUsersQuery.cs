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
            .Select(x => new AdminUserResponse(x.Id, x.Email, x.FirstName, x.LastName, x.IsActive, x.IsPlus, x.Plan, x.PlusExpiresAtUtc, x.AiTokenLimitPerDay, 0, x.UserRoles.Select(role => role.Role.Name).ToArray(), x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
        var userIds = items.Select(x => x.Id).ToArray();
        var startOfDay = DateTime.UtcNow.Date;
        var usageLogs = await _db.AiUsageLogs.AsNoTracking()
            .Where(x => userIds.Contains(x.UserId) && x.CreatedAtUtc >= startOfDay)
            .Select(x => new { x.UserId, x.InputTokens, x.OutputTokens })
            .ToListAsync(cancellationToken);
        var usage = usageLogs.GroupBy(x => x.UserId)
            .ToDictionary(group => group.Key, group => group.Sum(x => (long)x.InputTokens + x.OutputTokens));
        items = items.Select(x => x with { AiTokensUsedToday = usage.GetValueOrDefault(x.Id) }).ToList();
        return new PagedResponse<AdminUserResponse>(items, page, pageSize, total, (int)Math.Ceiling(total / (double)pageSize));
    }
}
