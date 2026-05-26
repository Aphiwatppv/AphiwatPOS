CREATE PROCEDURE [dbo].[Access_Permission_CodeExists]
    @PermissionCode NVARCHAR(100),
    @ExcludePermissionId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(CASE WHEN EXISTS
    (
        SELECT 1
        FROM [dbo].[AccessPermission]
        WHERE [PermissionCode] = @PermissionCode
          AND (@ExcludePermissionId IS NULL OR [PermissionId] <> @ExcludePermissionId)
    )
    THEN 1 ELSE 0 END AS BIT);
END;
