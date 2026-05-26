CREATE PROCEDURE [dbo].[Access_User_Get]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.[UserId],
        u.[Username],
        u.[DisplayName],
        u.[Email],
        u.[ProfileImageUrl],
        u.[IsActive],
        u.[IsLocked],
        u.[LockoutEndAtUtc],
        u.[CreatedAtUtc],
        u.[LastLoginAtUtc],
        ISNULL(roles.[Roles], N'') AS [Roles],
        ISNULL(permissions.[Permissions], N'') AS [Permissions]
    FROM [dbo].[AccessUser] u
    OUTER APPLY
    (
        SELECT STRING_AGG(r.[RoleName], N', ') AS [Roles]
        FROM [dbo].[AccessUserRole] ur
        INNER JOIN [dbo].[AccessRole] r ON r.[RoleId] = ur.[RoleId]
        WHERE ur.[UserId] = u.[UserId]
    ) roles
    OUTER APPLY
    (
        SELECT STRING_AGG(p.[PermissionCode], N', ') AS [Permissions]
        FROM
        (
            SELECT p.[PermissionCode]
            FROM [dbo].[AccessUserPermission] up
            INNER JOIN [dbo].[AccessPermission] p ON p.[PermissionId] = up.[PermissionId]
            WHERE up.[UserId] = u.[UserId]
            UNION
            SELECT p.[PermissionCode]
            FROM [dbo].[AccessUserRole] ur
            INNER JOIN [dbo].[AccessRolePermission] rp ON rp.[RoleId] = ur.[RoleId]
            INNER JOIN [dbo].[AccessPermission] p ON p.[PermissionId] = rp.[PermissionId]
            WHERE ur.[UserId] = u.[UserId]
        ) p
    ) permissions
    WHERE u.[UserId] = @UserId
    ORDER BY u.[Username];
END;
