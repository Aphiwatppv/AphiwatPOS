CREATE PROCEDURE [dbo].[Access_Permission_Create]
    @PermissionCode NVARCHAR(100),
    @PermissionName NVARCHAR(100),
    @ModuleName NVARCHAR(100),
    @Description NVARCHAR(250),
    @CreatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [dbo].[AccessPermission] ([PermissionCode], [PermissionName], [ModuleName], [Description], [CreatedByUserId])
    VALUES (@PermissionCode, @PermissionName, @ModuleName, @Description, NULLIF(@CreatedByUserId, 0));

    SELECT CAST(SCOPE_IDENTITY() AS INT);
END;
