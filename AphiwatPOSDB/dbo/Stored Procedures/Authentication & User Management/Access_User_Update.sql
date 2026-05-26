CREATE PROCEDURE [dbo].[Access_User_Update]
    @UserId INT,
    @DisplayName NVARCHAR(100),
    @Email NVARCHAR(254),
    @IsActive BIT,
    @Roles NVARCHAR(200),
    @UpdatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[AccessUser]
    SET
        [DisplayName] = @DisplayName,
        [Email] = @Email,
        [IsActive] = @IsActive,
        [UpdatedAtUtc] = SYSUTCDATETIME(),
        [UpdatedByUserId] = NULLIF(@UpdatedByUserId, 0)
    WHERE [UserId] = @UserId;

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
