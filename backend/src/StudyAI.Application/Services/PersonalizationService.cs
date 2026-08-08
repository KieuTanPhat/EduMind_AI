using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Contracts.Learning;
using StudyAI.Domain.Entities;
using StudyAI.Domain.Enums;

namespace StudyAI.Application.Services;

public sealed class PersonalizationService : IPersonalizationService
{
    private readonly IApplicationDbContext _db;

    public PersonalizationService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyCollection<RecommendationResponse>> GetRecommendationsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var recommendations = await _db.Recommendations
            .Where(x => x.UserId == userId && !x.IsCompleted && (x.ExpiresAtUtc == null || x.ExpiresAtUtc > now))
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
        var titles = recommendations.Select(x => x.Title).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var newRecommendations = new List<Recommendation>();

        var documentCount = await _db.Documents.CountAsync(x => x.UserId == userId, cancellationToken);
        if (documentCount == 0)
        {
            AddIfMissing("Bắt đầu thư viện học tập", "Tải tài liệu đầu tiên để EduMind AI xây dựng lộ trình học cho bạn.");
        }

        var unreviewedDocuments = await _db.Documents
            .CountAsync(x => x.UserId == userId && x.Status == DocumentStatus.Processed && x.Summary == null, cancellationToken);
        if (unreviewedDocuments > 0)
        {
            AddIfMissing("Tóm tắt tài liệu mới", "Bạn có tài liệu đã xử lý nhưng chưa tạo tóm tắt. Hãy bắt đầu từ các ý chính.");
        }

        var attempts = await _db.QuizAttempts
            .Where(x => x.UserId == userId && x.CompletedAtUtc != null)
            .ToListAsync(cancellationToken);
        if (attempts.Count > 0 && attempts.Average(x => x.TotalQuestions == 0 ? 0 : x.Score * 100m / x.TotalQuestions) < 50)
        {
            AddIfMissing("Ôn lại chủ đề còn yếu", "Điểm quiz trung bình đang dưới 50%. Hãy xem lại mind map và luyện thêm flashcards trước khi làm quiz mới.");
        }

        var reviewedCards = await _db.FlashcardReviews.CountAsync(x => x.UserId == userId, cancellationToken);
        var generatedCards = await _db.Flashcards.CountAsync(x => x.Document.UserId == userId, cancellationToken);
        if (generatedCards > 0 && reviewedCards < generatedCards)
        {
            AddIfMissing("Duy trì nhịp flashcard", "Bạn còn flashcard chưa ôn. Một phiên ôn ngắn mỗi ngày sẽ giúp ghi nhớ lâu hơn.");
        }

        if (newRecommendations.Count > 0)
        {
            _db.Recommendations.AddRange(newRecommendations);
            await _db.SaveChangesAsync(cancellationToken);
            recommendations.AddRange(newRecommendations);
        }

        return recommendations
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(Map)
            .ToArray();

        void AddIfMissing(string title, string description)
        {
            if (!titles.Contains(title))
            {
                var recommendation = new Recommendation(userId, title, description);
                titles.Add(title);
                newRecommendations.Add(recommendation);
            }
        }
    }

    private static RecommendationResponse Map(Recommendation recommendation)
        => new(recommendation.Id, recommendation.Title, recommendation.Description, recommendation.IsCompleted, recommendation.ExpiresAtUtc, recommendation.CreatedAtUtc);
}
