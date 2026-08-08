IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [DocumentCategories] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_DocumentCategories] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Roles] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    [NormalizedName] nvarchar(50) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Topics] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [NormalizedName] nvarchar(200) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_Topics] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL,
    [Email] nvarchar(256) NOT NULL,
    [NormalizedEmail] nvarchar(256) NOT NULL,
    [PasswordHash] nvarchar(255) NOT NULL,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AIUsageLogs] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Operation] nvarchar(100) NOT NULL,
    [Model] nvarchar(100) NOT NULL,
    [InputTokens] int NOT NULL,
    [OutputTokens] int NOT NULL,
    [Succeeded] bit NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_AIUsageLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AIUsageLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [AuditLogs] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NULL,
    [Action] nvarchar(100) NOT NULL,
    [ResourceType] nvarchar(100) NOT NULL,
    [ResourceId] uniqueidentifier NULL,
    [MetadataJson] nvarchar(max) NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AuditLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [Documents] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [CategoryId] uniqueidentifier NULL,
    [OriginalFileName] nvarchar(255) NOT NULL,
    [StoredFileName] nvarchar(255) NOT NULL,
    [StoragePath] nvarchar(1000) NOT NULL,
    [FileType] int NOT NULL,
    [FileSizeBytes] bigint NOT NULL,
    [Status] int NOT NULL,
    [ExtractedText] nvarchar(max) NULL,
    [ProcessingError] nvarchar(2000) NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_Documents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Documents_DocumentCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [DocumentCategories] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Documents_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Recommendations] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Title] nvarchar(500) NOT NULL,
    [Description] nvarchar(2000) NOT NULL,
    [IsCompleted] bit NOT NULL,
    [ExpiresAtUtc] datetime2 NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_Recommendations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Recommendations_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [RefreshTokens] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [TokenHash] nvarchar(128) NOT NULL,
    [ExpiresAtUtc] datetime2 NOT NULL,
    [RevokedAtUtc] datetime2 NULL,
    [ReplacedByTokenHash] nvarchar(128) NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [UserPreferences] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [LearningLevel] nvarchar(50) NOT NULL,
    [LearningGoal] nvarchar(500) NOT NULL,
    [PreferredLanguage] nvarchar(10) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_UserPreferences] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserPreferences_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [UserRoles] (
    [UserId] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [UserTopicProgress] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [TopicId] uniqueidentifier NOT NULL,
    [AverageScore] decimal(5,2) NOT NULL,
    [Attempts] int NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_UserTopicProgress] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserTopicProgress_Topics_TopicId] FOREIGN KEY ([TopicId]) REFERENCES [Topics] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserTopicProgress_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ChatSessions] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [DocumentId] uniqueidentifier NOT NULL,
    [Title] nvarchar(500) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_ChatSessions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChatSessions_Documents_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [Documents] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ChatSessions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Flashcards] (
    [Id] uniqueidentifier NOT NULL,
    [DocumentId] uniqueidentifier NOT NULL,
    [Question] nvarchar(2000) NOT NULL,
    [Answer] nvarchar(4000) NOT NULL,
    [Explanation] nvarchar(4000) NULL,
    [Model] nvarchar(100) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_Flashcards] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Flashcards_Documents_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [Documents] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [LearningProgress] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [DocumentId] uniqueidentifier NOT NULL,
    [CompletionPercentage] int NOT NULL,
    [StudyMinutes] int NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_LearningProgress] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LearningProgress_Documents_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [Documents] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LearningProgress_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [MindMaps] (
    [Id] uniqueidentifier NOT NULL,
    [DocumentId] uniqueidentifier NOT NULL,
    [Title] nvarchar(500) NOT NULL,
    [Model] nvarchar(100) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_MindMaps] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MindMaps_Documents_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [Documents] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Quizzes] (
    [Id] uniqueidentifier NOT NULL,
    [DocumentId] uniqueidentifier NOT NULL,
    [Title] nvarchar(500) NOT NULL,
    [Model] nvarchar(100) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_Quizzes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Quizzes_Documents_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [Documents] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Summaries] (
    [Id] uniqueidentifier NOT NULL,
    [DocumentId] uniqueidentifier NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [Model] nvarchar(100) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_Summaries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Summaries_Documents_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [Documents] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ChatMessages] (
    [Id] uniqueidentifier NOT NULL,
    [ChatSessionId] uniqueidentifier NOT NULL,
    [Role] nvarchar(30) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_ChatMessages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChatMessages_ChatSessions_ChatSessionId] FOREIGN KEY ([ChatSessionId]) REFERENCES [ChatSessions] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [FlashcardReviews] (
    [Id] uniqueidentifier NOT NULL,
    [FlashcardId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Status] int NOT NULL,
    [ReviewedAtUtc] datetime2 NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_FlashcardReviews] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FlashcardReviews_Flashcards_FlashcardId] FOREIGN KEY ([FlashcardId]) REFERENCES [Flashcards] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_FlashcardReviews_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [MindMapNodes] (
    [Id] uniqueidentifier NOT NULL,
    [MindMapId] uniqueidentifier NOT NULL,
    [ParentNodeId] uniqueidentifier NULL,
    [Label] nvarchar(500) NOT NULL,
    [Description] nvarchar(2000) NULL,
    [Depth] int NOT NULL,
    [PositionX] float NOT NULL,
    [PositionY] float NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_MindMapNodes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MindMapNodes_MindMapNodes_ParentNodeId] FOREIGN KEY ([ParentNodeId]) REFERENCES [MindMapNodes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MindMapNodes_MindMaps_MindMapId] FOREIGN KEY ([MindMapId]) REFERENCES [MindMaps] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Questions] (
    [Id] uniqueidentifier NOT NULL,
    [QuizId] uniqueidentifier NOT NULL,
    [Content] nvarchar(2000) NOT NULL,
    [Explanation] nvarchar(4000) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_Questions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Questions_Quizzes_QuizId] FOREIGN KEY ([QuizId]) REFERENCES [Quizzes] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [QuizAttempts] (
    [Id] uniqueidentifier NOT NULL,
    [QuizId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Score] int NOT NULL,
    [TotalQuestions] int NOT NULL,
    [StartedAtUtc] datetime2 NOT NULL,
    [CompletedAtUtc] datetime2 NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_QuizAttempts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_QuizAttempts_Quizzes_QuizId] FOREIGN KEY ([QuizId]) REFERENCES [Quizzes] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_QuizAttempts_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [QuestionOptions] (
    [Id] uniqueidentifier NOT NULL,
    [QuestionId] uniqueidentifier NOT NULL,
    [Text] nvarchar(1000) NOT NULL,
    [IsCorrect] bit NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_QuestionOptions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_QuestionOptions_Questions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Questions] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [UserAnswers] (
    [Id] uniqueidentifier NOT NULL,
    [QuizAttemptId] uniqueidentifier NOT NULL,
    [QuestionId] uniqueidentifier NOT NULL,
    [SelectedOptionId] uniqueidentifier NULL,
    [IsCorrect] bit NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_UserAnswers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserAnswers_QuestionOptions_SelectedOptionId] FOREIGN KEY ([SelectedOptionId]) REFERENCES [QuestionOptions] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserAnswers_Questions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Questions] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserAnswers_QuizAttempts_QuizAttemptId] FOREIGN KEY ([QuizAttemptId]) REFERENCES [QuizAttempts] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AIUsageLogs_UserId_Operation_CreatedAtUtc] ON [AIUsageLogs] ([UserId], [Operation], [CreatedAtUtc]);
