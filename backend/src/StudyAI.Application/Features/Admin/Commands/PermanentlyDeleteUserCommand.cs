using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Application.Common.Exceptions;

namespace StudyAI.Application.Features.Admin.Commands;

public sealed record PermanentlyDeleteUserCommand(Guid UserId) : IRequest;

public sealed class PermanentlyDeleteUserCommandHandler : IRequestHandler<PermanentlyDeleteUserCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;

    public PermanentlyDeleteUserCommandHandler(IApplicationDbContext db, IFileStorageService fileStorage)
    {
        _db = db;
        _fileStorage = fileStorage;
    }

    public async Task Handle(PermanentlyDeleteUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Id == command.UserId, cancellationToken)
            ?? throw new NotFoundException("User was not found.");

        var documentIds = await _db.Documents.Where(x => x.UserId == user.Id).Select(x => x.Id).ToListAsync(cancellationToken);
        var storagePaths = await _db.Documents.Where(x => x.UserId == user.Id).Select(x => x.StoragePath).ToListAsync(cancellationToken);
        var mindMapIds = await _db.MindMaps.Where(x => documentIds.Contains(x.DocumentId)).Select(x => x.Id).ToListAsync(cancellationToken);
        var flashcardIds = await _db.Flashcards.Where(x => documentIds.Contains(x.DocumentId)).Select(x => x.Id).ToListAsync(cancellationToken);
        var quizIds = await _db.Quizzes.Where(x => documentIds.Contains(x.DocumentId)).Select(x => x.Id).ToListAsync(cancellationToken);
        var questionIds = await _db.Questions.Where(x => quizIds.Contains(x.QuizId)).Select(x => x.Id).ToListAsync(cancellationToken);
        var quizAttemptIds = await _db.QuizAttempts.Where(x => x.UserId == user.Id || quizIds.Contains(x.QuizId)).Select(x => x.Id).ToListAsync(cancellationToken);
        var chatSessionIds = await _db.ChatSessions.Where(x => x.UserId == user.Id || documentIds.Contains(x.DocumentId)).Select(x => x.Id).ToListAsync(cancellationToken);

        // Delete restricted dependents explicitly so SQL Server can remove the user without FK conflicts.
        _db.UserAnswers.RemoveRange(_db.UserAnswers.Where(x => quizAttemptIds.Contains(x.QuizAttemptId)));
        _db.QuizAttempts.RemoveRange(_db.QuizAttempts.Where(x => quizAttemptIds.Contains(x.Id)));
        _db.FlashcardReviews.RemoveRange(_db.FlashcardReviews.Where(x => x.UserId == user.Id || flashcardIds.Contains(x.FlashcardId)));
        _db.LearningProgress.RemoveRange(_db.LearningProgress.Where(x => x.UserId == user.Id || documentIds.Contains(x.DocumentId)));
        _db.UserTopicProgress.RemoveRange(_db.UserTopicProgress.Where(x => x.UserId == user.Id));
        _db.AiUsageLogs.RemoveRange(_db.AiUsageLogs.Where(x => x.UserId == user.Id));
        _db.ChatMessages.RemoveRange(_db.ChatMessages.Where(x => chatSessionIds.Contains(x.ChatSessionId)));
        _db.ChatSessions.RemoveRange(_db.ChatSessions.Where(x => chatSessionIds.Contains(x.Id)));
        _db.AuditLogs.RemoveRange(_db.AuditLogs.Where(x => x.UserId == user.Id));
        _db.EmailVerificationTokens.RemoveRange(_db.EmailVerificationTokens.Where(x => x.UserId == user.Id));
        _db.EmailVerificationOtps.RemoveRange(_db.EmailVerificationOtps.Where(x => x.UserId == user.Id));
        _db.PlusRequests.RemoveRange(_db.PlusRequests.Where(x => x.UserId == user.Id));
        _db.SupportTickets.RemoveRange(_db.SupportTickets.Where(x => x.UserId == user.Id));
        _db.Recommendations.RemoveRange(_db.Recommendations.Where(x => x.UserId == user.Id));
        _db.UserPreferences.RemoveRange(_db.UserPreferences.Where(x => x.UserId == user.Id));
        _db.RefreshTokens.RemoveRange(_db.RefreshTokens.Where(x => x.UserId == user.Id));
        _db.UserRoles.RemoveRange(_db.UserRoles.Where(x => x.UserId == user.Id));

        _db.MindMapNodes.RemoveRange(_db.MindMapNodes.Where(x => mindMapIds.Contains(x.MindMapId)));
        _db.MindMaps.RemoveRange(_db.MindMaps.Where(x => mindMapIds.Contains(x.Id)));
        _db.Summaries.RemoveRange(_db.Summaries.Where(x => documentIds.Contains(x.DocumentId)));
        _db.QuestionOptions.RemoveRange(_db.QuestionOptions.Where(x => questionIds.Contains(x.QuestionId)));
        _db.Questions.RemoveRange(_db.Questions.Where(x => questionIds.Contains(x.Id)));
        _db.Quizzes.RemoveRange(_db.Quizzes.Where(x => quizIds.Contains(x.Id)));
        _db.Flashcards.RemoveRange(_db.Flashcards.Where(x => flashcardIds.Contains(x.Id)));
        _db.Documents.RemoveRange(_db.Documents.Where(x => documentIds.Contains(x.Id)));
        _db.Users.Remove(user);

        foreach (var storagePath in storagePaths)
        {
            await _fileStorage.DeleteAsync(storagePath, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
