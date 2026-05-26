CREATE PROCEDURE [dbo].[Access_User_HasPermission]
    @UserId INT,
    @PermissionCode NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(
        CASE WHEN EXISTS
        (
            SELECT 1
            FROM [dbo].[AccessUser] u
            WHERE u.[UserId] = @UserId
              AND u.[IsActive] = 1
              AND u.[IsLocked] = 0
              AND
              (
                  EXISTS
                  (
                      SELECT 1
                      FROM [dbo].[AccessUserPermission] up
                      INNER JOIN [dbo].[AccessPermission] p ON p.[PermissionId] = up.[PermissionId]
                      WHERE up.[UserId] = u.[UserId]
                        AND p.[PermissionCode] = @PermissionCode
                        AND p.[IsActive] = 1
                  )
                  OR EXISTS
                  (
                      SELECT 1
                      FROM [dbo].[AccessUserRole] ur
                      INNER JOIN [dbo].[AccessRole] r ON r.[RoleId] = ur.[RoleId]
                      INNER JOIN [dbo].[AccessRolePermission] rp ON rp.[RoleId] = ur.[RoleId]
                      INNER JOIN [dbo].[AccessPermission] p ON p.[PermissionId] = rp.[PermissionId]
                      WHERE ur.[UserId] = u.[UserId]
                        AND p.[PermissionCode] = @PermissionCode
                        AND r.[IsActive] = 1
                        AND p.[IsActive] = 1
                  )
              )
        )
        THEN 1 ELSE 0 END AS BIT) AS [IsAuthorized];
END;
