using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class ChatSession : Entity
{
    private ChatSession() { }

    public ChatSession(Guid userId, Guid documentId, string title)
    {
        UserId = userId;
        DocumentId = documentId;
        Title = title;
    }

    public Guid UserId { get; private set; }

    public Guid DocumentId { get; private set; }

    public string Title { get; private set; } = null!;

    public User User { get; private set; } = null!;

    public Document Document { get; private set; } = null!;

    public ICollection<ChatMessage> Messages { get; private set; } = new List<ChatMessage>();
}
