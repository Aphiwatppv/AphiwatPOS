CREATE PROCEDURE dbo.spSalesDocumentIssue
    @SalesHeaderId BIGINT,
    @DocumentType NVARCHAR(30),
    @CustomerName NVARCHAR(200)=N'',
    @CustomerTaxId NVARCHAR(50)=N'',
    @CustomerBranch NVARCHAR(100)=N'',
    @CustomerAddress NVARCHAR(500)=N'',
    @IssuedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF @DocumentType NOT IN (N'Receipt', N'ShortTaxInvoice', N'FullTaxInvoice') THROW 52400, 'Invalid document type.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.SalesHeader WHERE SalesHeaderId=@SalesHeaderId) THROW 52401, 'Sale was not found.', 1;
    IF EXISTS (SELECT 1 FROM dbo.SalesDocument WHERE SalesHeaderId=@SalesHeaderId AND DocumentType=@DocumentType)
    BEGIN
        SELECT TOP (1) d.*, h.SaleNo FROM dbo.SalesDocument d JOIN dbo.SalesHeader h ON h.SalesHeaderId=d.SalesHeaderId WHERE d.SalesHeaderId=@SalesHeaderId AND d.DocumentType=@DocumentType;
        RETURN;
    END
    DECLARE @Prefix NVARCHAR(10)=CASE @DocumentType WHEN N'Receipt' THEN N'RC' WHEN N'ShortTaxInvoice' THEN N'STAX' ELSE N'TAX' END;
    DECLARE @DocumentNo NVARCHAR(50)=CONCAT(@Prefix, FORMAT(SYSUTCDATETIME(), 'yyyyMMddHHmmssfff'));
    INSERT dbo.SalesDocument (SalesHeaderId, DocumentType, DocumentNo, CustomerName, CustomerTaxId, CustomerBranch, CustomerAddress, SubtotalAmount, DiscountAmount, VatAmount, NetAmount, IssuedByUserId)
    SELECT h.SalesHeaderId, @DocumentType, @DocumentNo, COALESCE(NULLIF(@CustomerName,N''), c.CustomerName, N'Walk-in'), ISNULL(@CustomerTaxId,N''), ISNULL(@CustomerBranch,N''), ISNULL(@CustomerAddress,N''), h.SubtotalAmount, h.TotalDiscountAmount, h.TaxAmount, h.NetAmount, NULLIF(@IssuedByUserId,0)
    FROM dbo.SalesHeader h LEFT JOIN dbo.Customer c ON c.CustomerId=h.CustomerId WHERE h.SalesHeaderId=@SalesHeaderId;
    SELECT TOP (1) d.*, h.SaleNo FROM dbo.SalesDocument d JOIN dbo.SalesHeader h ON h.SalesHeaderId=d.SalesHeaderId WHERE d.SalesDocumentId=SCOPE_IDENTITY();
END;

