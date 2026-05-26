CREATE PROCEDURE [dbo].[Access_User_SetLock]
    @UserId INT,
    @IsLocked BIT,
    @LockoutEndAtUtc DATETIME2(0) = NULL,
    @UpdatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[AccessUser]
    SET
        [IsLocked] = @IsLocked,
        [LockoutEndAtUtc] = CASE WHEN @IsLocked = 1 THEN @LockoutEndAtUtc ELSE NULL END,
        [AccessFailedCount] = CASE WHEN @IsLocked = 0 THEN 0 ELSE [AccessFailedCount] END,
        [UpdatedAtUtc] = SYSUTCDATETIME(),
        [UpdatedByUserId] = NULLIF(@UpdatedByUserId, 0)
    WHERE [UserId] = @UserId;
END;
