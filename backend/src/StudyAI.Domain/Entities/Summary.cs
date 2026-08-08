using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class Summary : Entity
{
    private Summary() { }

    public Summary(Guid documentId, string content, string model)
    {
        DocumentId = documentId;
        Content = content;
        Model = model;
    }

    public Guid DocumentId { get; private set; }

    public string Content { get; private set; } = null!;

    public string Model { get; private set; } = null!;

    public Document Document { get; private set; } = null!;

    public void Update(string content, string model)
    {
        Content = content;
        Model = model;
        Touch(DateTime.UtcNow);
    }
}
