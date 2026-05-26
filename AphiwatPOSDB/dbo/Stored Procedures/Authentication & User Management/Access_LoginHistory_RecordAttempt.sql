CREATE PROCEDURE [dbo].[Access_LoginHistory_RecordAttempt]
    @UserId INT = NULL,
    @Username NVARCHAR(50),
    @Succeeded BIT,
    @FailureReason NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [dbo].[AccessLoginHistory] ([UserId], [Username], [Succeeded], [FailureReason])
    VALUES (@UserId, @Username, @Succeeded, @FailureReason);

    DECLARE @LoginHistoryId BIGINT = SCOPE_IDENTITY();

    IF @UserId IS NOT NULL AND @Succeeded = 1
    BEGIN
        UPDATE [dbo].[AccessUser]
        SET
            [LastLoginAtUtc] = SYSUTCDATETIME(),
            [AccessFailedCount] = 0,
            [IsLocked] = 0,
            [LockoutEndAtUtc] = NULL
        WHERE [UserId] = @UserId;
    END;

    IF @UserId IS NOT NULL AND @Succeeded = 0
    BEGIN
        UPDATE [dbo].[AccessUser]
        SET
            [AccessFailedCount] = [AccessFailedCount] + 1,
            [IsLocked] = CASE WHEN [AccessFailedCount] + 1 >= 5 THEN 1 ELSE [IsLocked] END,
            [LockoutEndAtUtc] = CASE WHEN [AccessFailedCount] + 1 >= 5 THEN DATEADD(MINUTE, 15, SYSUTCDATETIME()) ELSE [LockoutEndAtUtc] END
        WHERE [UserId] = @UserId;
    END;

    SELECT @LoginHistoryId;
END;
