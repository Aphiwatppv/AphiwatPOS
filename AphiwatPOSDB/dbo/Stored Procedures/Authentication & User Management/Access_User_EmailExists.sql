CREATE PROCEDURE [dbo].[Access_User_EmailExists]
    @Email NVARCHAR(254),
    @ExcludeUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(CASE WHEN @Email <> N'' AND EXISTS
    (
        SELECT 1
        FROM [dbo].[AccessUser]
        WHERE [Email] = @Email
          AND (@ExcludeUserId IS NULL OR [UserId] <> @ExcludeUserId)
    )
    THEN 1 ELSE 0 END AS BIT);
END;
