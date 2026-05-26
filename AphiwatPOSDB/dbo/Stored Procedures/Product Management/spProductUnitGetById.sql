CREATE PROCEDURE [dbo].[spProductUnitGetById] @UnitId INT AS BEGIN SET NOCOUNT ON; SELECT * FROM [dbo].[ProductUnit] WHERE UnitId=@UnitId; END;
