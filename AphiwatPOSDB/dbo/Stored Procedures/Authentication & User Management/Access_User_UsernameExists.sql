CREATE PROCEDURE [dbo].[Access_User_UsernameExists]
    @Username NVARCHAR(50),
    @ExcludeUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(CASE WHEN EXISTS
    (
        SELECT 1
        FROM [dbo].[AccessUser]
        WHERE [Username] = @Username
          AND (@ExcludeUserId IS NULL OR [UserId] <> @ExcludeUserId)
    )
    THEN 1 ELSE 0 END AS BIT);
END;
