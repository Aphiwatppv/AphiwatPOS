IF OBJECT_ID(N'dbo.SalesDocument', N'U') IS NULL
BEGIN
CREATE TABLE dbo.SalesDocument
(
    SalesDocumentId BIGINT IDENTITY(1,1) NOT NULL,
    SalesHeaderId BIGINT NOT NULL,
    DocumentType NVARCHAR(30) NOT NULL,
    DocumentNo NVARCHAR(50) NOT NULL,
    IssueDate DATETIME2(0) NOT NULL CONSTRAINT DF_SalesDocument_IssueDate DEFAULT (SYSUTCDATETIME()),
    CustomerName NVARCHAR(200) NOT NULL CONSTRAINT DF_SalesDocument_CustomerName DEFAULT (N''),
    CustomerTaxId NVARCHAR(50) NOT NULL CONSTRAINT DF_SalesDocument_CustomerTaxId DEFAULT (N''),
    CustomerBranch NVARCHAR(100) NOT NULL CONSTRAINT DF_SalesDocument_CustomerBranch DEFAULT (N''),
    CustomerAddress NVARCHAR(500) NOT NULL CONSTRAINT DF_SalesDocument_CustomerAddress DEFAULT (N''),
    SubtotalAmount DECIMAL(18,4) NOT NULL,
    DiscountAmount DECIMAL(18,4) NOT NULL,
    VatAmount DECIMAL(18,4) NOT NULL,
    NetAmount DECIMAL(18,4) NOT NULL,
    PrintedCount INT NOT NULL CONSTRAINT DF_SalesDocument_PrintedCount DEFAULT (0),
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_SalesDocument_Status DEFAULT (N'Issued'),
    IssuedByUserId INT NULL,
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_SalesDocument_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_SalesDocument PRIMARY KEY CLUSTERED (SalesDocumentId),
    CONSTRAINT FK_SalesDocument_SalesHeader FOREIGN KEY (SalesHeaderId) REFERENCES dbo.SalesHeader(SalesHeaderId),
    CONSTRAINT FK_SalesDocument_IssuedBy FOREIGN KEY (IssuedByUserId) REFERENCES dbo.AccessUser(UserId),
    CONSTRAINT UQ_SalesDocument_DocumentNo UNIQUE (DocumentNo),
    CONSTRAINT UQ_SalesDocument_Sale_Type UNIQUE (SalesHeaderId, DocumentType),
    CONSTRAINT CK_SalesDocument_Type CHECK (DocumentType IN (N'Receipt', N'ShortTaxInvoice', N'FullTaxInvoice'))
);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SalesDocument_SalesHeaderId' AND object_id = OBJECT_ID(N'dbo.SalesDocument'))
    CREATE INDEX IX_SalesDocument_SalesHeaderId ON dbo.SalesDocument(SalesHeaderId);
GO

CREATE OR ALTER PROCEDURE dbo.spSalesDocumentIssue
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
        SELECT TOP (1) d.*, h.SaleNo
        FROM dbo.SalesDocument d
        JOIN dbo.SalesHeader h ON h.SalesHeaderId=d.SalesHeaderId
        WHERE d.SalesHeaderId=@SalesHeaderId AND d.DocumentType=@DocumentType;
        RETURN;
    END

    DECLARE @Prefix NVARCHAR(10)=CASE @DocumentType WHEN N'Receipt' THEN N'RC' WHEN N'ShortTaxInvoice' THEN N'STAX' ELSE N'TAX' END;
    DECLARE @DocumentNo NVARCHAR(50)=CONCAT(@Prefix, FORMAT(SYSUTCDATETIME(), 'yyyyMMddHHmmssfff'));

    INSERT dbo.SalesDocument
    (
        SalesHeaderId, DocumentType, DocumentNo, CustomerName, CustomerTaxId, CustomerBranch, CustomerAddress,
        SubtotalAmount, DiscountAmount, VatAmount, NetAmount, IssuedByUserId
    )
    SELECT
        h.SalesHeaderId,
        @DocumentType,
        @DocumentNo,
        COALESCE(NULLIF(@CustomerName,N''), c.CustomerName, N'Walk-in'),
        ISNULL(@CustomerTaxId,N''),
        ISNULL(@CustomerBranch,N''),
        ISNULL(@CustomerAddress,N''),
        h.SubtotalAmount,
        h.TotalDiscountAmount,
        h.TaxAmount,
        h.NetAmount,
        NULLIF(@IssuedByUserId,0)
    FROM dbo.SalesHeader h
    LEFT JOIN dbo.Customer c ON c.CustomerId=h.CustomerId
    WHERE h.SalesHeaderId=@SalesHeaderId;

    SELECT TOP (1) d.*, h.SaleNo
    FROM dbo.SalesDocument d
    JOIN dbo.SalesHeader h ON h.SalesHeaderId=d.SalesHeaderId
    WHERE d.SalesDocumentId=SCOPE_IDENTITY();
END;
GO

CREATE OR ALTER PROCEDURE dbo.spSalesDocumentGetBySalesHeaderId @SalesHeaderId BIGINT AS
BEGIN
    SET NOCOUNT ON;
    SELECT d.*, h.SaleNo
    FROM dbo.SalesDocument d
    JOIN dbo.SalesHeader h ON h.SalesHeaderId=d.SalesHeaderId
    WHERE d.SalesHeaderId=@SalesHeaderId
    ORDER BY d.IssueDate DESC, d.SalesDocumentId DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.spSalesDocumentRecordPrint @SalesDocumentId BIGINT, @PrintedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.SalesDocument
    SET PrintedCount = PrintedCount + 1
    WHERE SalesDocumentId=@SalesDocumentId;

    SELECT TOP (1) d.*, h.SaleNo
    FROM dbo.SalesDocument d
    JOIN dbo.SalesHeader h ON h.SalesHeaderId=d.SalesHeaderId
    WHERE d.SalesDocumentId=@SalesDocumentId;
END;
GO
