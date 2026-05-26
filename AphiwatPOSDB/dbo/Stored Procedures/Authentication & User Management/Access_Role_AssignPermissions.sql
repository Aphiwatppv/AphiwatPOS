CREATE PROCEDURE [dbo].[Access_Role_AssignPermissions]
    @RoleId INT,
    @Permissions NVARCHAR(MAX),
    @UpdatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [dbo].[AccessRolePermission]
    WHERE [RoleId] = @RoleId;

    INSERT INTO [dbo].[AccessRolePermission] ([RoleId], [PermissionId])
    SELECT @RoleId, p.[PermissionId]
    FROM [dbo].[AccessPermission] p
    INNER JOIN STRING_SPLIT(@Permissions, N',') incoming ON p.[PermissionCode] = TRIM(incoming.[value])
    WHERE p.[IsActive] = 1;

    UPDATE [dbo].[AccessRole]
    SET [UpdatedAtUtc] = SYSUTCDATETIME(), [UpdatedByUserId] = NULLIF(@UpdatedByUserId, 0)
    WHERE [RoleId] = @RoleId;
END;
