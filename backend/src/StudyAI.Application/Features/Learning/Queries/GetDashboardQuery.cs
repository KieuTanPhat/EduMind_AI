using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Contracts.Learning;
using StudyAI.Domain.Enums;

namespace StudyAI.Application.Features.Learning.Queries;

public sealed record GetDashboardQuery(Guid UserId) : IRequest<DashboardResponse>;

public sealed class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IPersonalizationService _personalization;

    public GetDashboardQueryHandler(IApplicationDbContext db, IPersonalizationService personalization)
    {
        _db = db;
        _personalization = personalization;
    }

    public async Task<DashboardResponse> Handle(GetDashboardQuery query, CancellationToken cancellationToken)
    {
        var documents = await _db.Documents
            .Where(x => x.UserId == query.UserId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
        var attempts = await _db.QuizAttempts
            .Where(x => x.UserId == query.UserId && x.CompletedAtUtc != null)
            .ToListAsync(cancellationToken);
        var flashcardsReviewed = await _db.FlashcardReviews.CountAsync(x => x.UserId == query.UserId, cancellationToken);
        var recommendations = await _personalization.GetRecommendationsAsync(query.UserId, cancellationToken);

        var average = attempts.Count == 0
            ? 0
            : Math.Round(attempts.Average(x => x.TotalQuestions == 0 ? 0 : x.Score * 100m / x.TotalQuestions), 2);

        return new DashboardResponse(
            documents.Count,
            documents.Count(x => x.Status == DocumentStatus.Processed),
            documents.Count(x => x.Status == DocumentStatus.Processing || x.Status == DocumentStatus.Uploaded),
            attempts.Count,
            average,
            flashcardsReviewed,
            documents.Take(5).Select(x => new DashboardDocumentResponse(x.Id, x.OriginalFileName, x.FileType.ToString(), x.Status.ToString(), x.FileSizeBytes, x.CreatedAtUtc)).ToArray(),
            recommendations.Take(5).ToArray());
    }
}
