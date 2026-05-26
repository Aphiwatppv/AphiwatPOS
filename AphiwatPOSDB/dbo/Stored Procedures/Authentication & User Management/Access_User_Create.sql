CREATE PROCEDURE [dbo].[Access_User_Create]
    @Username NVARCHAR(50),
    @DisplayName NVARCHAR(100),
    @Email NVARCHAR(254),
    @PasswordHash NVARCHAR(500),
    @Roles NVARCHAR(200),
    @CreatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [dbo].[AccessUser] ([Username], [DisplayName], [Email], [PasswordHash], [CreatedByUserId])
    VALUES (@Username, @DisplayName, @Email, @PasswordHash, NULLIF(@CreatedByUserId, 0));

    DECLARE @UserId INT = SCOPE_IDENTITY();

    INSERT INTO [dbo].[AccessUserRole] ([UserId], [RoleId])
    SELECT @UserId, r.[RoleId]
    FROM [dbo].[AccessRole] r
    INNER JOIN STRING_SPLIT(@Roles, N',') incoming
        ON r.[RoleCode] = TRIM(incoming.[value]) OR r.[RoleName] = TRIM(incoming.[value])
    WHERE r.[IsActive] = 1;

    SELECT @UserId;
END;
