CREATE PROCEDURE [dbo].[spSalesReturnGetById] @SalesReturnHeaderId BIGINT AS BEGIN SET NOCOUNT ON; SELECT r.*, h.SaleNo FROM dbo.SalesReturnHeader r JOIN dbo.SalesHeader h ON h.SalesHeaderId=r.SalesHeaderId WHERE r.SalesReturnHeaderId=@SalesReturnHeaderId; EXEC dbo.spSalesReturnGetItemsByReturnId @SalesReturnHeaderId; EXEC dbo.spSalesReturnGetPaymentsByReturnId @SalesReturnHeaderId; END;

