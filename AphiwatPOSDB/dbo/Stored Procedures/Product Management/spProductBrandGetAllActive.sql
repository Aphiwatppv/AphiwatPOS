CREATE PROCEDURE [dbo].[spProductBrandGetAllActive] AS BEGIN SET NOCOUNT ON; SELECT * FROM [dbo].[ProductBrand] WHERE IsActive=1 ORDER BY BrandName; END;
