CREATE PROCEDURE [dbo].[Access_LoginHistory_RecordLogout]
    @LoginHistoryId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[AccessLoginHistory]
    SET [LogoutAtUtc] = SYSUTCDATETIME()
    WHERE [LoginHistoryId] = @LoginHistoryId
      AND [LogoutAtUtc] IS NULL;
END;
