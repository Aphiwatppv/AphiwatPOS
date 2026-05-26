CREATE PROCEDURE [dbo].[Access_User_ListPaged]
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @SearchText NVARCHAR(100) = NULL,
    @IsActive BIT = NULL,
    @IsLocked BIT = NULL,
    @RoleCode NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EffectivePageNumber INT = CASE WHEN @PageNumber < 1 THEN 1 ELSE @PageNumber END;
    DECLARE @EffectivePageSize INT = CASE WHEN @PageSize < 1 THEN 20 ELSE @PageSize END;
    DECLARE @Offset INT = (@EffectivePageNumber - 1) * @EffectivePageSize;

    CREATE TABLE #FilteredUsers ([UserId] INT NOT NULL PRIMARY KEY);

    INSERT INTO #FilteredUsers ([UserId])
    SELECT DISTINCT u.[UserId]
    FROM [dbo].[AccessUser] u
    LEFT JOIN [dbo].[AccessUserRole] ur ON ur.[UserId] = u.[UserId]
    LEFT JOIN [dbo].[AccessRole] r ON r.[RoleId] = ur.[RoleId]
    WHERE (@IsActive IS NULL OR u.[IsActive] = @IsActive)
      AND (@IsLocked IS NULL OR u.[IsLocked] = @IsLocked)
      AND
      (
          @SearchText IS NULL
          OR u.[Username] LIKE N'%' + @SearchText + N'%'
          OR u.[DisplayName] LIKE N'%' + @SearchText + N'%'
          OR u.[Email] LIKE N'%' + @SearchText + N'%'
      )
      AND (@RoleCode IS NULL OR r.[RoleCode] = @RoleCode OR r.[RoleName] = @RoleCode);

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
    FROM #FilteredUsers fu
    INNER JOIN [dbo].[AccessUser] u ON u.[UserId] = fu.[UserId]
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
    ORDER BY u.[Username]
    OFFSET @Offset ROWS FETCH NEXT @EffectivePageSize ROWS ONLY;

    SELECT COUNT(1) FROM #FilteredUsers;
END;
