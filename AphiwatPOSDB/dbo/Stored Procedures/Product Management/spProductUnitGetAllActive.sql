CREATE PROCEDURE [dbo].[spProductUnitGetAllActive] AS BEGIN SET NOCOUNT ON; SELECT * FROM [dbo].[ProductUnit] WHERE IsActive=1 ORDER BY UnitName; END;
