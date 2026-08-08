using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class MindMapNode : Entity
{
    private MindMapNode() { }

    public MindMapNode(Guid mindMapId, string label, int depth, Guid? parentNodeId = null)
    {
        MindMapId = mindMapId;
        Label = label;
        Depth = depth;
        ParentNodeId = parentNodeId;
    }

    public Guid MindMapId { get; private set; }

    public Guid? ParentNodeId { get; private set; }

    public string Label { get; private set; } = null!;

    public string? Description { get; private set; }

    public int Depth { get; private set; }

    public double PositionX { get; private set; }

    public double PositionY { get; private set; }

    public MindMap MindMap { get; private set; } = null!;

    public MindMapNode? ParentNode { get; private set; }

    public ICollection<MindMapNode> Children { get; private set; } = new List<MindMapNode>();

    public void SetPosition(double x, double y)
    {
        PositionX = x;
        PositionY = y;
        Touch(DateTime.UtcNow);
    }

    public void SetDescription(string? description)
    {
        Description = description;
        Touch(DateTime.UtcNow);
    }
}
