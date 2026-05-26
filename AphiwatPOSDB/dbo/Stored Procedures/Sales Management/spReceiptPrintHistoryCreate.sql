CREATE PROCEDURE [dbo].[spReceiptPrintHistoryCreate] @SalesHeaderId BIGINT=NULL, @SalesReturnHeaderId BIGINT=NULL, @ReceiptNo NVARCHAR(50), @ReceiptType NVARCHAR(20), @PrintedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.ReceiptPrintHistory (SalesHeaderId, SalesReturnHeaderId, ReceiptNo, ReceiptType, PrintedByUserId) VALUES (@SalesHeaderId, @SalesReturnHeaderId, @ReceiptNo, @ReceiptType, @PrintedByUserId);
    SELECT CONVERT(BIGINT, SCOPE_IDENTITY());
END;

