using StudyAI.Contracts.Learning;

namespace StudyAI.Application.Abstractions;

public interface IPersonalizationService
{
    Task<IReadOnlyCollection<RecommendationResponse>> GetRecommendationsAsync(Guid userId, CancellationToken cancellationToken);
}
