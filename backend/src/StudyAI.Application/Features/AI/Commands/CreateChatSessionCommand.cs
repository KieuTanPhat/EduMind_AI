using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.AI;
using StudyAI.Domain.Entities;

namespace StudyAI.Application.Features.AI.Commands;

public sealed record CreateChatSessionCommand(Guid UserId, Guid DocumentId, CreateChatSessionRequest Request) : IRequest<ChatSessionResponse>;

public sealed class CreateChatSessionCommandHandler : IRequestHandler<CreateChatSessionCommand, ChatSessionResponse>
{
    private readonly IApplicationDbContext _db;

    public CreateChatSessionCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ChatSessionResponse> Handle(CreateChatSessionCommand command, CancellationToken cancellationToken)
    {
        var documentExists = await _db.Documents.AnyAsync(x => x.Id == command.DocumentId && x.UserId == command.UserId, cancellationToken);
        if (!documentExists)
        {
            throw new NotFoundException("Document was not found.");
        }

        var title = string.IsNullOrWhiteSpace(command.Request.Title) ? "Document Q&A" : command.Request.Title.Trim()[..Math.Min(command.Request.Title.Trim().Length, 200)];
        var session = new ChatSession(command.UserId, command.DocumentId, title);
        _db.ChatSessions.Add(session);
        await _db.SaveChangesAsync(cancellationToken);
        return new ChatSessionResponse(session.Id, session.DocumentId, session.Title, session.CreatedAtUtc);
    }
}
