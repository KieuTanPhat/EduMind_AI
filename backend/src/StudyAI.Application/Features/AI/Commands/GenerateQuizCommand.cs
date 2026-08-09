using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.AI;
using StudyAI.Domain.Entities;

namespace StudyAI.Application.Features.AI.Commands;

public sealed record GenerateQuizCommand(Guid UserId, Guid DocumentId, bool ForceRegenerate, int? QuestionCount) : IRequest<QuizResponse>;

public sealed class GenerateQuizCommandHandler : IRequestHandler<GenerateQuizCommand, QuizResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IAiService _aiService;
    private readonly ITextProcessingService _textProcessing;

    public GenerateQuizCommandHandler(IApplicationDbContext db, IAiService aiService, ITextProcessingService textProcessing)
    {
        _db = db;
        _aiService = aiService;
        _textProcessing = textProcessing;
    }

    public async Task<QuizResponse> Handle(GenerateQuizCommand command, CancellationToken cancellationToken)
    {
        var questionCount = command.QuestionCount ?? 5;
        if (questionCount is not (5 or 10 or 20))
        {
            throw new BadRequestException("Quiz question count must be 5, 10, or 20.");
        }

        var document = await _db.Documents.SingleOrDefaultAsync(x => x.Id == command.DocumentId && x.UserId == command.UserId, cancellationToken)
            ?? throw new NotFoundException("Document was not found.");
        EnsureProcessed(document);

        if (questionCount == 20)
        {
            await EntitlementPolicy.EnsurePlusAsync(_db, command.UserId, "Quiz 20 câu", cancellationToken);
        }

        var existing = await _db.Quizzes.Include(x => x.Questions).ThenInclude(x => x.Options)
            .Where(x => x.DocumentId == document.Id).OrderByDescending(x => x.CreatedAtUtc).FirstOrDefaultAsync(cancellationToken);
        if (existing is not null && !command.ForceRegenerate)
        {
            return Map(existing);
        }

        await EntitlementPolicy.EnsureDailyAiTokenAllowanceAsync(_db, command.UserId, cancellationToken);

        var preference = await _db.UserPreferences.AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == command.UserId, cancellationToken);
        var result = await _aiService.GenerateAsync(
            new AiGenerationRequest("quiz", BuildContext(document.ExtractedText!), AiPromptTemplates.WithPreferences(AiPromptTemplates.QuizForCount(questionCount), preference), true),
            cancellationToken);
        using var json = AiJsonHelpers.Parse(result.Text);
        var title = AiJsonHelpers.RequiredString(json.RootElement, "title", 500);
        if (!json.RootElement.TryGetProperty("questions", out var questionsElement) || questionsElement.ValueKind != JsonValueKind.Array)
        {
            throw new BadRequestException("AI quiz output must contain a questions array.");
        }

        if (command.ForceRegenerate && existing is not null)
        {
            _db.Quizzes.Remove(existing);
        }

        var quiz = new Quiz(document.Id, title, result.Model);
        foreach (var questionElement in questionsElement.EnumerateArray().Take(questionCount))
        {
            if (!questionElement.TryGetProperty("options", out var optionsElement) || optionsElement.ValueKind != JsonValueKind.Array)
            {
                throw new BadRequestException("Each AI quiz question must contain an options array.");
            }

            var options = optionsElement.EnumerateArray().Take(6).Select(option => new QuestionOption(
                Guid.Empty,
                AiJsonHelpers.RequiredString(option, "text", 1000),
                option.TryGetProperty("isCorrect", out var isCorrect) && isCorrect.ValueKind == JsonValueKind.True)).ToList();
            if (options.Count < 2 || options.Count(x => x.IsCorrect) != 1)
            {
                throw new BadRequestException("Each AI quiz question must have at least two options and exactly one correct option.");
            }

            var question = new Question(quiz.Id, AiJsonHelpers.RequiredString(questionElement, "content", 2000), AiJsonHelpers.RequiredString(questionElement, "explanation", 4000));
            foreach (var option in options)
            {
                question.Options.Add(new QuestionOption(question.Id, option.Text, option.IsCorrect));
            }
            quiz.Questions.Add(question);
        }

        if (quiz.Questions.Count != questionCount)
        {
            throw new BadRequestException($"AI did not return exactly {questionCount} quiz questions. Please try again.");
        }

        _db.Quizzes.Add(quiz);
        _db.AiUsageLogs.Add(new AiUsageLog(command.UserId, "quiz", result.Model, result.InputTokens, result.OutputTokens));
        await _db.SaveChangesAsync(cancellationToken);
        return Map(quiz);
    }

    private string BuildContext(string text) => string.Join("\n\n--- CHUNK ---\n\n", _textProcessing.Chunk(text).Take(6));

    private static void EnsureProcessed(Domain.Entities.Document document)
    {
        if (document.ExtractedText is null || document.Status != Domain.Enums.DocumentStatus.Processed)
        {
            throw new BadRequestException("The document is not processed yet.");
        }
    }

    private static QuizResponse Map(Quiz quiz) => new(
        quiz.Id,
        quiz.DocumentId,
        quiz.Title,
        quiz.Questions.OrderBy(x => x.CreatedAtUtc).Select(question => new QuizQuestionResponse(
            question.Id,
            question.Content,
            question.Explanation,
            question.Options.Select(option => new QuizOptionResponse(option.Id, option.Text, option.IsCorrect)).ToArray())).ToArray(),
        quiz.CreatedAtUtc,
        quiz.UpdatedAtUtc);
}
