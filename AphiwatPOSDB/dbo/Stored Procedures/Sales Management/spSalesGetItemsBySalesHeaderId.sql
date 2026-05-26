CREATE PROCEDURE [dbo].[spSalesGetItemsBySalesHeaderId] @SalesHeaderId BIGINT AS BEGIN SET NOCOUNT ON; SELECT * FROM dbo.SalesItem WHERE SalesHeaderId=@SalesHeaderId ORDER BY SalesItemId; END;

