using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class AiUsageLog : Entity
{
    private AiUsageLog() { }

    public AiUsageLog(Guid userId, string operation, string model, int inputTokens, int outputTokens)
    {
        UserId = userId;
        Operation = operation;
        Model = model;
        InputTokens = inputTokens;
        OutputTokens = outputTokens;
    }

    public Guid UserId { get; private set; }

    public string Operation { get; private set; } = null!;

    public string Model { get; private set; } = null!;

    public int InputTokens { get; private set; }

    public int OutputTokens { get; private set; }

    public bool Succeeded { get; private set; } = true;

    public User User { get; private set; } = null!;

    public void MarkFailed() => Succeeded = false;
}
