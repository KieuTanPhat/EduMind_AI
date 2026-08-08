using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.AI;
using StudyAI.Domain.Entities;

namespace StudyAI.Application.Features.AI.Commands;

public sealed record GenerateFlashcardsCommand(Guid UserId, Guid DocumentId, bool ForceRegenerate) : IRequest<FlashcardsResponse>;

public sealed class GenerateFlashcardsCommandHandler : IRequestHandler<GenerateFlashcardsCommand, FlashcardsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IAiService _aiService;
    private readonly ITextProcessingService _textProcessing;

    public GenerateFlashcardsCommandHandler(IApplicationDbContext db, IAiService aiService, ITextProcessingService textProcessing)
    {
        _db = db;
        _aiService = aiService;
        _textProcessing = textProcessing;
    }

    public async Task<FlashcardsResponse> Handle(GenerateFlashcardsCommand command, CancellationToken cancellationToken)
    {
        var document = await _db.Documents.SingleOrDefaultAsync(x => x.Id == command.DocumentId && x.UserId == command.UserId, cancellationToken)
            ?? throw new NotFoundException("Document was not found.");
        EnsureProcessed(document);

        var existing = await _db.Flashcards.AsNoTracking().Where(x => x.DocumentId == document.Id).ToListAsync(cancellationToken);
        if (existing.Count > 0 && !command.ForceRegenerate)
        {
            return Map(document.Id, existing);
        }

        var preference = await _db.UserPreferences.AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == command.UserId, cancellationToken);
        var result = await _aiService.GenerateAsync(
            new AiGenerationRequest("flashcards", BuildContext(document.ExtractedText!), AiPromptTemplates.WithPreferences(AiPromptTemplates.Flashcards, preference), true),
            cancellationToken);
        using var json = AiJsonHelpers.Parse(result.Text);
        var cardsElement = json.RootElement.ValueKind == JsonValueKind.Array
            ? json.RootElement
            : json.RootElement.TryGetProperty("cards", out var cards) && cards.ValueKind == JsonValueKind.Array
                ? cards
                : throw new BadRequestException("AI flashcard output must contain a cards array.");

        if (command.ForceRegenerate && existing.Count > 0)
        {
            _db.Flashcards.RemoveRange(existing);
        }

        var flashcards = new List<Flashcard>();
        foreach (var card in cardsElement.EnumerateArray().Take(30))
        {
            var flashcard = new Flashcard(
                document.Id,
                AiJsonHelpers.RequiredString(card, "question", 2000),
                AiJsonHelpers.RequiredString(card, "answer", 4000),
                AiJsonHelpers.OptionalString(card, "explanation", 4000),
                result.Model);
            flashcards.Add(flashcard);
            _db.Flashcards.Add(flashcard);
        }

        if (flashcards.Count == 0)
        {
            throw new BadRequestException("AI did not return any flashcards.");
        }

        _db.AiUsageLogs.Add(new AiUsageLog(command.UserId, "flashcards", result.Model, result.InputTokens, result.OutputTokens));
        await _db.SaveChangesAsync(cancellationToken);
        return Map(document.Id, flashcards);
    }

    private string BuildContext(string text) => string.Join("\n\n--- CHUNK ---\n\n", _textProcessing.Chunk(text).Take(6));

    private static void EnsureProcessed(Domain.Entities.Document document)
    {
        if (document.ExtractedText is null || document.Status != Domain.Enums.DocumentStatus.Processed)
        {
            throw new BadRequestException("The document is not processed yet.");
        }
    }

    private static FlashcardsResponse Map(Guid documentId, IEnumerable<Flashcard> cards) => new(documentId, cards.Select(card => new FlashcardResponse(card.Id, card.DocumentId, card.Question, card.Answer, card.Explanation, card.CreatedAtUtc)).ToArray());
}
