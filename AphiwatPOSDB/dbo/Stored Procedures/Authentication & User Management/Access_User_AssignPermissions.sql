CREATE PROCEDURE [dbo].[Access_User_AssignPermissions]
    @UserId INT,
    @Permissions NVARCHAR(MAX),
    @UpdatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [dbo].[AccessUserPermission]
    WHERE [UserId] = @UserId;

    INSERT INTO [dbo].[AccessUserPermission] ([UserId], [PermissionId])
    SELECT @UserId, p.[PermissionId]
    FROM [dbo].[AccessPermission] p
    INNER JOIN STRING_SPLIT(@Permissions, N',') incoming ON p.[PermissionCode] = TRIM(incoming.[value]);

    UPDATE [dbo].[AccessUser]
    SET [UpdatedAtUtc] = SYSUTCDATETIME(), [UpdatedByUserId] = NULLIF(@UpdatedByUserId, 0)
    WHERE [UserId] = @UserId;
END;
