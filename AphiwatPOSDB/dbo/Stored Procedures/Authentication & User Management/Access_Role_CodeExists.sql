CREATE PROCEDURE [dbo].[Access_Role_CodeExists]
    @RoleCode NVARCHAR(50),
    @ExcludeRoleId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(CASE WHEN EXISTS
    (
        SELECT 1
        FROM [dbo].[AccessRole]
        WHERE [RoleCode] = @RoleCode
          AND (@ExcludeRoleId IS NULL OR [RoleId] <> @ExcludeRoleId)
    )
    THEN 1 ELSE 0 END AS BIT);
END;
