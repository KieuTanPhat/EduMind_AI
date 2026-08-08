using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.Learning;
using StudyAI.Domain.Entities;

namespace StudyAI.Application.Features.Learning.Commands;

public sealed record UpdateProgressCommand(Guid UserId, Guid DocumentId, UpdateProgressRequest Request) : IRequest<ProgressDocumentResponse>;

public sealed class UpdateProgressCommandValidator : AbstractValidator<UpdateProgressCommand>
{
    public UpdateProgressCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.DocumentId).NotEmpty();
        RuleFor(x => x.Request.CompletionPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.Request.StudyMinutes).GreaterThanOrEqualTo(0).LessThanOrEqualTo(24 * 60 * 365);
    }
}

public sealed class UpdateProgressCommandHandler : IRequestHandler<UpdateProgressCommand, ProgressDocumentResponse>
{
    private readonly IApplicationDbContext _db;

    public UpdateProgressCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ProgressDocumentResponse> Handle(UpdateProgressCommand command, CancellationToken cancellationToken)
    {
        var document = await _db.Documents.SingleOrDefaultAsync(x => x.Id == command.DocumentId && x.UserId == command.UserId, cancellationToken)
            ?? throw new NotFoundException("Document was not found.");
        var progress = await _db.LearningProgress.SingleOrDefaultAsync(x => x.UserId == command.UserId && x.DocumentId == command.DocumentId, cancellationToken);
        if (progress is null)
        {
            progress = new LearningProgress(command.UserId, command.DocumentId);
            _db.LearningProgress.Add(progress);
        }

        progress.Update(command.Request.CompletionPercentage, command.Request.StudyMinutes);
        await _db.SaveChangesAsync(cancellationToken);
        return new ProgressDocumentResponse(document.Id, document.OriginalFileName, command.Request.CompletionPercentage, command.Request.StudyMinutes, progress.UpdatedAtUtc ?? progress.CreatedAtUtc);
    }
}
