using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;
using StudyAI.Contracts.AI;

namespace StudyAI.Application.Features.AI.Queries;

public sealed record GetQuizQuery(Guid UserId, Guid DocumentId) : IRequest<QuizResponse>;

public sealed class GetQuizQueryHandler : IRequestHandler<GetQuizQuery, QuizResponse>
{
    private readonly IApplicationDbContext _db;

    public GetQuizQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<QuizResponse> Handle(GetQuizQuery query, CancellationToken cancellationToken)
    {
        var quiz = await _db.Quizzes.AsNoTracking().Include(x => x.Questions).ThenInclude(x => x.Options)
            .Where(x => x.DocumentId == query.DocumentId && x.Document.UserId == query.UserId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Quiz was not found.");
        return new QuizResponse(quiz.Id, quiz.DocumentId, quiz.Title, quiz.Questions.Select(question => new QuizQuestionResponse(question.Id, question.Content, question.Explanation, question.Options.Select(option => new QuizOptionResponse(option.Id, option.Text, option.IsCorrect)).ToArray())).ToArray(), quiz.CreatedAtUtc, quiz.UpdatedAtUtc);
    }
}
