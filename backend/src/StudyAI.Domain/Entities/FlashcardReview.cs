using StudyAI.Domain.Common;
using StudyAI.Domain.Enums;

namespace StudyAI.Domain.Entities;

public sealed class FlashcardReview : Entity
{
    private FlashcardReview() { }

    public FlashcardReview(Guid flashcardId, Guid userId, FlashcardReviewStatus status)
    {
        FlashcardId = flashcardId;
        UserId = userId;
        Status = status;
        ReviewedAtUtc = DateTime.UtcNow;
    }

    public Guid FlashcardId { get; private set; }

    public Guid UserId { get; private set; }

    public FlashcardReviewStatus Status { get; private set; }

    public DateTime ReviewedAtUtc { get; private set; }

    public Flashcard Flashcard { get; private set; } = null!;

    public User User { get; private set; } = null!;
}
