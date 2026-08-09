using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class User : Entity
{
    private User() { }

    public User(string email, string normalizedEmail, string passwordHash, string firstName, string lastName)
    {
        Email = email;
        NormalizedEmail = normalizedEmail;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
    }

    public string Email { get; private set; } = null!;

    public string NormalizedEmail { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public string FirstName { get; private set; } = null!;

    public string LastName { get; private set; } = null!;

    public bool IsActive { get; private set; } = true;

    public bool IsEmailVerified { get; private set; }

    public bool IsPlus { get; private set; }

    public string Plan { get; private set; } = "Free";

    public long? AiTokenLimitPerDay { get; private set; }

    public DateTime? PlusGrantedAtUtc { get; private set; }

    public DateTime? PlusExpiresAtUtc { get; private set; }

    public UserPreference? Preference { get; private set; }

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    public ICollection<Document> Documents { get; private set; } = new List<Document>();

    public void SetPreference(UserPreference preference)
    {
        Preference = preference;
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        Touch(DateTime.UtcNow);
    }

    public void ChangePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        Touch(DateTime.UtcNow);
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch(DateTime.UtcNow);
    }

    public void Activate()
    {
        IsActive = true;
        Touch(DateTime.UtcNow);
    }

    public void VerifyEmail() => IsEmailVerified = true;

    public bool HasActivePlus(DateTime utcNow) => IsPlus && (!PlusExpiresAtUtc.HasValue || PlusExpiresAtUtc.Value > utcNow);

    public bool IsPro => string.Equals(Plan, "Pro", StringComparison.OrdinalIgnoreCase);

    public void GrantPlus(DateTime utcNow, DateTime? expiresAtUtc = null)
    {
        IsPlus = true;
        Plan = "Plus";
        PlusGrantedAtUtc = utcNow;
        PlusExpiresAtUtc = expiresAtUtc;
        Touch(utcNow);
    }

    public void GrantPro(DateTime utcNow, DateTime? expiresAtUtc = null)
    {
        IsPlus = true;
        Plan = "Pro";
        PlusGrantedAtUtc = utcNow;
        PlusExpiresAtUtc = expiresAtUtc;
        Touch(utcNow);
    }

    public void SetPlan(string plan, DateTime utcNow, DateTime? expiresAtUtc = null)
    {
        if (plan is not ("Free" or "Plus" or "Pro")) throw new ArgumentException("Unsupported plan.", nameof(plan));
        Plan = plan;
        IsPlus = plan is "Plus" or "Pro";
        PlusGrantedAtUtc = IsPlus ? utcNow : null;
        PlusExpiresAtUtc = IsPlus ? expiresAtUtc : null;
        Touch(utcNow);
    }

    public void SetAiTokenLimitPerDay(long? limit)
    {
        if (limit is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limit));
        }

        AiTokenLimitPerDay = limit;
        Touch(DateTime.UtcNow);
    }
}
