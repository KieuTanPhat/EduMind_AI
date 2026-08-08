namespace StudyAI.Application.Abstractions;

public interface IPersonalizationService
{
    Task<IReadOnlyCollection<string>> GetRecommendationsAsync(Guid userId, CancellationToken cancellationToken);
}
