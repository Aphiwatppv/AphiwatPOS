CREATE PROCEDURE [dbo].[spSalesReturnGetItemsByReturnId] @SalesReturnHeaderId BIGINT AS BEGIN SET NOCOUNT ON; SELECT * FROM dbo.SalesReturnItem WHERE SalesReturnHeaderId=@SalesReturnHeaderId ORDER BY SalesReturnItemId; END;

