using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.AI;
using StudyAI.Domain.Entities;

namespace StudyAI.Application.Features.AI.Commands;

public sealed record SendChatMessageCommand(Guid UserId, Guid SessionId, SendChatMessageRequest Request) : IRequest<ChatMessageResponse>;

public sealed class SendChatMessageCommandHandler : IRequestHandler<SendChatMessageCommand, ChatMessageResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IAiService _aiService;
    private readonly ITextProcessingService _textProcessing;

    public SendChatMessageCommandHandler(IApplicationDbContext db, IAiService aiService, ITextProcessingService textProcessing)
    {
        _db = db;
        _aiService = aiService;
        _textProcessing = textProcessing;
    }

    public async Task<ChatMessageResponse> Handle(SendChatMessageCommand command, CancellationToken cancellationToken)
    {
        var content = command.Request.Content?.Trim();
        if (string.IsNullOrWhiteSpace(content) || content.Length > 4000)
        {
            throw new BadRequestException("Chat message must contain between 1 and 4000 characters.");
        }

        var session = await _db.ChatSessions
            .Include(x => x.Document)
            .Include(x => x.Messages)
            .SingleOrDefaultAsync(x => x.Id == command.SessionId && x.UserId == command.UserId, cancellationToken)
            ?? throw new NotFoundException("Chat session was not found.");
        if (session.Document.ExtractedText is null)
        {
            throw new BadRequestException("The document is not processed yet.");
        }

        var userMessage = new ChatMessage(session.Id, "user", content);
        session.Messages.Add(userMessage);
        var history = string.Join("\n", session.Messages.OrderByDescending(x => x.CreatedAtUtc).Take(10).Reverse().Select(message => $"{message.Role}: {message.Content}"));
        var preference = await _db.UserPreferences.AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == command.UserId, cancellationToken);
        var prompt = $"{AiPromptTemplates.WithPreferences(AiPromptTemplates.Chat, preference)}\n\nCHAT HISTORY:\n{history}\n\nUSER QUESTION:\n{content}";
        var result = await _aiService.GenerateAsync(
            new AiGenerationRequest("chat", BuildContext(session.Document.ExtractedText), prompt, false),
            cancellationToken);

        var assistantMessage = new ChatMessage(session.Id, "assistant", result.Text);
        session.Messages.Add(assistantMessage);
        _db.AiUsageLogs.Add(new AiUsageLog(command.UserId, "chat", result.Model, result.InputTokens, result.OutputTokens));
        await _db.SaveChangesAsync(cancellationToken);
        return new ChatMessageResponse(assistantMessage.Id, session.Id, assistantMessage.Role, assistantMessage.Content, assistantMessage.CreatedAtUtc);
    }

    private string BuildContext(string text) => string.Join("\n\n--- CHUNK ---\n\n", _textProcessing.Chunk(text).Take(6));
}
