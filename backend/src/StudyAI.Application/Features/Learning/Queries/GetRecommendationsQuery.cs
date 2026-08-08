using MediatR;
using StudyAI.Application.Abstractions;
using StudyAI.Contracts.Learning;

namespace StudyAI.Application.Features.Learning.Queries;

public sealed record GetRecommendationsQuery(Guid UserId) : IRequest<IReadOnlyCollection<RecommendationResponse>>;

public sealed class GetRecommendationsQueryHandler : IRequestHandler<GetRecommendationsQuery, IReadOnlyCollection<RecommendationResponse>>
{
    private readonly IPersonalizationService _personalization;

    public GetRecommendationsQueryHandler(IPersonalizationService personalization) => _personalization = personalization;

    public Task<IReadOnlyCollection<RecommendationResponse>> Handle(GetRecommendationsQuery query, CancellationToken cancellationToken)
        => _personalization.GetRecommendationsAsync(query.UserId, cancellationToken);
}
