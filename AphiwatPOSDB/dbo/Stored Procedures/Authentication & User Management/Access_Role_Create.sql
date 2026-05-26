CREATE PROCEDURE [dbo].[Access_Role_Create]
    @RoleCode NVARCHAR(50),
    @RoleName NVARCHAR(50),
    @Description NVARCHAR(200),
    @CreatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [dbo].[AccessRole] ([RoleCode], [RoleName], [Description], [CreatedByUserId])
    VALUES (@RoleCode, @RoleName, @Description, NULLIF(@CreatedByUserId, 0));

    SELECT CAST(SCOPE_IDENTITY() AS INT);
END;
