CREATE PROCEDURE [dbo].[Access_User_AssignRoles]
    @UserId INT,
    @Roles NVARCHAR(400),
    @UpdatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [dbo].[AccessUserRole]
    WHERE [UserId] = @UserId;

    INSERT INTO [dbo].[AccessUserRole] ([UserId], [RoleId])
    SELECT @UserId, r.[RoleId]
    FROM [dbo].[AccessRole] r
    INNER JOIN STRING_SPLIT(@Roles, N',') incoming
        ON r.[RoleCode] = TRIM(incoming.[value]) OR r.[RoleName] = TRIM(incoming.[value])
    WHERE r.[IsActive] = 1;

    UPDATE [dbo].[AccessUser]
    SET [UpdatedAtUtc] = SYSUTCDATETIME(), [UpdatedByUserId] = NULLIF(@UpdatedByUserId, 0)
    WHERE [UserId] = @UserId;
END;
