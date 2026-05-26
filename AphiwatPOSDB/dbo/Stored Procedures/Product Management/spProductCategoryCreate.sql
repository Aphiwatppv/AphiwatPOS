CREATE PROCEDURE [dbo].[spProductCategoryCreate] @CategoryCode NVARCHAR(50), @CategoryName NVARCHAR(100), @Description NVARCHAR(500), @DisplayOrder INT, @CreatedByUserId INT AS
BEGIN SET NOCOUNT ON;
    INSERT INTO [dbo].[ProductCategory] (CategoryCode, CategoryName, Description, DisplayOrder, CreatedByUserId)
    VALUES (@CategoryCode, @CategoryName, ISNULL(@Description, N''), @DisplayOrder, NULLIF(@CreatedByUserId, 0));
    SELECT CAST(SCOPE_IDENTITY() AS INT);
END;
