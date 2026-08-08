using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class MindMap : Entity
{
    private MindMap() { }

    public MindMap(Guid documentId, string title, string model)
    {
        DocumentId = documentId;
        Title = title;
        Model = model;
    }

    public Guid DocumentId { get; private set; }

    public string Title { get; private set; } = null!;

    public string Model { get; private set; } = null!;

    public Document Document { get; private set; } = null!;

    public ICollection<MindMapNode> Nodes { get; private set; } = new List<MindMapNode>();

    public void UpdateTitle(string title, string model)
    {
        Title = title;
        Model = model;
        Touch(DateTime.UtcNow);
    }
}
