using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.AI;
using StudyAI.Domain.Entities;

namespace StudyAI.Application.Features.AI.Commands;

public sealed record SubmitQuizCommand(Guid UserId, Guid QuizId, SubmitQuizRequest Request) : IRequest<QuizResultResponse>;

public sealed class SubmitQuizCommandHandler : IRequestHandler<SubmitQuizCommand, QuizResultResponse>
{
    private readonly IApplicationDbContext _db;

    public SubmitQuizCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<QuizResultResponse> Handle(SubmitQuizCommand command, CancellationToken cancellationToken)
    {
        var quiz = await _db.Quizzes
            .Include(x => x.Document)
            .Include(x => x.Questions)
            .ThenInclude(x => x.Options)
            .SingleOrDefaultAsync(x => x.Id == command.QuizId && x.Document.UserId == command.UserId, cancellationToken)
            ?? throw new NotFoundException("Quiz was not found.");

        if (quiz.Questions.Count == 0)
        {
            throw new BadRequestException("The quiz has no questions.");
        }

        var answers = command.Request.Answers
            .GroupBy(x => x.QuestionId)
            .ToDictionary(x => x.Key, x => x.Last().SelectedOptionId);
        var questionIds = quiz.Questions.Select(x => x.Id).ToHashSet();
        if (answers.Keys.Any(questionId => !questionIds.Contains(questionId)))
        {
            throw new BadRequestException("The submission contains a question from another quiz.");
        }

        var attempt = new QuizAttempt(quiz.Id, command.UserId);
        var score = 0;
        foreach (var question in quiz.Questions)
        {
            answers.TryGetValue(question.Id, out var selectedOptionId);
            var selectedOption = question.Options.FirstOrDefault(option => option.Id == selectedOptionId);
            var isCorrect = selectedOption?.IsCorrect == true;
            if (isCorrect)
            {
                score++;
            }

            attempt.Answers.Add(new UserAnswer(attempt.Id, question.Id, selectedOption?.Id, isCorrect));
        }

        var completedAtUtc = DateTime.UtcNow;
        attempt.Complete(score, quiz.Questions.Count, completedAtUtc);
        _db.QuizAttempts.Add(attempt);
        await _db.SaveChangesAsync(cancellationToken);

        return new QuizResultResponse(
            attempt.Id,
            quiz.Id,
            score,
            quiz.Questions.Count,
            Math.Round(score * 100m / quiz.Questions.Count, 2),
            completedAtUtc);
    }
}
