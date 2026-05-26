CREATE PROCEDURE [dbo].[Access_User_ResetPassword]
    @UserId INT,
    @PasswordHash NVARCHAR(500),
    @UpdatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[AccessUser]
    SET
        [PasswordHash] = @PasswordHash,
        [AccessFailedCount] = 0,
        [IsLocked] = 0,
        [LockoutEndAtUtc] = NULL,
        [UpdatedAtUtc] = SYSUTCDATETIME(),
        [UpdatedByUserId] = NULLIF(@UpdatedByUserId, 0)
    WHERE [UserId] = @UserId;
END;
