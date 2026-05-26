CREATE PROCEDURE [dbo].[Access_User_UpdateProfileImage]
    @UserId INT,
    @ProfileImageUrl NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[AccessUser]
    SET
        [ProfileImageUrl] = @ProfileImageUrl,
        [UpdatedAtUtc] = SYSUTCDATETIME(),
        [UpdatedByUserId] = @UserId
    WHERE [UserId] = @UserId;
END;
