using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.AI;

namespace StudyAI.Application.Features.AI.Queries;

public sealed record GetChatSessionsQuery(Guid UserId, Guid DocumentId) : IRequest<IReadOnlyCollection<ChatSessionResponse>>;

public sealed class GetChatSessionsQueryHandler : IRequestHandler<GetChatSessionsQuery, IReadOnlyCollection<ChatSessionResponse>>
{
    private readonly IApplicationDbContext _db;

    public GetChatSessionsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyCollection<ChatSessionResponse>> Handle(GetChatSessionsQuery query, CancellationToken cancellationToken)
    {
        var exists = await _db.Documents.AnyAsync(x => x.Id == query.DocumentId && x.UserId == query.UserId, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException("Document was not found.");
        }

        return await _db.ChatSessions.AsNoTracking().Where(x => x.DocumentId == query.DocumentId && x.UserId == query.UserId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new ChatSessionResponse(x.Id, x.DocumentId, x.Title, x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
