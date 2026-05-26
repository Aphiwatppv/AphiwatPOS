CREATE PROCEDURE [dbo].[Access_Role_Update]
    @RoleId INT,
    @RoleName NVARCHAR(50),
    @Description NVARCHAR(200),
    @IsActive BIT,
    @UpdatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[AccessRole]
    SET
        [RoleName] = @RoleName,
        [Description] = @Description,
        [IsActive] = @IsActive,
        [UpdatedAtUtc] = SYSUTCDATETIME(),
        [UpdatedByUserId] = NULLIF(@UpdatedByUserId, 0)
    WHERE [RoleId] = @RoleId;
END;
