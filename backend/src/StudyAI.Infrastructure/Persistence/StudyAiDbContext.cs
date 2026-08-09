using Microsoft.EntityFrameworkCore;
using StudyAI.Application.Abstractions;
using StudyAI.Domain.Entities;

namespace StudyAI.Infrastructure.Persistence;

public sealed class StudyAiDbContext : DbContext, IApplicationDbContext
{
    public StudyAiDbContext(DbContextOptions<StudyAiDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
    public DbSet<PendingRegistration> PendingRegistrations => Set<PendingRegistration>();
    public DbSet<EmailVerificationOtp> EmailVerificationOtps => Set<EmailVerificationOtp>();
    public DbSet<CaptchaChallenge> CaptchaChallenges => Set<CaptchaChallenge>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentCategory> DocumentCategories => Set<DocumentCategory>();
    public DbSet<Summary> Summaries => Set<Summary>();
    public DbSet<MindMap> MindMaps => Set<MindMap>();
    public DbSet<MindMapNode> MindMapNodes => Set<MindMapNode>();
    public DbSet<Flashcard> Flashcards => Set<Flashcard>();
    public DbSet<FlashcardReview> FlashcardReviews => Set<FlashcardReview>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<UserAnswer> UserAnswers => Set<UserAnswer>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<LearningProgress> LearningProgress => Set<LearningProgress>();
    public DbSet<UserTopicProgress> UserTopicProgress => Set<UserTopicProgress>();
    public DbSet<Recommendation> Recommendations => Set<Recommendation>();
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<AiUsageLog> AiUsageLogs => Set<AiUsageLog>();
    public DbSet<CvScoreSnapshot> CvScoreSnapshots => Set<CvScoreSnapshot>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<PlusRequest> PlusRequests => Set<PlusRequest>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<PlanPolicy> PlanPolicies => Set<PlanPolicy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.NormalizedEmail).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.NormalizedEmail).IsUnique();
            entity.Property(x => x.IsEmailVerified).HasDefaultValue(false);
            entity.Property(x => x.IsPlus).HasDefaultValue(false);
            entity.Property(x => x.Plan).HasMaxLength(20).HasDefaultValue("Free");
            entity.Property(x => x.AiTokenLimitPerDay);
        });

        modelBuilder.Entity<EmailVerificationToken>(entity =>
        {
            entity.ToTable("EmailVerificationTokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => new { x.UserId, x.ExpiresAtUtc });
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PendingRegistration>(entity =>
        {
            entity.ToTable("PendingRegistrations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.NormalizedEmail).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.NormalizedEmail).IsUnique();
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => x.ExpiresAtUtc);
        });

        modelBuilder.Entity<EmailVerificationOtp>(entity =>
        {
            entity.ToTable("EmailVerificationOtps");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CodeHash).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => new { x.UserId, x.ExpiresAtUtc });
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CaptchaChallenge>(entity =>
        {
            entity.ToTable("CaptchaChallenges");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Question).HasMaxLength(200).IsRequired();
            entity.Property(x => x.AnswerHash).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.ExpiresAtUtc);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(50).IsRequired();
            entity.Property(x => x.NormalizedName).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => x.NormalizedName).IsUnique();
            entity.HasData(
                new
                {
                    Id = Guid.Parse("a1d8f3c5-7c0f-4e2d-a1c2-3fd9e7d18c01"),
                    CreatedAtUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = (DateTime?)null,
                    Name = "User",
                    NormalizedName = "USER"
                },
                new
                {
                    Id = Guid.Parse("b2e9f4d6-8d10-4f3e-b2d3-4fe0f8e29d12"),
                    CreatedAtUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = (DateTime?)null,
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                });
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(x => new { x.UserId, x.RoleId });
            entity.HasOne(x => x.User).WithMany(x => x.UserRoles).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Role).WithMany(x => x.UserRoles).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserPreference>(entity =>
        {
            entity.ToTable("UserPreferences");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LearningLevel).HasMaxLength(50).IsRequired();
            entity.Property(x => x.LearningGoal).HasMaxLength(500).IsRequired();
            entity.Property(x => x.PreferredLanguage).HasMaxLength(10).IsRequired();
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.HasOne(x => x.User).WithOne(x => x.Preference).HasForeignKey<UserPreference>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ReplacedByTokenHash).HasMaxLength(128);
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => new { x.UserId, x.ExpiresAtUtc });
            entity.HasOne(x => x.User).WithMany(x => x.RefreshTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("Documents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.StoredFileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.StoragePath).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.ContentHash).HasMaxLength(64);
            entity.HasIndex(x => new { x.UserId, x.ContentHash });
            entity.Property(x => x.ExtractedText).HasColumnType("nvarchar(max)");
            entity.Property(x => x.ProcessingError).HasMaxLength(2000);
            entity.HasIndex(x => new { x.UserId, x.Status });
            entity.HasOne(x => x.User).WithMany(x => x.Documents).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Category).WithMany(x => x.Documents).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DocumentCategory>(entity =>
        {
            entity.ToTable("DocumentCategories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Summary>(entity =>
        {
            entity.ToTable("Summaries");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Content).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(x => x.Model).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.DocumentId).IsUnique();
            entity.HasOne(x => x.Document).WithOne(x => x.Summary).HasForeignKey<Summary>(x => x.DocumentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MindMap>(entity =>
        {
            entity.ToTable("MindMaps");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Model).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.DocumentId).IsUnique();
            entity.HasOne(x => x.Document).WithOne(x => x.MindMap).HasForeignKey<MindMap>(x => x.DocumentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MindMapNode>(entity =>
        {
            entity.ToTable("MindMapNodes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Label).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.HasIndex(x => new { x.MindMapId, x.ParentNodeId });
            entity.HasOne(x => x.MindMap).WithMany(x => x.Nodes).HasForeignKey(x => x.MindMapId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ParentNode).WithMany(x => x.Children).HasForeignKey(x => x.ParentNodeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Flashcard>(entity =>
        {
            entity.ToTable("Flashcards");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Question).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Answer).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.Explanation).HasMaxLength(4000);
            entity.Property(x => x.Model).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.DocumentId);
            entity.HasOne(x => x.Document).WithMany(x => x.Flashcards).HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FlashcardReview>(entity =>
        {
            entity.ToTable("FlashcardReviews");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.FlashcardId, x.ReviewedAtUtc });
            entity.HasOne(x => x.Flashcard).WithMany(x => x.Reviews).HasForeignKey(x => x.FlashcardId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.ToTable("Quizzes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Model).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.DocumentId);
            entity.HasOne(x => x.Document).WithMany(x => x.Quizzes).HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable("Questions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Content).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Explanation).HasMaxLength(4000).IsRequired();
            entity.HasOne(x => x.Quiz).WithMany(x => x.Questions).HasForeignKey(x => x.QuizId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuestionOption>(entity =>
        {
            entity.ToTable("QuestionOptions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Text).HasMaxLength(1000).IsRequired();
            entity.HasOne(x => x.Question).WithMany(x => x.Options).HasForeignKey(x => x.QuestionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuizAttempt>(entity =>
        {
            entity.ToTable("QuizAttempts");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.QuizId, x.CompletedAtUtc });
            entity.HasOne(x => x.Quiz).WithMany(x => x.Attempts).HasForeignKey(x => x.QuizId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserAnswer>(entity =>
        {
            entity.ToTable("UserAnswers");
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.QuizAttempt).WithMany(x => x.Answers).HasForeignKey(x => x.QuizAttemptId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Question).WithMany(x => x.UserAnswers).HasForeignKey(x => x.QuestionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SelectedOption).WithMany().HasForeignKey(x => x.SelectedOptionId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.ToTable("Topics");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.NormalizedName).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => x.NormalizedName).IsUnique();
        });

        modelBuilder.Entity<LearningProgress>(entity =>
        {
            entity.ToTable("LearningProgress");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.DocumentId }).IsUnique();
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Document).WithMany().HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserTopicProgress>(entity =>
        {
            entity.ToTable("UserTopicProgress");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AverageScore).HasPrecision(5, 2);
            entity.HasIndex(x => new { x.UserId, x.TopicId }).IsUnique();
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Topic).WithMany(x => x.UserProgress).HasForeignKey(x => x.TopicId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Recommendation>(entity =>
        {
            entity.ToTable("Recommendations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            entity.HasIndex(x => new { x.UserId, x.IsCompleted, x.ExpiresAtUtc });
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.ToTable("ChatSessions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(500).IsRequired();
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Document).WithMany().HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("ChatMessages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Role).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Content).HasColumnType("nvarchar(max)").IsRequired();
            entity.HasIndex(x => new { x.ChatSessionId, x.CreatedAtUtc });
            entity.HasOne(x => x.ChatSession).WithMany(x => x.Messages).HasForeignKey(x => x.ChatSessionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AiUsageLog>(entity =>
        {
            entity.ToTable("AIUsageLogs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Operation).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Model).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => new { x.UserId, x.Operation, x.CreatedAtUtc });
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CvScoreSnapshot>(entity =>
        {
            entity.ToTable("CvScoreSnapshots");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TargetRole).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ExperienceLevel).HasMaxLength(30).IsRequired();
            entity.Property(x => x.JobDescriptionHash).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ResponseJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(x => x.Model).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => new { x.UserId, x.DocumentId, x.TargetRole, x.ExperienceLevel, x.JobDescriptionHash }).IsUnique();
            entity.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Document>().WithMany().HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Action).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ResourceType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.MetadataJson).HasColumnType("nvarchar(max)");
            entity.HasIndex(x => new { x.UserId, x.CreatedAtUtc });
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PlusRequest>(entity =>
        {
            entity.ToTable("PlusRequests");
            entity.Property(x => x.Plan).HasMaxLength(20).HasDefaultValue("Plus");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.AmountVnd).HasPrecision(18, 2);
            entity.Property(x => x.TransferContent).HasMaxLength(300).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Note).HasMaxLength(1000);
            entity.Property(x => x.SepayTransactionId).HasMaxLength(100);
            entity.Property(x => x.ExpiresAtUtc);
            entity.Property(x => x.PaidAtUtc);
            entity.HasIndex(x => new { x.UserId, x.Status, x.CreatedAtUtc });
            entity.HasIndex(x => x.SepayTransactionId).IsUnique().HasFilter("[SepayTransactionId] IS NOT NULL");
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.ToTable("SupportTickets");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Subject).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.AdminReply).HasMaxLength(4000);
            entity.HasIndex(x => new { x.UserId, x.Status, x.CreatedAtUtc });
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlanPolicy>(entity =>
        {
            entity.ToTable("PlanPolicies");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Plan).HasMaxLength(20).IsRequired();
            entity.HasIndex(x => x.Plan).IsUnique();
            entity.Property(x => x.MaxUploadSizeMb).IsRequired();
        });
    }
}