GO

CREATE INDEX [IX_AuditLogs_UserId_CreatedAtUtc] ON [AuditLogs] ([UserId], [CreatedAtUtc]);
GO

CREATE INDEX [IX_ChatMessages_ChatSessionId_CreatedAtUtc] ON [ChatMessages] ([ChatSessionId], [CreatedAtUtc]);
GO

CREATE INDEX [IX_ChatSessions_DocumentId] ON [ChatSessions] ([DocumentId]);
GO

CREATE INDEX [IX_ChatSessions_UserId] ON [ChatSessions] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_DocumentCategories_Name] ON [DocumentCategories] ([Name]);
GO

CREATE INDEX [IX_Documents_CategoryId] ON [Documents] ([CategoryId]);
GO

CREATE INDEX [IX_Documents_UserId_Status] ON [Documents] ([UserId], [Status]);
GO

CREATE INDEX [IX_FlashcardReviews_FlashcardId] ON [FlashcardReviews] ([FlashcardId]);
GO

CREATE INDEX [IX_FlashcardReviews_UserId_FlashcardId_ReviewedAtUtc] ON [FlashcardReviews] ([UserId], [FlashcardId], [ReviewedAtUtc]);
GO

CREATE INDEX [IX_Flashcards_DocumentId] ON [Flashcards] ([DocumentId]);
GO

