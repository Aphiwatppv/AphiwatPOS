CREATE PROCEDURE [dbo].[Access_User_GetForSignIn]
    @Username NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.[UserId],
        u.[Username],
        u.[DisplayName],
        u.[ProfileImageUrl],
        u.[PasswordHash],
        u.[IsActive],
        u.[IsLocked],
        u.[LockoutEndAtUtc],
        ISNULL(roles.[Roles], N'') AS [Roles],
        ISNULL(rolePermissions.[Permissions], N'') +
            CASE
                WHEN rolePermissions.[Permissions] IS NOT NULL AND userPermissions.[Permissions] IS NOT NULL THEN N','
                ELSE N''
            END +
            ISNULL(userPermissions.[Permissions], N'') AS [Permissions]
    FROM [dbo].[AccessUser] u
    OUTER APPLY
    (
        SELECT STRING_AGG(r.[RoleName], N',') AS [Roles]
        FROM [dbo].[AccessUserRole] ur
        INNER JOIN [dbo].[AccessRole] r ON r.[RoleId] = ur.[RoleId]
        WHERE ur.[UserId] = u.[UserId]
          AND r.[IsActive] = 1
    ) roles
    OUTER APPLY
    (
        SELECT STRING_AGG(p.[PermissionCode], N',') AS [Permissions]
        FROM [dbo].[AccessUserRole] ur
        INNER JOIN [dbo].[AccessRolePermission] rp ON rp.[RoleId] = ur.[RoleId]
        INNER JOIN [dbo].[AccessPermission] p ON p.[PermissionId] = rp.[PermissionId]
        WHERE ur.[UserId] = u.[UserId]
          AND p.[IsActive] = 1
    ) rolePermissions
    OUTER APPLY
    (
        SELECT STRING_AGG(p.[PermissionCode], N',') AS [Permissions]
        FROM [dbo].[AccessUserPermission] up
        INNER JOIN [dbo].[AccessPermission] p ON p.[PermissionId] = up.[PermissionId]
        WHERE up.[UserId] = u.[UserId]
          AND p.[IsActive] = 1
    ) userPermissions
    WHERE u.[Username] = @Username;
END;
