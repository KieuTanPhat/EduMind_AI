using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class Flashcard : Entity
{
    private Flashcard() { }

    public Flashcard(Guid documentId, string question, string answer, string? explanation, string model)
    {
        DocumentId = documentId;
        Question = question;
        Answer = answer;
        Explanation = explanation;
        Model = model;
    }

    public Guid DocumentId { get; private set; }

    public string Question { get; private set; } = null!;

    public string Answer { get; private set; } = null!;

    public string? Explanation { get; private set; }

    public string Model { get; private set; } = null!;

    public Document Document { get; private set; } = null!;

    public ICollection<FlashcardReview> Reviews { get; private set; } = new List<FlashcardReview>();
}
