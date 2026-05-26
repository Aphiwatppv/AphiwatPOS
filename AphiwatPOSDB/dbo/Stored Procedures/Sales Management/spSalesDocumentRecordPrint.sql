CREATE PROCEDURE dbo.spSalesDocumentRecordPrint @SalesDocumentId BIGINT, @PrintedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.SalesDocument SET PrintedCount = PrintedCount + 1 WHERE SalesDocumentId=@SalesDocumentId;
    SELECT TOP (1) d.*, h.SaleNo FROM dbo.SalesDocument d JOIN dbo.SalesHeader h ON h.SalesHeaderId=d.SalesHeaderId WHERE d.SalesDocumentId=@SalesDocumentId;
END;