CREATE INDEX [IX_LearningProgress_DocumentId] ON [LearningProgress] ([DocumentId]);
GO

CREATE UNIQUE INDEX [IX_LearningProgress_UserId_DocumentId] ON [LearningProgress] ([UserId], [DocumentId]);
GO

CREATE INDEX [IX_MindMapNodes_MindMapId_ParentNodeId] ON [MindMapNodes] ([MindMapId], [ParentNodeId]);
GO

CREATE INDEX [IX_MindMapNodes_ParentNodeId] ON [MindMapNodes] ([ParentNodeId]);
GO

CREATE UNIQUE INDEX [IX_MindMaps_DocumentId] ON [MindMaps] ([DocumentId]);
GO

CREATE INDEX [IX_QuestionOptions_QuestionId] ON [QuestionOptions] ([QuestionId]);
GO

CREATE INDEX [IX_Questions_QuizId] ON [Questions] ([QuizId]);
GO

CREATE INDEX [IX_QuizAttempts_QuizId] ON [QuizAttempts] ([QuizId]);
GO

CREATE INDEX [IX_QuizAttempts_UserId_QuizId_CompletedAtUtc] ON [QuizAttempts] ([UserId], [QuizId], [CompletedAtUtc]);
GO

CREATE INDEX [IX_Quizzes_DocumentId] ON [Quizzes] ([DocumentId]);
GO

CREATE INDEX [IX_Recommendations_UserId_IsCompleted_ExpiresAtUtc] ON [Recommendations] ([UserId], [IsCompleted], [ExpiresAtUtc]);
GO

CREATE UNIQUE INDEX [IX_RefreshTokens_TokenHash] ON [RefreshTokens] ([TokenHash]);
GO

CREATE INDEX [IX_RefreshTokens_UserId_ExpiresAtUtc] ON [RefreshTokens] ([UserId], [ExpiresAtUtc]);
GO

CREATE UNIQUE INDEX [IX_Roles_NormalizedName] ON [Roles] ([NormalizedName]);
GO

CREATE UNIQUE INDEX [IX_Summaries_DocumentId] ON [Summaries] ([DocumentId]);
GO

CREATE UNIQUE INDEX [IX_Topics_NormalizedName] ON [Topics] ([NormalizedName]);
GO

CREATE INDEX [IX_UserAnswers_QuestionId] ON [UserAnswers] ([QuestionId]);
GO

CREATE INDEX [IX_UserAnswers_QuizAttemptId] ON [UserAnswers] ([QuizAttemptId]);
GO

CREATE INDEX [IX_UserAnswers_SelectedOptionId] ON [UserAnswers] ([SelectedOptionId]);
GO

CREATE UNIQUE INDEX [IX_UserPreferences_UserId] ON [UserPreferences] ([UserId]);
GO

CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles] ([RoleId]);
GO

CREATE UNIQUE INDEX [IX_Users_NormalizedEmail] ON [Users] ([NormalizedEmail]);
GO

CREATE INDEX [IX_UserTopicProgress_TopicId] ON [UserTopicProgress] ([TopicId]);
GO

CREATE UNIQUE INDEX [IX_UserTopicProgress_UserId_TopicId] ON [UserTopicProgress] ([UserId], [TopicId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260808022514_InitialCreate', N'8.0.20');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'Name', N'NormalizedName', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[Roles]'))
    SET IDENTITY_INSERT [Roles] ON;
INSERT INTO [Roles] ([Id], [CreatedAtUtc], [Name], [NormalizedName], [UpdatedAtUtc])
VALUES ('a1d8f3c5-7c0f-4e2d-a1c2-3fd9e7d18c01', '2025-01-01T00:00:00.0000000Z', N'User', N'USER', NULL),
('b2e9f4d6-8d10-4f3e-b2d3-4fe0f8e29d12', '2025-01-01T00:00:00.0000000Z', N'Admin', N'ADMIN', NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'Name', N'NormalizedName', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[Roles]'))
    SET IDENTITY_INSERT [Roles] OFF;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260808022824_SeedSystemRoles', N'8.0.20');
GO

COMMIT;
GO

