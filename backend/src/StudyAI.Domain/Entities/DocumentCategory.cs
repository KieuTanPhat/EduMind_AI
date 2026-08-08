using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class DocumentCategory : Entity
{
    private DocumentCategory() { }

    public DocumentCategory(string name)
    {
        Name = name;
    }

    public string Name { get; private set; } = null!;

    public ICollection<Document> Documents { get; private set; } = new List<Document>();
}
