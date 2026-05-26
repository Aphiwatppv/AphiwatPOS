CREATE PROCEDURE dbo.spSalesDocumentGetBySalesHeaderId @SalesHeaderId BIGINT AS
BEGIN
    SET NOCOUNT ON;
    SELECT d.*, h.SaleNo FROM dbo.SalesDocument d JOIN dbo.SalesHeader h ON h.SalesHeaderId=d.SalesHeaderId WHERE d.SalesHeaderId=@SalesHeaderId ORDER BY d.IssueDate DESC, d.SalesDocumentId DESC;
END;

