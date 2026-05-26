CREATE PROCEDURE [dbo].[spReceiptPrintHistoryGetBySalesHeaderId] @SalesHeaderId BIGINT AS BEGIN SET NOCOUNT ON; SELECT * FROM dbo.ReceiptPrintHistory WHERE SalesHeaderId=@SalesHeaderId ORDER BY PrintedDate DESC; END;

