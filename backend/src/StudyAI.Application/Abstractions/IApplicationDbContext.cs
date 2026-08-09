using Microsoft.EntityFrameworkCore;
using StudyAI.Domain.Entities;

namespace StudyAI.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<UserPreference> UserPreferences { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<EmailVerificationToken> EmailVerificationTokens { get; }
    DbSet<PendingRegistration> PendingRegistrations { get; }
    DbSet<EmailVerificationOtp> EmailVerificationOtps { get; }
    DbSet<CaptchaChallenge> CaptchaChallenges { get; }
    DbSet<Document> Documents { get; }
    DbSet<DocumentCategory> DocumentCategories { get; }
    DbSet<Summary> Summaries { get; }
    DbSet<MindMap> MindMaps { get; }
    DbSet<MindMapNode> MindMapNodes { get; }
    DbSet<Flashcard> Flashcards { get; }
    DbSet<FlashcardReview> FlashcardReviews { get; }
    DbSet<Quiz> Quizzes { get; }
    DbSet<Question> Questions { get; }
    DbSet<QuestionOption> QuestionOptions { get; }
    DbSet<QuizAttempt> QuizAttempts { get; }
    DbSet<UserAnswer> UserAnswers { get; }
    DbSet<Topic> Topics { get; }
    DbSet<LearningProgress> LearningProgress { get; }
    DbSet<UserTopicProgress> UserTopicProgress { get; }
    DbSet<Recommendation> Recommendations { get; }
    DbSet<ChatSession> ChatSessions { get; }
    DbSet<ChatMessage> ChatMessages { get; }
    DbSet<AiUsageLog> AiUsageLogs { get; }
    DbSet<CvScoreSnapshot> CvScoreSnapshots { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<PlusRequest> PlusRequests { get; }
    DbSet<SupportTicket> SupportTickets { get; }
    DbSet<PlanPolicy> PlanPolicies { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
