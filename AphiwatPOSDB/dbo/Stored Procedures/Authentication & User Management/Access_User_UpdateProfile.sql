CREATE PROCEDURE [dbo].[Access_User_UpdateProfile]
    @UserId INT,
    @DisplayName NVARCHAR(100),
    @Email NVARCHAR(254)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[AccessUser]
    SET
        [DisplayName] = @DisplayName,
        [Email] = @Email,
        [UpdatedAtUtc] = SYSUTCDATETIME(),
        [UpdatedByUserId] = @UserId
    WHERE [UserId] = @UserId;
END;
