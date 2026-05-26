CREATE PROCEDURE [dbo].[Access_Permission_SetActive]
    @PermissionId INT,
    @IsActive BIT,
    @UpdatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[AccessPermission]
    SET
        [IsActive] = @IsActive,
        [UpdatedAtUtc] = SYSUTCDATETIME(),
        [UpdatedByUserId] = NULLIF(@UpdatedByUserId, 0)
    WHERE [PermissionId] = @PermissionId;
END;
