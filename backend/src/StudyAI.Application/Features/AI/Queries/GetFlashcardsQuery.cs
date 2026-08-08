using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.AI;

namespace StudyAI.Application.Features.AI.Queries;

public sealed record GetFlashcardsQuery(Guid UserId, Guid DocumentId) : IRequest<FlashcardsResponse>;

public sealed class GetFlashcardsQueryHandler : IRequestHandler<GetFlashcardsQuery, FlashcardsResponse>
{
    private readonly IApplicationDbContext _db;

    public GetFlashcardsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<FlashcardsResponse> Handle(GetFlashcardsQuery query, CancellationToken cancellationToken)
    {
        var cards = await _db.Flashcards.AsNoTracking()
            .Where(x => x.DocumentId == query.DocumentId && x.Document.UserId == query.UserId)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
        if (cards.Count == 0)
        {
            throw new NotFoundException("Flashcards were not found.");
        }

        return new FlashcardsResponse(query.DocumentId, cards.Select(card => new FlashcardResponse(card.Id, card.DocumentId, card.Question, card.Answer, card.Explanation, card.CreatedAtUtc)).ToArray());
    }
}
