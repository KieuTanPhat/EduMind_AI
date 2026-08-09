using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class PlanPolicy : Entity
{
    private PlanPolicy() { }

    public PlanPolicy(string plan, int maxUploadSizeMb, int? dailyDocumentLimit, long? dailyTokenLimit)
    {
        Plan = plan;
        MaxUploadSizeMb = maxUploadSizeMb;
        DailyDocumentLimit = dailyDocumentLimit;
        DailyTokenLimit = dailyTokenLimit;
    }

    public string Plan { get; private set; } = null!;
    public int MaxUploadSizeMb { get; private set; }
    public int? DailyDocumentLimit { get; private set; }
    public long? DailyTokenLimit { get; private set; }

    public void Update(int maxUploadSizeMb, int? dailyDocumentLimit, long? dailyTokenLimit, DateTime utcNow)
    {
        if (maxUploadSizeMb < 1) throw new ArgumentOutOfRangeException(nameof(maxUploadSizeMb));
        if (dailyDocumentLimit is < 0) throw new ArgumentOutOfRangeException(nameof(dailyDocumentLimit));
        if (dailyTokenLimit is < 0) throw new ArgumentOutOfRangeException(nameof(dailyTokenLimit));
        MaxUploadSizeMb = maxUploadSizeMb;
        DailyDocumentLimit = dailyDocumentLimit;
        DailyTokenLimit = dailyTokenLimit;
        Touch(utcNow);
    }
}
