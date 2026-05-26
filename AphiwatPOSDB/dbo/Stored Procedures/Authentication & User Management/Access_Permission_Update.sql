CREATE PROCEDURE [dbo].[Access_Permission_Update]
    @PermissionId INT,
    @PermissionName NVARCHAR(100),
    @ModuleName NVARCHAR(100),
    @Description NVARCHAR(250),
    @IsActive BIT,
    @UpdatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[AccessPermission]
    SET
        [PermissionName] = @PermissionName,
        [ModuleName] = @ModuleName,
        [Description] = @Description,
        [IsActive] = @IsActive,
        [UpdatedAtUtc] = SYSUTCDATETIME(),
        [UpdatedByUserId] = NULLIF(@UpdatedByUserId, 0)
    WHERE [PermissionId] = @PermissionId;
END;
