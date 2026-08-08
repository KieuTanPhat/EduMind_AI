using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class AuditLog : Entity
{
    private AuditLog() { }

    public AuditLog(Guid? userId, string action, string resourceType, Guid? resourceId)
    {
        UserId = userId;
        Action = action;
        ResourceType = resourceType;
        ResourceId = resourceId;
    }

    public Guid? UserId { get; private set; }

    public string Action { get; private set; } = null!;

    public string ResourceType { get; private set; } = null!;

    public Guid? ResourceId { get; private set; }

    public string? MetadataJson { get; private set; }

    public User? User { get; private set; }
}
