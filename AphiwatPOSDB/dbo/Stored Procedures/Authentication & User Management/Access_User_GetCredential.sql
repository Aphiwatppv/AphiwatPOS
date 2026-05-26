CREATE PROCEDURE [dbo].[Access_User_GetCredential]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [UserId],
        [Username],
        [DisplayName],
        [PasswordHash],
        [IsActive],
        [IsLocked],
        [LockoutEndAtUtc],
        N'' AS [Roles],
        N'' AS [Permissions]
    FROM [dbo].[AccessUser]
    WHERE [UserId] = @UserId;
END;
