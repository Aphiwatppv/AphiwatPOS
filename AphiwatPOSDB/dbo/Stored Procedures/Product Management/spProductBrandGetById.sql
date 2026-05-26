CREATE PROCEDURE [dbo].[spProductBrandGetById] @BrandId INT AS BEGIN SET NOCOUNT ON; SELECT * FROM [dbo].[ProductBrand] WHERE BrandId=@BrandId; END;
