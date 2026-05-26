CREATE PROCEDURE [dbo].[Access_Role_Get]
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        r.[RoleId],
        r.[RoleCode],
        r.[RoleName],
        r.[Description],
        r.[IsActive],
        ISNULL(permissions.[Permissions], N'') AS [Permissions]
    FROM [dbo].[AccessRole] r
    OUTER APPLY
    (
        SELECT STRING_AGG(p.[PermissionCode], N', ') AS [Permissions]
        FROM [dbo].[AccessRolePermission] rp
        INNER JOIN [dbo].[AccessPermission] p ON p.[PermissionId] = rp.[PermissionId]
        WHERE rp.[RoleId] = r.[RoleId]
    ) permissions
    WHERE r.[RoleId] = @RoleId;
END;
