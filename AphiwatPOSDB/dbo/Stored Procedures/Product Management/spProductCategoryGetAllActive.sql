CREATE PROCEDURE [dbo].[spProductCategoryGetAllActive] AS BEGIN SET NOCOUNT ON; SELECT * FROM [dbo].[ProductCategory] WHERE IsActive = 1 ORDER BY DisplayOrder, CategoryName; END;
