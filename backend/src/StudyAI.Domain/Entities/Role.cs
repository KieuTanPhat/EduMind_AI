using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class Role : Entity
{
    private Role() { }

    public Role(string name)
    {
        Name = name;
        NormalizedName = name.ToUpperInvariant();
    }

    public string Name { get; private set; } = null!;

    public string NormalizedName { get; private set; } = null!;

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
}
