using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.Learning;
using StudyAI.Domain.Entities;
using StudyAI.Domain.Enums;

namespace StudyAI.Application.Features.Learning.Commands;

public sealed record ReviewFlashcardCommand(Guid UserId, Guid FlashcardId, ReviewFlashcardRequest Request) : IRequest;

public sealed class ReviewFlashcardCommandValidator : AbstractValidator<ReviewFlashcardCommand>
{
    public ReviewFlashcardCommandValidator()
    {
        RuleFor(x => x.Request.Status).NotEmpty().Must(status => Enum.TryParse<FlashcardReviewStatus>(status, true, out var parsed) && parsed != FlashcardReviewStatus.Unknown || status.Equals("unknown", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Status must be known, review or unknown.");
    }
}

public sealed class ReviewFlashcardCommandHandler : IRequestHandler<ReviewFlashcardCommand>
{
    private readonly IApplicationDbContext _db;

    public ReviewFlashcardCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(ReviewFlashcardCommand command, CancellationToken cancellationToken)
    {
        var flashcard = await _db.Flashcards.Include(x => x.Document)
            .SingleOrDefaultAsync(x => x.Id == command.FlashcardId && x.Document.UserId == command.UserId, cancellationToken)
            ?? throw new NotFoundException("Flashcard was not found.");
        var status = command.Request.Status.ToLowerInvariant() switch
        {
            "known" => FlashcardReviewStatus.Known,
            "review" => FlashcardReviewStatus.Review,
            "unknown" => FlashcardReviewStatus.Unknown,
            _ => throw new BadRequestException("Status must be known, review or unknown.")
        };
        _db.FlashcardReviews.Add(new FlashcardReview(flashcard.Id, command.UserId, status));
        await _db.SaveChangesAsync(cancellationToken);
    }
}
