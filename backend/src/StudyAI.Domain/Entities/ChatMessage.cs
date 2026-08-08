using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class ChatMessage : Entity
{
    private ChatMessage() { }

    public ChatMessage(Guid chatSessionId, string role, string content)
    {
        ChatSessionId = chatSessionId;
        Role = role;
        Content = content;
    }

    public Guid ChatSessionId { get; private set; }

    public string Role { get; private set; } = null!;

    public string Content { get; private set; } = null!;

    public ChatSession ChatSession { get; private set; } = null!;
}
