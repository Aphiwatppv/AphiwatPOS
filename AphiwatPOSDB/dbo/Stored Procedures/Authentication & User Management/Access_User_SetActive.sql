CREATE PROCEDURE [dbo].[Access_User_SetActive]
    @UserId INT,
    @IsActive BIT,
    @UpdatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[AccessUser]
    SET
        [IsActive] = @IsActive,
        [UpdatedAtUtc] = SYSUTCDATETIME(),
        [UpdatedByUserId] = NULLIF(@UpdatedByUserId, 0)
    WHERE [UserId] = @UserId;
END;
