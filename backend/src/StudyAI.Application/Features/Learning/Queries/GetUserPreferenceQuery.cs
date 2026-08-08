using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.Learning;

namespace StudyAI.Application.Features.Learning.Queries;

public sealed record GetUserPreferenceQuery(Guid UserId) : IRequest<UserPreferenceResponse>;

public sealed class GetUserPreferenceQueryHandler : IRequestHandler<GetUserPreferenceQuery, UserPreferenceResponse>
{
    private readonly IApplicationDbContext _db;

    public GetUserPreferenceQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<UserPreferenceResponse> Handle(GetUserPreferenceQuery query, CancellationToken cancellationToken)
    {
        var preference = await _db.UserPreferences.SingleOrDefaultAsync(x => x.UserId == query.UserId, cancellationToken)
            ?? throw new NotFoundException("User preference was not found.");
        return new UserPreferenceResponse(preference.LearningLevel, preference.LearningGoal, preference.PreferredLanguage);
    }
}
