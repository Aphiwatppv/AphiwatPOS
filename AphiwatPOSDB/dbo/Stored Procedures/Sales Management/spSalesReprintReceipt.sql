CREATE PROCEDURE [dbo].[spSalesReprintReceipt] @SalesHeaderId BIGINT, @PrintedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ReceiptNo NVARCHAR(50)=(SELECT SaleNo FROM dbo.SalesHeader WHERE SalesHeaderId=@SalesHeaderId);
    EXEC dbo.spReceiptPrintHistoryCreate @SalesHeaderId=@SalesHeaderId, @ReceiptNo=@ReceiptNo, @ReceiptType=N'Sale', @PrintedByUserId=@PrintedByUserId;
    SELECT TOP (1) * FROM dbo.ReceiptPrintHistory WHERE SalesHeaderId=@SalesHeaderId ORDER BY ReceiptPrintHistoryId DESC;
END;

