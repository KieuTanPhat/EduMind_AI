using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Contracts.Learning;

namespace StudyAI.Application.Features.Learning.Queries;

public sealed record GetProgressQuery(Guid UserId) : IRequest<ProgressResponse>;

public sealed class GetProgressQueryHandler : IRequestHandler<GetProgressQuery, ProgressResponse>
{
    private readonly IApplicationDbContext _db;

    public GetProgressQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ProgressResponse> Handle(GetProgressQuery query, CancellationToken cancellationToken)
    {
        var documents = await _db.Documents.Where(x => x.UserId == query.UserId).OrderByDescending(x => x.CreatedAtUtc).ToListAsync(cancellationToken);
        var progress = await _db.LearningProgress.Where(x => x.UserId == query.UserId).ToListAsync(cancellationToken);
        var attempts = await _db.QuizAttempts.Where(x => x.UserId == query.UserId && x.CompletedAtUtc != null).ToListAsync(cancellationToken);
        var flashcardsReviewed = await _db.FlashcardReviews.CountAsync(x => x.UserId == query.UserId, cancellationToken);
        var byDocument = progress.ToDictionary(x => x.DocumentId);
        var rows = documents.Select(document =>
        {
            byDocument.TryGetValue(document.Id, out var item);
            return new ProgressDocumentResponse(document.Id, document.OriginalFileName, item?.CompletionPercentage ?? 0, item?.StudyMinutes ?? 0, item?.UpdatedAtUtc ?? document.CreatedAtUtc);
        }).ToArray();
        var averageQuiz = attempts.Count == 0 ? 0 : Math.Round(attempts.Average(x => x.TotalQuestions == 0 ? 0 : x.Score * 100m / x.TotalQuestions), 2);

        return new ProgressResponse(
            documents.Count,
            rows.Count(x => x.CompletionPercentage >= 100),
            rows.Sum(x => x.StudyMinutes),
            rows.Length == 0 ? 0 : (int)Math.Round(rows.Average(x => x.CompletionPercentage)),
            attempts.Count,
            averageQuiz,
            flashcardsReviewed,
            rows);
    }
}
