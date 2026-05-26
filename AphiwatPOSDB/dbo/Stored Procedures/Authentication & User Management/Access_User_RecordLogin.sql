CREATE PROCEDURE [dbo].[Access_User_RecordLogin]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[AccessUser]
    SET [LastLoginAtUtc] = SYSUTCDATETIME()
    WHERE [UserId] = @UserId;
END;
