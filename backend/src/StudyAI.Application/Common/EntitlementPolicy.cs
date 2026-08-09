using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;

namespace StudyAI.Application.Common;

public static class EntitlementPolicy
{
    public static async Task EnsurePlusAsync(IApplicationDbContext db, Guid userId, string feature, CancellationToken cancellationToken)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == userId && x.IsActive, cancellationToken)
            ?? throw new NotFoundException("User was not found.");
        if (!user.HasActivePlus(DateTime.UtcNow))
        {
            throw new BadRequestException($"{feature} is available on the Plus plan. Upgrade to continue.");
        }
    }

    public static async Task EnsureDailyAiAllowanceAsync(IApplicationDbContext db, Guid userId, string operation, int freeLimit, CancellationToken cancellationToken)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == userId && x.IsActive, cancellationToken)
            ?? throw new NotFoundException("User was not found.");
        if (user.HasActivePlus(DateTime.UtcNow)) return;

        var start = DateTime.UtcNow.Date;
        var used = await db.AiUsageLogs.CountAsync(x => x.UserId == userId && x.Operation == operation && x.CreatedAtUtc >= start, cancellationToken);
        if (used >= freeLimit)
        {
            throw new BadRequestException($"Free plan allows {freeLimit} {operation} uses per day. Upgrade to Plus for unlimited access.");
        }
    }

    public static async Task EnsureDailyPlanOperationAllowanceAsync(IApplicationDbContext db, Guid userId, string operation, int freeLimit, int plusLimit, CancellationToken cancellationToken)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == userId && x.IsActive, cancellationToken)
            ?? throw new NotFoundException("User was not found.");
        if (user.IsPro) return;

        var limit = user.HasActivePlus(DateTime.UtcNow) ? plusLimit : freeLimit;
        var start = DateTime.UtcNow.Date;
        var used = await db.AiUsageLogs.CountAsync(x => x.UserId == userId && x.Operation == operation && x.CreatedAtUtc >= start, cancellationToken);
        if (used >= limit)
        {
            throw new BadRequestException($"Daily {operation} limit reached ({limit} per day). Upgrade your plan to continue.");
        }
    }

    public static async Task EnsureDailyAiTokenAllowanceAsync(IApplicationDbContext db, Guid userId, CancellationToken cancellationToken)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == userId && x.IsActive, cancellationToken)
            ?? throw new NotFoundException("User was not found.");
        if (user.IsPro)
        {
            return;
        }

        var planPolicy = await db.PlanPolicies.AsNoTracking().SingleOrDefaultAsync(x => x.Plan == user.Plan, cancellationToken);
        var limit = user.AiTokenLimitPerDay ?? planPolicy?.DailyTokenLimit;
        if (!limit.HasValue) return;

        var start = DateTime.UtcNow.Date;
        var used = await db.AiUsageLogs.Where(x => x.UserId == userId && x.CreatedAtUtc >= start)
            .Select(x => (long)x.InputTokens + x.OutputTokens)
            .SumAsync(cancellationToken);
        if (used >= limit.Value)
        {
            throw new BadRequestException("Daily AI token quota has been reached. Upgrade your plan or contact support.");
        }
    }
}
