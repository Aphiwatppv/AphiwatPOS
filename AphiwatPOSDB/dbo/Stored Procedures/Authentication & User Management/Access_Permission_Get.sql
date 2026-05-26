CREATE PROCEDURE [dbo].[Access_Permission_Get]
    @PermissionId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [PermissionId], [PermissionCode], [PermissionName], [ModuleName], [Description], [IsActive]
    FROM [dbo].[AccessPermission]
    WHERE [PermissionId] = @PermissionId;
END;
