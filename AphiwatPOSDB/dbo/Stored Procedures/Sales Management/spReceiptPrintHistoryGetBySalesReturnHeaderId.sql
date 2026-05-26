CREATE PROCEDURE [dbo].[spReceiptPrintHistoryGetBySalesReturnHeaderId] @SalesReturnHeaderId BIGINT AS BEGIN SET NOCOUNT ON; SELECT * FROM dbo.ReceiptPrintHistory WHERE SalesReturnHeaderId=@SalesReturnHeaderId ORDER BY PrintedDate DESC; END;

