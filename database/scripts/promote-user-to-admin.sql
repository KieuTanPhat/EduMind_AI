-- Replace the email before running this script against the StudyAI database.
DECLARE @Email nvarchar(256) = N'admin@example.com';
DECLARE @UserId uniqueidentifier = (SELECT Id FROM Users WHERE NormalizedEmail = UPPER(@Email));
DECLARE @AdminRoleId uniqueidentifier = (SELECT Id FROM Roles WHERE NormalizedName = N'ADMIN');

IF @UserId IS NULL
    THROW 50001, 'The user email was not found.', 1;

IF @AdminRoleId IS NULL
    THROW 50002, 'The Admin role was not found. Apply migrations first.', 1;

IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @UserId AND RoleId = @AdminRoleId)
    INSERT INTO UserRoles (Id, CreatedAtUtc, UserId, RoleId) VALUES (NEWID(), SYSUTCDATETIME(), @UserId, @AdminRoleId);
