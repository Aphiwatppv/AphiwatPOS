CREATE PROCEDURE [dbo].[Access_Role_PermissionList]
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT p.[PermissionId], p.[PermissionCode], p.[PermissionName], p.[ModuleName], p.[Description], p.[IsActive]
    FROM [dbo].[AccessRolePermission] rp
    INNER JOIN [dbo].[AccessPermission] p ON p.[PermissionId] = rp.[PermissionId]
    WHERE rp.[RoleId] = @RoleId
    ORDER BY p.[PermissionCode];
END;
