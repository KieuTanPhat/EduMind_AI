using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.AI;

namespace StudyAI.Application.Features.AI.Queries;

public sealed record GetChatMessagesQuery(Guid UserId, Guid SessionId) : IRequest<IReadOnlyCollection<ChatMessageResponse>>;

public sealed class GetChatMessagesQueryHandler : IRequestHandler<GetChatMessagesQuery, IReadOnlyCollection<ChatMessageResponse>>
{
    private readonly IApplicationDbContext _db;

    public GetChatMessagesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyCollection<ChatMessageResponse>> Handle(GetChatMessagesQuery query, CancellationToken cancellationToken)
    {
        var sessionExists = await _db.ChatSessions.AnyAsync(x => x.Id == query.SessionId && x.UserId == query.UserId, cancellationToken);
        if (!sessionExists)
        {
            throw new NotFoundException("Chat session was not found.");
        }

        return await _db.ChatMessages.AsNoTracking().Where(x => x.ChatSessionId == query.SessionId)
            .OrderBy(x => x.CreatedAtUtc)
            .Select(x => new ChatMessageResponse(x.Id, x.ChatSessionId, x.Role, x.Content, x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
