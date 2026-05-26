CREATE PROCEDURE [dbo].[spProductCategoryGetById] @CategoryId INT AS BEGIN SET NOCOUNT ON; SELECT * FROM [dbo].[ProductCategory] WHERE CategoryId = @CategoryId; END;
