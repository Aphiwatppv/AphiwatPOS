CREATE PROCEDURE [dbo].[Access_LoginHistory_List]
    @UserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (500)
        [LoginHistoryId],
        [UserId],
        [Username],
        [Succeeded],
        [FailureReason],
        [AttemptedAtUtc],
        [LogoutAtUtc]
    FROM [dbo].[AccessLoginHistory]
    WHERE @UserId IS NULL OR [UserId] = @UserId
    ORDER BY [AttemptedAtUtc] DESC, [LoginHistoryId] DESC;
END;
