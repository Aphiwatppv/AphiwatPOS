CREATE PROCEDURE [dbo].[spSalesGetDetail] @SalesHeaderId BIGINT AS BEGIN SET NOCOUNT ON; EXEC dbo.spSalesGetById @SalesHeaderId; EXEC dbo.spSalesGetItemsBySalesHeaderId @SalesHeaderId; EXEC dbo.spSalesGetPaymentsBySalesHeaderId @SalesHeaderId; END;

