CREATE PROCEDURE [dbo].[Access_Role_SetActive]
    @RoleId INT,
    @IsActive BIT,
    @UpdatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[AccessRole]
    SET
        [IsActive] = @IsActive,
        [UpdatedAtUtc] = SYSUTCDATETIME(),
        [UpdatedByUserId] = NULLIF(@UpdatedByUserId, 0)
    WHERE [RoleId] = @RoleId;
END;
