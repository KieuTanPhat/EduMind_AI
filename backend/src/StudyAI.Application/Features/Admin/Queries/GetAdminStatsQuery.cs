using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Contracts.Admin;
using StudyAI.Domain.Enums;

namespace StudyAI.Application.Features.Admin.Queries;

public sealed record GetAdminStatsQuery() : IRequest<AdminStatsResponse>;

public sealed class GetAdminStatsQueryHandler : IRequestHandler<GetAdminStatsQuery, AdminStatsResponse>
{
    private readonly IApplicationDbContext _db;

    public GetAdminStatsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<AdminStatsResponse> Handle(GetAdminStatsQuery query, CancellationToken cancellationToken)
    {
        var users = await _db.Users.Select(x => x.IsActive).ToListAsync(cancellationToken);
        var documents = await _db.Documents.Select(x => new { x.FileSizeBytes, x.Status }).ToListAsync(cancellationToken);
        var usage = await _db.AiUsageLogs.Select(x => new { x.InputTokens, x.OutputTokens }).ToListAsync(cancellationToken);
        return new AdminStatsResponse(
            users.Count,
            users.Count(x => x),
            documents.Count,
            documents.Count(x => x.Status == DocumentStatus.Processed),
            documents.Sum(x => x.FileSizeBytes),
            usage.Count,
            usage.Sum(x => x.InputTokens),
            usage.Sum(x => x.OutputTokens));
    }
}
