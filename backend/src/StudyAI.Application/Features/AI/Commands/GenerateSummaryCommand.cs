using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.AI;
using StudyAI.Domain.Entities;

namespace StudyAI.Application.Features.AI.Commands;

public sealed record GenerateSummaryCommand(Guid UserId, Guid DocumentId, bool ForceRegenerate) : IRequest<SummaryResponse>;

public sealed class GenerateSummaryCommandHandler : IRequestHandler<GenerateSummaryCommand, SummaryResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IAiService _aiService;
    private readonly ITextProcessingService _textProcessing;

    public GenerateSummaryCommandHandler(IApplicationDbContext db, IAiService aiService, ITextProcessingService textProcessing)
    {
        _db = db;
        _aiService = aiService;
        _textProcessing = textProcessing;
    }

    public async Task<SummaryResponse> Handle(GenerateSummaryCommand command, CancellationToken cancellationToken)
    {
        var document = await _db.Documents.Include(x => x.Summary)
            .SingleOrDefaultAsync(x => x.Id == command.DocumentId && x.UserId == command.UserId, cancellationToken)
            ?? throw new NotFoundException("Document was not found.");
        EnsureProcessed(document);

        if (document.Summary is not null && !command.ForceRegenerate)
        {
            return Map(document.Summary);
        }

        var preference = await _db.UserPreferences.AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == command.UserId, cancellationToken);
        var result = await _aiService.GenerateAsync(
            new AiGenerationRequest("summary", BuildContext(document.ExtractedText!), AiPromptTemplates.WithPreferences(AiPromptTemplates.Summary, preference), false),
            cancellationToken);

        _db.AiUsageLogs.Add(new AiUsageLog(command.UserId, "summary", result.Model, result.InputTokens, result.OutputTokens));
        Summary summary;
        if (document.Summary is null)
        {
            summary = new Summary(document.Id, result.Text, result.Model);
            document.SetSummary(summary);
        }
        else
        {
            summary = document.Summary;
            summary.Update(result.Text, result.Model);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Map(summary);
    }

    private string BuildContext(string text) => string.Join("\n\n--- CHUNK ---\n\n", _textProcessing.Chunk(text).Take(6));

    private static void EnsureProcessed(Domain.Entities.Document document)
    {
        if (document.ExtractedText is null || document.Status != Domain.Enums.DocumentStatus.Processed)
        {
            throw new BadRequestException("The document is not processed yet.");
        }
    }

    private static SummaryResponse Map(Summary summary) => new(summary.Id, summary.DocumentId, summary.Content, summary.Model, summary.CreatedAtUtc, summary.UpdatedAtUtc);
}
