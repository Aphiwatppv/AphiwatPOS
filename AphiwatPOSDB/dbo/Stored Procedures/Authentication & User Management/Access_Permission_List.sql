CREATE PROCEDURE [dbo].[Access_Permission_List]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [PermissionId], [PermissionCode], [PermissionName], [ModuleName], [Description], [IsActive]
    FROM [dbo].[AccessPermission]
    ORDER BY [PermissionCode];
END;
