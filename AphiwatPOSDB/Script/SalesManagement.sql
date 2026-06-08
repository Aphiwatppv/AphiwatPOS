IF OBJECT_ID(N'dbo.PaymentMethod', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[PaymentMethod]
(
    [PaymentMethodId] INT IDENTITY(1,1) NOT NULL,
    [PaymentMethodCode] NVARCHAR(50) NOT NULL,
    [PaymentMethodName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NOT NULL CONSTRAINT [DF_PaymentMethod_Description] DEFAULT (N''),
    [RequireReferenceNo] BIT NOT NULL CONSTRAINT [DF_PaymentMethod_RequireReferenceNo] DEFAULT (0),
    [IsCash] BIT NOT NULL CONSTRAINT [DF_PaymentMethod_IsCash] DEFAULT (0),
    [IsActive] BIT NOT NULL CONSTRAINT [DF_PaymentMethod_IsActive] DEFAULT (1),
    [DisplayOrder] INT NOT NULL CONSTRAINT [DF_PaymentMethod_DisplayOrder] DEFAULT (0),
    [CreatedByUserId] INT NULL,
    [UpdatedByUserId] INT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_PaymentMethod_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    [UpdatedDate] DATETIME2(0) NULL,
    CONSTRAINT [PK_PaymentMethod] PRIMARY KEY CLUSTERED ([PaymentMethodId]),
    CONSTRAINT [UQ_PaymentMethod_Code] UNIQUE ([PaymentMethodCode]),
    CONSTRAINT [FK_PaymentMethod_CreatedBy] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[AccessUser] ([UserId]),
    CONSTRAINT [FK_PaymentMethod_UpdatedBy] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[AccessUser] ([UserId])
);
END;
GO

IF OBJECT_ID(N'dbo.SalesHeader', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SalesHeader]
(
    [SalesHeaderId] BIGINT IDENTITY(1,1) NOT NULL,
    [SaleNo] NVARCHAR(50) NOT NULL,
    [SaleDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_SalesHeader_SaleDate] DEFAULT (SYSUTCDATETIME()),
    [CustomerId] INT NULL,
    [CashierUserId] INT NOT NULL,
    [SubtotalAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_SalesHeader_SubtotalAmount] DEFAULT (0),
    [ItemDiscountAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_SalesHeader_ItemDiscountAmount] DEFAULT (0),
    [OrderDiscountAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_SalesHeader_OrderDiscountAmount] DEFAULT (0),
    [TotalDiscountAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_SalesHeader_TotalDiscountAmount] DEFAULT (0),
    [TaxAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_SalesHeader_TaxAmount] DEFAULT (0),
    [NetAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_SalesHeader_NetAmount] DEFAULT (0),
    [PaidAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_SalesHeader_PaidAmount] DEFAULT (0),
    [ChangeAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_SalesHeader_ChangeAmount] DEFAULT (0),
    [Status] NVARCHAR(30) NOT NULL CONSTRAINT [DF_SalesHeader_Status] DEFAULT (N'Completed'),
    [Remark] NVARCHAR(1000) NOT NULL CONSTRAINT [DF_SalesHeader_Remark] DEFAULT (N''),
    [CreatedByUserId] INT NULL,
    [UpdatedByUserId] INT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_SalesHeader_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    [UpdatedDate] DATETIME2(0) NULL,
    CONSTRAINT [PK_SalesHeader] PRIMARY KEY CLUSTERED ([SalesHeaderId]),
    CONSTRAINT [UQ_SalesHeader_SaleNo] UNIQUE ([SaleNo]),
    CONSTRAINT [FK_SalesHeader_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customer] ([CustomerId]),
    CONSTRAINT [FK_SalesHeader_Cashier] FOREIGN KEY ([CashierUserId]) REFERENCES [dbo].[AccessUser] ([UserId]),
    CONSTRAINT [FK_SalesHeader_CreatedBy] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[AccessUser] ([UserId]),
    CONSTRAINT [FK_SalesHeader_UpdatedBy] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[AccessUser] ([UserId]),
    CONSTRAINT [CK_SalesHeader_Status] CHECK ([Status] IN (N'Completed',N'PartiallyRefunded',N'Refunded',N'Voided')),
    CONSTRAINT [CK_SalesHeader_Amounts] CHECK ([SubtotalAmount] >= 0 AND [ItemDiscountAmount] >= 0 AND [OrderDiscountAmount] >= 0 AND [TotalDiscountAmount] >= 0 AND [TaxAmount] >= 0 AND [NetAmount] >= 0 AND [PaidAmount] >= 0 AND [ChangeAmount] >= 0)
);
END;
GO

IF OBJECT_ID(N'dbo.SalesItem', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SalesItem]
(
    [SalesItemId] BIGINT IDENTITY(1,1) NOT NULL,
    [SalesHeaderId] BIGINT NOT NULL,
    [ProductId] INT NOT NULL,
    [ProductCodeSnapshot] NVARCHAR(50) NOT NULL,
    [ProductNameSnapshot] NVARCHAR(200) NOT NULL,
    [BarcodeSnapshot] NVARCHAR(100) NULL,
    [UnitId] INT NOT NULL,
    [UnitSymbolSnapshot] NVARCHAR(30) NOT NULL,
    [Quantity] DECIMAL(18,4) NOT NULL,
    [UnitPrice] DECIMAL(18,4) NOT NULL,
    [CostPriceSnapshot] DECIMAL(18,4) NOT NULL,
    [ItemDiscountAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_SalesItem_ItemDiscountAmount] DEFAULT (0),
    [TaxAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_SalesItem_TaxAmount] DEFAULT (0),
    [LineSubtotal] DECIMAL(18,4) NOT NULL,
    [LineTotal] DECIMAL(18,4) NOT NULL,
    [ReturnedQty] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_SalesItem_ReturnedQty] DEFAULT (0),
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_SalesItem_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_SalesItem] PRIMARY KEY CLUSTERED ([SalesItemId]),
    CONSTRAINT [FK_SalesItem_Header] FOREIGN KEY ([SalesHeaderId]) REFERENCES [dbo].[SalesHeader] ([SalesHeaderId]),
    CONSTRAINT [FK_SalesItem_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([ProductId]),
    CONSTRAINT [FK_SalesItem_Unit] FOREIGN KEY ([UnitId]) REFERENCES [dbo].[ProductUnit] ([UnitId]),
    CONSTRAINT [CK_SalesItem_Quantity] CHECK ([Quantity] > 0 AND [ReturnedQty] >= 0 AND [ReturnedQty] <= [Quantity]),
    CONSTRAINT [CK_SalesItem_Amounts] CHECK ([UnitPrice] >= 0 AND [CostPriceSnapshot] >= 0 AND [ItemDiscountAmount] >= 0 AND [TaxAmount] >= 0 AND [LineSubtotal] >= 0 AND [LineTotal] >= 0 AND [ItemDiscountAmount] <= [LineSubtotal])
);
END;
GO

IF OBJECT_ID(N'dbo.SalesPayment', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SalesPayment]
(
    [SalesPaymentId] BIGINT IDENTITY(1,1) NOT NULL,
    [SalesHeaderId] BIGINT NOT NULL,
    [PaymentMethodId] INT NOT NULL,
    [PaymentAmount] DECIMAL(18,4) NOT NULL,
    [ReferenceNo] NVARCHAR(100) NULL,
    [PaymentDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_SalesPayment_PaymentDate] DEFAULT (SYSUTCDATETIME()),
    [CreatedByUserId] INT NOT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_SalesPayment_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_SalesPayment] PRIMARY KEY CLUSTERED ([SalesPaymentId]),
    CONSTRAINT [FK_SalesPayment_Header] FOREIGN KEY ([SalesHeaderId]) REFERENCES [dbo].[SalesHeader] ([SalesHeaderId]),
    CONSTRAINT [FK_SalesPayment_Method] FOREIGN KEY ([PaymentMethodId]) REFERENCES [dbo].[PaymentMethod] ([PaymentMethodId]),
    CONSTRAINT [FK_SalesPayment_CreatedBy] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[AccessUser] ([UserId]),
    CONSTRAINT [CK_SalesPayment_Amount] CHECK ([PaymentAmount] > 0)
);
END;
GO

IF OBJECT_ID(N'dbo.HeldSaleHeader', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[HeldSaleHeader]
(
    [HeldSaleHeaderId] BIGINT IDENTITY(1,1) NOT NULL,
    [HeldSaleNo] NVARCHAR(50) NOT NULL,
    [HeldDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_HeldSaleHeader_HeldDate] DEFAULT (SYSUTCDATETIME()),
    [CustomerId] INT NULL,
    [CashierUserId] INT NOT NULL,
    [Note] NVARCHAR(1000) NOT NULL CONSTRAINT [DF_HeldSaleHeader_Note] DEFAULT (N''),
    [EstimatedSubtotalAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_HeldSaleHeader_EstimatedSubtotalAmount] DEFAULT (0),
    [EstimatedDiscountAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_HeldSaleHeader_EstimatedDiscountAmount] DEFAULT (0),
    [EstimatedTaxAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_HeldSaleHeader_EstimatedTaxAmount] DEFAULT (0),
    [EstimatedNetAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_HeldSaleHeader_EstimatedNetAmount] DEFAULT (0),
    [Status] NVARCHAR(30) NOT NULL CONSTRAINT [DF_HeldSaleHeader_Status] DEFAULT (N'Held'),
    [CreatedByUserId] INT NULL,
    [UpdatedByUserId] INT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_HeldSaleHeader_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    [UpdatedDate] DATETIME2(0) NULL,
    CONSTRAINT [PK_HeldSaleHeader] PRIMARY KEY CLUSTERED ([HeldSaleHeaderId]),
    CONSTRAINT [UQ_HeldSaleHeader_HeldSaleNo] UNIQUE ([HeldSaleNo]),
    CONSTRAINT [FK_HeldSaleHeader_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customer] ([CustomerId]),
    CONSTRAINT [FK_HeldSaleHeader_Cashier] FOREIGN KEY ([CashierUserId]) REFERENCES [dbo].[AccessUser] ([UserId]),
    CONSTRAINT [CK_HeldSaleHeader_Status] CHECK ([Status] IN (N'Held',N'Resumed',N'Cancelled',N'Expired',N'Completed')),
    CONSTRAINT [CK_HeldSaleHeader_Amounts] CHECK ([EstimatedSubtotalAmount] >= 0 AND [EstimatedDiscountAmount] >= 0 AND [EstimatedTaxAmount] >= 0 AND [EstimatedNetAmount] >= 0)
);
END;
GO

IF OBJECT_ID(N'dbo.HeldSaleItem', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[HeldSaleItem]
(
    [HeldSaleItemId] BIGINT IDENTITY(1,1) NOT NULL,
    [HeldSaleHeaderId] BIGINT NOT NULL,
    [ProductId] INT NOT NULL,
    [ProductCodeSnapshot] NVARCHAR(50) NOT NULL,
    [ProductNameSnapshot] NVARCHAR(200) NOT NULL,
    [BarcodeSnapshot] NVARCHAR(100) NULL,
    [UnitId] INT NOT NULL,
    [UnitSymbolSnapshot] NVARCHAR(30) NOT NULL,
    [Quantity] DECIMAL(18,4) NOT NULL,
    [UnitPrice] DECIMAL(18,4) NOT NULL,
    [CostPriceSnapshot] DECIMAL(18,4) NOT NULL,
    [ItemDiscountAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_HeldSaleItem_ItemDiscountAmount] DEFAULT (0),
    [TaxAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_HeldSaleItem_TaxAmount] DEFAULT (0),
    [LineSubtotal] DECIMAL(18,4) NOT NULL,
    [LineTotal] DECIMAL(18,4) NOT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_HeldSaleItem_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_HeldSaleItem] PRIMARY KEY CLUSTERED ([HeldSaleItemId]),
    CONSTRAINT [FK_HeldSaleItem_Header] FOREIGN KEY ([HeldSaleHeaderId]) REFERENCES [dbo].[HeldSaleHeader] ([HeldSaleHeaderId]),
    CONSTRAINT [FK_HeldSaleItem_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([ProductId]),
    CONSTRAINT [FK_HeldSaleItem_Unit] FOREIGN KEY ([UnitId]) REFERENCES [dbo].[ProductUnit] ([UnitId]),
    CONSTRAINT [CK_HeldSaleItem_Quantity] CHECK ([Quantity] > 0),
    CONSTRAINT [CK_HeldSaleItem_Amounts] CHECK ([UnitPrice] >= 0 AND [CostPriceSnapshot] >= 0 AND [ItemDiscountAmount] >= 0 AND [TaxAmount] >= 0 AND [LineSubtotal] >= 0 AND [LineTotal] >= 0 AND [ItemDiscountAmount] <= [LineSubtotal])
);
END;
GO

IF OBJECT_ID(N'dbo.SalesReturnHeader', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SalesReturnHeader]
(
    [SalesReturnHeaderId] BIGINT IDENTITY(1,1) NOT NULL,
    [ReturnNo] NVARCHAR(50) NOT NULL,
    [SalesHeaderId] BIGINT NOT NULL,
    [ReturnDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_SalesReturnHeader_ReturnDate] DEFAULT (SYSUTCDATETIME()),
    [CustomerId] INT NULL,
    [CashierUserId] INT NOT NULL,
    [RefundSubtotalAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_SalesReturnHeader_RefundSubtotalAmount] DEFAULT (0),
    [RefundDiscountAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_SalesReturnHeader_RefundDiscountAmount] DEFAULT (0),
    [RefundTaxAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_SalesReturnHeader_RefundTaxAmount] DEFAULT (0),
    [RefundNetAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_SalesReturnHeader_RefundNetAmount] DEFAULT (0),
    [Reason] NVARCHAR(500) NOT NULL,
    [Status] NVARCHAR(30) NOT NULL CONSTRAINT [DF_SalesReturnHeader_Status] DEFAULT (N'Draft'),
    [ApprovedByUserId] INT NULL,
    [ApprovedDate] DATETIME2(0) NULL,
    [CreatedByUserId] INT NULL,
    [UpdatedByUserId] INT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_SalesReturnHeader_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    [UpdatedDate] DATETIME2(0) NULL,
    CONSTRAINT [PK_SalesReturnHeader] PRIMARY KEY CLUSTERED ([SalesReturnHeaderId]),
    CONSTRAINT [UQ_SalesReturnHeader_ReturnNo] UNIQUE ([ReturnNo]),
    CONSTRAINT [FK_SalesReturnHeader_SalesHeader] FOREIGN KEY ([SalesHeaderId]) REFERENCES [dbo].[SalesHeader] ([SalesHeaderId]),
    CONSTRAINT [FK_SalesReturnHeader_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customer] ([CustomerId]),
    CONSTRAINT [FK_SalesReturnHeader_Cashier] FOREIGN KEY ([CashierUserId]) REFERENCES [dbo].[AccessUser] ([UserId]),
    CONSTRAINT [FK_SalesReturnHeader_ApprovedBy] FOREIGN KEY ([ApprovedByUserId]) REFERENCES [dbo].[AccessUser] ([UserId]),
    CONSTRAINT [CK_SalesReturnHeader_Status] CHECK ([Status] IN (N'Draft',N'Approved',N'Rejected',N'Completed',N'Cancelled')),
    CONSTRAINT [CK_SalesReturnHeader_Amounts] CHECK ([RefundSubtotalAmount] >= 0 AND [RefundDiscountAmount] >= 0 AND [RefundTaxAmount] >= 0 AND [RefundNetAmount] >= 0)
);
END;
GO

IF OBJECT_ID(N'dbo.SalesReturnItem', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SalesReturnItem]
(
    [SalesReturnItemId] BIGINT IDENTITY(1,1) NOT NULL,
    [SalesReturnHeaderId] BIGINT NOT NULL,
    [SalesItemId] BIGINT NOT NULL,
    [ProductId] INT NOT NULL,
    [ProductCodeSnapshot] NVARCHAR(50) NOT NULL,
    [ProductNameSnapshot] NVARCHAR(200) NOT NULL,
    [BarcodeSnapshot] NVARCHAR(100) NULL,
    [UnitId] INT NOT NULL,
    [UnitSymbolSnapshot] NVARCHAR(30) NOT NULL,
    [QuantityReturned] DECIMAL(18,4) NOT NULL,
    [RefundUnitPrice] DECIMAL(18,4) NOT NULL,
    [RefundAmount] DECIMAL(18,4) NOT NULL,
    [ReturnToStock] BIT NOT NULL CONSTRAINT [DF_SalesReturnItem_ReturnToStock] DEFAULT (0),
    [ReturnCondition] NVARCHAR(30) NOT NULL,
    [Reason] NVARCHAR(500) NOT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_SalesReturnItem_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_SalesReturnItem] PRIMARY KEY CLUSTERED ([SalesReturnItemId]),
    CONSTRAINT [FK_SalesReturnItem_Header] FOREIGN KEY ([SalesReturnHeaderId]) REFERENCES [dbo].[SalesReturnHeader] ([SalesReturnHeaderId]),
    CONSTRAINT [FK_SalesReturnItem_SalesItem] FOREIGN KEY ([SalesItemId]) REFERENCES [dbo].[SalesItem] ([SalesItemId]),
    CONSTRAINT [FK_SalesReturnItem_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([ProductId]),
    CONSTRAINT [FK_SalesReturnItem_Unit] FOREIGN KEY ([UnitId]) REFERENCES [dbo].[ProductUnit] ([UnitId]),
    CONSTRAINT [CK_SalesReturnItem_Condition] CHECK ([ReturnCondition] IN (N'Good',N'Damaged',N'Expired',N'Defective')),
    CONSTRAINT [CK_SalesReturnItem_ReturnToStock] CHECK ([ReturnToStock] = 0 OR [ReturnCondition] = N'Good'),
    CONSTRAINT [CK_SalesReturnItem_Amounts] CHECK ([QuantityReturned] > 0 AND [RefundUnitPrice] >= 0 AND [RefundAmount] >= 0)
);
END;
GO

IF OBJECT_ID(N'dbo.SalesReturnPayment', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SalesReturnPayment]
(
    [SalesReturnPaymentId] BIGINT IDENTITY(1,1) NOT NULL,
    [SalesReturnHeaderId] BIGINT NOT NULL,
    [PaymentMethodId] INT NOT NULL,
    [RefundAmount] DECIMAL(18,4) NOT NULL,
    [ReferenceNo] NVARCHAR(100) NULL,
    [PaymentDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_SalesReturnPayment_PaymentDate] DEFAULT (SYSUTCDATETIME()),
    [CreatedByUserId] INT NOT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_SalesReturnPayment_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_SalesReturnPayment] PRIMARY KEY CLUSTERED ([SalesReturnPaymentId]),
    CONSTRAINT [FK_SalesReturnPayment_Header] FOREIGN KEY ([SalesReturnHeaderId]) REFERENCES [dbo].[SalesReturnHeader] ([SalesReturnHeaderId]),
    CONSTRAINT [FK_SalesReturnPayment_Method] FOREIGN KEY ([PaymentMethodId]) REFERENCES [dbo].[PaymentMethod] ([PaymentMethodId]),
    CONSTRAINT [FK_SalesReturnPayment_CreatedBy] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[AccessUser] ([UserId]),
    CONSTRAINT [CK_SalesReturnPayment_Amount] CHECK ([RefundAmount] > 0)
);
END;
GO

IF OBJECT_ID(N'dbo.ReceiptPrintHistory', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[ReceiptPrintHistory]
(
    [ReceiptPrintHistoryId] BIGINT IDENTITY(1,1) NOT NULL,
    [SalesHeaderId] BIGINT NULL,
    [SalesReturnHeaderId] BIGINT NULL,
    [ReceiptNo] NVARCHAR(50) NOT NULL,
    [ReceiptType] NVARCHAR(20) NOT NULL,
    [PrintedByUserId] INT NOT NULL,
    [PrintedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_ReceiptPrintHistory_PrintedDate] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_ReceiptPrintHistory] PRIMARY KEY CLUSTERED ([ReceiptPrintHistoryId]),
    CONSTRAINT [FK_ReceiptPrintHistory_SalesHeader] FOREIGN KEY ([SalesHeaderId]) REFERENCES [dbo].[SalesHeader] ([SalesHeaderId]),
    CONSTRAINT [FK_ReceiptPrintHistory_SalesReturnHeader] FOREIGN KEY ([SalesReturnHeaderId]) REFERENCES [dbo].[SalesReturnHeader] ([SalesReturnHeaderId]),
    CONSTRAINT [FK_ReceiptPrintHistory_PrintedBy] FOREIGN KEY ([PrintedByUserId]) REFERENCES [dbo].[AccessUser] ([UserId]),
    CONSTRAINT [CK_ReceiptPrintHistory_Type] CHECK ([ReceiptType] IN (N'Sale',N'Return')),
    CONSTRAINT [CK_ReceiptPrintHistory_Parent] CHECK (([SalesHeaderId] IS NOT NULL AND [SalesReturnHeaderId] IS NULL) OR ([SalesHeaderId] IS NULL AND [SalesReturnHeaderId] IS NOT NULL))
);
END;
GO

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

IF OBJECT_ID(N'dbo.DailySalesClosing', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[DailySalesClosing]
(
    [DailySalesClosingId] BIGINT IDENTITY(1,1) NOT NULL,
    [ClosingDate] DATE NOT NULL,
    [CashierUserId] INT NULL,
    [GrossSalesAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_DailySalesClosing_GrossSalesAmount] DEFAULT (0),
    [DiscountAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_DailySalesClosing_DiscountAmount] DEFAULT (0),
    [TaxAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_DailySalesClosing_TaxAmount] DEFAULT (0),
    [NetSalesAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_DailySalesClosing_NetSalesAmount] DEFAULT (0),
    [RefundAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_DailySalesClosing_RefundAmount] DEFAULT (0),
    [ExpectedCashAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_DailySalesClosing_ExpectedCashAmount] DEFAULT (0),
    [ActualCashAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_DailySalesClosing_ActualCashAmount] DEFAULT (0),
    [CashDifferenceAmount] AS ([ActualCashAmount] - [ExpectedCashAmount]) PERSISTED,
    [GrossProfitAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_DailySalesClosing_GrossProfitAmount] DEFAULT (0),
    [Notes] NVARCHAR(1000) NOT NULL CONSTRAINT [DF_DailySalesClosing_Notes] DEFAULT (N''),
    [ClosedByUserId] INT NOT NULL,
    [ClosedAtUtc] DATETIME2(0) NOT NULL CONSTRAINT [DF_DailySalesClosing_ClosedAtUtc] DEFAULT (SYSUTCDATETIME()),
    [UpdatedByUserId] INT NULL,
    [UpdatedAtUtc] DATETIME2(0) NULL,
    CONSTRAINT [PK_DailySalesClosing] PRIMARY KEY CLUSTERED ([DailySalesClosingId]),
    CONSTRAINT [UQ_DailySalesClosing_Date_Cashier] UNIQUE ([ClosingDate], [CashierUserId]),
    CONSTRAINT [FK_DailySalesClosing_Cashier] FOREIGN KEY ([CashierUserId]) REFERENCES [dbo].[AccessUser] ([UserId]),
    CONSTRAINT [FK_DailySalesClosing_ClosedBy] FOREIGN KEY ([ClosedByUserId]) REFERENCES [dbo].[AccessUser] ([UserId]),
    CONSTRAINT [FK_DailySalesClosing_UpdatedBy] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[AccessUser] ([UserId])
);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SalesHeader_SaleDate_Status' AND object_id = OBJECT_ID(N'dbo.SalesHeader')) CREATE INDEX [IX_SalesHeader_SaleDate_Status] ON [dbo].[SalesHeader] ([SaleDate] DESC, [Status]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SalesHeader_CustomerId' AND object_id = OBJECT_ID(N'dbo.SalesHeader')) CREATE INDEX [IX_SalesHeader_CustomerId] ON [dbo].[SalesHeader] ([CustomerId]) WHERE [CustomerId] IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SalesItem_Header' AND object_id = OBJECT_ID(N'dbo.SalesItem')) CREATE INDEX [IX_SalesItem_Header] ON [dbo].[SalesItem] ([SalesHeaderId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SalesPayment_Header' AND object_id = OBJECT_ID(N'dbo.SalesPayment')) CREATE INDEX [IX_SalesPayment_Header] ON [dbo].[SalesPayment] ([SalesHeaderId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_HeldSaleHeader_Status_Date' AND object_id = OBJECT_ID(N'dbo.HeldSaleHeader')) CREATE INDEX [IX_HeldSaleHeader_Status_Date] ON [dbo].[HeldSaleHeader] ([Status], [HeldDate] DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SalesReturnHeader_SalesHeaderId' AND object_id = OBJECT_ID(N'dbo.SalesReturnHeader')) CREATE INDEX [IX_SalesReturnHeader_SalesHeaderId] ON [dbo].[SalesReturnHeader] ([SalesHeaderId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ReceiptPrintHistory_SalesHeaderId' AND object_id = OBJECT_ID(N'dbo.ReceiptPrintHistory')) CREATE INDEX [IX_ReceiptPrintHistory_SalesHeaderId] ON [dbo].[ReceiptPrintHistory] ([SalesHeaderId]) WHERE [SalesHeaderId] IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SalesDocument_SalesHeaderId' AND object_id = OBJECT_ID(N'dbo.SalesDocument')) CREATE INDEX IX_SalesDocument_SalesHeaderId ON dbo.SalesDocument(SalesHeaderId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DailySalesClosing_Date' AND object_id = OBJECT_ID(N'dbo.DailySalesClosing')) CREATE INDEX [IX_DailySalesClosing_Date] ON [dbo].[DailySalesClosing] ([ClosingDate] DESC);
GO

MERGE [dbo].[PaymentMethod] AS target
USING (VALUES
    (N'CASH', N'Cash', N'Cash payment', 0, 1, 10),
    (N'CARD', N'Credit/Debit Card', N'Card terminal payment', 1, 0, 20),
    (N'QR', N'QR Payment', N'QR code or mobile banking payment', 1, 0, 30),
    (N'TRANSFER', N'Bank Transfer', N'Bank transfer payment', 1, 0, 40),
    (N'VOUCHER', N'Voucher', N'Voucher or gift certificate', 1, 0, 50),
    (N'CREDIT', N'Customer Credit', N'Member customer credit account payment', 0, 0, 60)
) AS source ([PaymentMethodCode], [PaymentMethodName], [Description], [RequireReferenceNo], [IsCash], [DisplayOrder])
ON target.[PaymentMethodCode] = source.[PaymentMethodCode]
WHEN NOT MATCHED THEN
    INSERT ([PaymentMethodCode], [PaymentMethodName], [Description], [RequireReferenceNo], [IsCash], [IsActive], [DisplayOrder])
    VALUES (source.[PaymentMethodCode], source.[PaymentMethodName], source.[Description], source.[RequireReferenceNo], source.[IsCash], 1, source.[DisplayOrder]);
GO

CREATE OR ALTER PROCEDURE [dbo].[spPaymentMethodGetAll] AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM [dbo].[PaymentMethod] ORDER BY DisplayOrder, PaymentMethodName;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spPaymentMethodGetAllActive] AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM [dbo].[PaymentMethod] WHERE IsActive = 1 ORDER BY DisplayOrder, PaymentMethodName;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spPaymentMethodGetById] @PaymentMethodId INT AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM [dbo].[PaymentMethod] WHERE PaymentMethodId = @PaymentMethodId;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spPaymentMethodCreate]
    @PaymentMethodCode NVARCHAR(50), @PaymentMethodName NVARCHAR(100), @Description NVARCHAR(500) = N'', @RequireReferenceNo BIT = 0, @IsCash BIT = 0, @DisplayOrder INT = 0, @CreatedByUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.PaymentMethod WHERE PaymentMethodCode = @PaymentMethodCode) THROW 52000, 'Payment method code already exists.', 1;
    INSERT INTO dbo.PaymentMethod (PaymentMethodCode, PaymentMethodName, Description, RequireReferenceNo, IsCash, DisplayOrder, CreatedByUserId)
    VALUES (@PaymentMethodCode, @PaymentMethodName, ISNULL(@Description,N''), @RequireReferenceNo, @IsCash, @DisplayOrder, NULLIF(@CreatedByUserId,0));
    SELECT CONVERT(INT, SCOPE_IDENTITY());
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spPaymentMethodUpdate]
    @PaymentMethodId INT, @PaymentMethodCode NVARCHAR(50), @PaymentMethodName NVARCHAR(100), @Description NVARCHAR(500) = N'', @RequireReferenceNo BIT = 0, @IsCash BIT = 0, @IsActive BIT = 1, @DisplayOrder INT = 0, @UpdatedByUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.PaymentMethod WHERE PaymentMethodCode = @PaymentMethodCode AND PaymentMethodId <> @PaymentMethodId) THROW 52001, 'Payment method code already exists.', 1;
    UPDATE dbo.PaymentMethod SET PaymentMethodCode=@PaymentMethodCode, PaymentMethodName=@PaymentMethodName, Description=ISNULL(@Description,N''), RequireReferenceNo=@RequireReferenceNo, IsCash=@IsCash, IsActive=@IsActive, DisplayOrder=@DisplayOrder, UpdatedByUserId=NULLIF(@UpdatedByUserId,0), UpdatedDate=SYSUTCDATETIME()
    WHERE PaymentMethodId=@PaymentMethodId;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spPaymentMethodToggleActive] @PaymentMethodId INT, @IsActive BIT, @UpdatedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.PaymentMethod SET IsActive=@IsActive, UpdatedByUserId=NULLIF(@UpdatedByUserId,0), UpdatedDate=SYSUTCDATETIME() WHERE PaymentMethodId=@PaymentMethodId;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spPaymentMethodCheckCodeExists] @PaymentMethodCode NVARCHAR(50), @ExcludePaymentMethodId INT = NULL AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.PaymentMethod WHERE PaymentMethodCode=@PaymentMethodCode AND (@ExcludePaymentMethodId IS NULL OR PaymentMethodId<>@ExcludePaymentMethodId)) THEN 1 ELSE 0 END AS BIT);
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spSalesGetProductByBarcode] @Barcode NVARCHAR(100), @LocationId INT AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (1) p.ProductId, p.ProductCode, p.ProductName, p.Barcode, p.UnitId, u.UnitSymbol, p.CostPrice, p.SellingPrice, p.WholesalePrice, p.WholesaleMinQty, p.TaxRate, p.DiscountAllowed, p.IsStockTracked, p.IsActive, p.Status, ISNULL(s.CurrentStock,0) CurrentStock
    FROM dbo.Product p
    JOIN dbo.ProductUnit u ON u.UnitId = p.UnitId
    LEFT JOIN dbo.InventoryStock s ON s.ProductId = p.ProductId AND s.LocationId = @LocationId
    WHERE p.Barcode = @Barcode AND p.IsActive = 1 AND p.Status = N'Active';
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spSalesSearchProducts] @SearchText NVARCHAR(200) = NULL, @LocationId INT, @Top INT = 20 AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@Top) p.ProductId, p.ProductCode, p.ProductName, p.Barcode, p.UnitId, u.UnitSymbol, p.CostPrice, p.SellingPrice, p.WholesalePrice, p.WholesaleMinQty, p.TaxRate, p.DiscountAllowed, p.IsStockTracked, p.IsActive, p.Status, ISNULL(s.CurrentStock,0) CurrentStock
    FROM dbo.Product p
    JOIN dbo.ProductUnit u ON u.UnitId = p.UnitId
    LEFT JOIN dbo.InventoryStock s ON s.ProductId = p.ProductId AND s.LocationId = @LocationId
    WHERE p.IsActive = 1 AND p.Status = N'Active'
      AND (@SearchText IS NULL OR p.ProductCode LIKE N'%' + @SearchText + N'%' OR p.ProductName LIKE N'%' + @SearchText + N'%' OR p.Barcode LIKE N'%' + @SearchText + N'%')
    ORDER BY p.ProductName;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spSalesCompleteTransaction]
    @CustomerId INT = NULL,
    @CashierUserId INT,
    @HeldSaleHeaderId BIGINT = NULL,
    @UseCustomerCredit BIT = 0,
    @CustomerCreditAmount DECIMAL(18,2) = 0,
    @OrderDiscountAmount DECIMAL(18,4) = 0,
    @TaxAmount DECIMAL(18,4) = 0,
    @Remark NVARCHAR(1000) = N'',
    @AllowNegativeStock BIT = 0,
    @CreatedByUserId INT,
    @ItemsJson NVARCHAR(MAX),
    @PaymentsJson NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @Items TABLE (ProductId INT, LocationId INT, Quantity DECIMAL(18,4), UnitPrice DECIMAL(18,4), ItemDiscountAmount DECIMAL(18,4), TaxAmount DECIMAL(18,4));
    DECLARE @Payments TABLE (PaymentMethodId INT, PaymentAmount DECIMAL(18,4), ReferenceNo NVARCHAR(100));
    INSERT INTO @Items SELECT productId, locationId, quantity, unitPrice, itemDiscountAmount, taxAmount FROM OPENJSON(@ItemsJson) WITH (productId INT '$.productId', locationId INT '$.locationId', quantity DECIMAL(18,4) '$.quantity', unitPrice DECIMAL(18,4) '$.unitPrice', itemDiscountAmount DECIMAL(18,4) '$.itemDiscountAmount', taxAmount DECIMAL(18,4) '$.taxAmount');
    INSERT INTO @Payments SELECT paymentMethodId, paymentAmount, referenceNo FROM OPENJSON(@PaymentsJson) WITH (paymentMethodId INT '$.paymentMethodId', paymentAmount DECIMAL(18,4) '$.paymentAmount', referenceNo NVARCHAR(100) '$.referenceNo');

    IF NOT EXISTS (SELECT 1 FROM @Items) THROW 52100, 'Sale must have at least one item.', 1;
    IF EXISTS (SELECT 1 FROM @Items WHERE Quantity <= 0 OR ItemDiscountAmount < 0) THROW 52101, 'Invalid sale item amount.', 1;
    IF EXISTS (SELECT 1 FROM @Payments WHERE PaymentAmount <= 0) THROW 52102, 'Invalid payment amount.', 1;
    IF EXISTS (SELECT 1 FROM @Items i JOIN dbo.Product p ON p.ProductId=i.ProductId WHERE p.IsActive = 0 OR p.Status <> N'Active') THROW 52103, 'Product is not active.', 1;
    IF EXISTS (SELECT 1 FROM @Items i WHERE NOT EXISTS (SELECT 1 FROM dbo.Product p WHERE p.ProductId=i.ProductId)) THROW 52104, 'Product does not exist.', 1;
    IF EXISTS (SELECT 1 FROM @Payments p JOIN dbo.PaymentMethod pm ON pm.PaymentMethodId=p.PaymentMethodId WHERE pm.RequireReferenceNo=1 AND NULLIF(LTRIM(RTRIM(ISNULL(p.ReferenceNo,N''))),N'') IS NULL) THROW 52105, 'Payment reference number is required.', 1;
    UPDATE i
    SET UnitPrice = CASE WHEN i.UnitPrice > 0 THEN i.UnitPrice ELSE p.SellingPrice END,
        TaxAmount = CASE WHEN p.TaxRate <= 0 THEN 0 ELSE ROUND(((i.Quantity * CASE WHEN i.UnitPrice > 0 THEN i.UnitPrice ELSE p.SellingPrice END) - i.ItemDiscountAmount) * (p.TaxRate / (100.0 + p.TaxRate)), 4) END
    FROM @Items i
    JOIN dbo.Product p ON p.ProductId = i.ProductId;

    IF EXISTS (SELECT 1 FROM @Items i JOIN dbo.Product p ON p.ProductId=i.ProductId WHERE i.ItemDiscountAmount > 0 AND p.DiscountAllowed = 0) THROW 52109, 'Discount is not allowed for one or more products.', 1;
    IF EXISTS (SELECT 1 FROM @Items WHERE ItemDiscountAmount > Quantity * UnitPrice) THROW 52110, 'Item discount cannot exceed line subtotal.', 1;

    DECLARE @Subtotal DECIMAL(18,4) = (SELECT SUM(Quantity * UnitPrice) FROM @Items);
    DECLARE @ItemDiscount DECIMAL(18,4) = (SELECT SUM(ItemDiscountAmount) FROM @Items);
    DECLARE @LineTax DECIMAL(18,4) = (SELECT SUM(TaxAmount) FROM @Items);
    DECLARE @TotalTax DECIMAL(18,4) = @LineTax + ISNULL(@TaxAmount,0);
    DECLARE @TotalDiscount DECIMAL(18,4) = @ItemDiscount + ISNULL(@OrderDiscountAmount,0);
    DECLARE @Net DECIMAL(18,4) = @Subtotal - @TotalDiscount + ISNULL(@TaxAmount,0);
    IF @UseCustomerCredit = 1
    BEGIN
        IF @CustomerId IS NULL THROW 52130, 'Customer credit payment requires a selected customer.', 1;
        IF @CustomerCreditAmount IS NULL OR @CustomerCreditAmount <= 0 SET @CustomerCreditAmount = CONVERT(DECIMAL(18,2), @Net);
        IF @CustomerCreditAmount <= 0 THROW 52131, 'Customer credit amount must be greater than zero.', 1;
        DECLARE @CustomerCreditPaymentMethodId INT = (SELECT TOP 1 PaymentMethodId FROM dbo.PaymentMethod WHERE PaymentMethodCode = N'CREDIT' AND IsActive = 1);
        IF @CustomerCreditPaymentMethodId IS NULL THROW 52132, 'Customer credit payment method is not configured.', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.Customer WHERE CustomerId = @CustomerId AND IsActive = 1) THROW 52133, 'Customer does not exist or is inactive.', 1;
        INSERT INTO @Payments (PaymentMethodId, PaymentAmount, ReferenceNo)
        VALUES (@CustomerCreditPaymentMethodId, @CustomerCreditAmount, NULL);
    END
    DECLARE @Paid DECIMAL(18,4) = (SELECT SUM(PaymentAmount) FROM @Payments);
    DECLARE @CashPaid DECIMAL(18,4) = (SELECT ISNULL(SUM(p.PaymentAmount),0) FROM @Payments p JOIN dbo.PaymentMethod pm ON pm.PaymentMethodId=p.PaymentMethodId WHERE pm.IsCash=1);
    DECLARE @Change DECIMAL(18,4) = CASE WHEN @Paid > @Net AND @CashPaid > 0 THEN @Paid - @Net ELSE 0 END;
    IF @Net < 0 THROW 52106, 'Net amount cannot be negative.', 1;
    IF @Paid < @Net THROW 52107, 'Total payment amount must be greater than or equal to net amount.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;
        IF EXISTS (SELECT 1 FROM @Items i JOIN dbo.Product p ON p.ProductId=i.ProductId AND p.IsStockTracked=1 LEFT JOIN dbo.InventoryStock s WITH (UPDLOCK,HOLDLOCK) ON s.ProductId=i.ProductId AND s.LocationId=i.LocationId WHERE ISNULL(s.CurrentStock,0) < i.Quantity) AND @AllowNegativeStock=0 THROW 52108, 'Insufficient stock.', 1;

        DECLARE @SalesHeaderId BIGINT, @SaleNo NVARCHAR(50) = CONCAT(N'SAL', FORMAT(SYSUTCDATETIME(),'yyyyMMddHHmmssfff'));
        INSERT INTO dbo.SalesHeader (SaleNo, CustomerId, CashierUserId, SubtotalAmount, ItemDiscountAmount, OrderDiscountAmount, TotalDiscountAmount, TaxAmount, NetAmount, PaidAmount, ChangeAmount, Status, Remark, CreatedByUserId)
        VALUES (@SaleNo, @CustomerId, @CashierUserId, @Subtotal, @ItemDiscount, ISNULL(@OrderDiscountAmount,0), @TotalDiscount, @TotalTax, @Net, @Paid, @Change, N'Completed', ISNULL(@Remark,N''), NULLIF(@CreatedByUserId,0));
        SET @SalesHeaderId = SCOPE_IDENTITY();

        IF @UseCustomerCredit = 1
        BEGIN
            DECLARE @CreditLimit DECIMAL(18,2), @UsedCreditBefore DECIMAL(18,2), @AvailableCredit DECIMAL(18,2), @CreditTermDays INT;
            SELECT @CreditLimit = CreditLimit, @UsedCreditBefore = CurrentOutstandingAmount, @AvailableCredit = AvailableCredit, @CreditTermDays = CreditTermDays
            FROM dbo.CustomerCredit WITH (UPDLOCK, HOLDLOCK)
            WHERE CustomerId = @CustomerId AND AllowCredit = 1 AND CreditStatus = N'Good';

            IF @CreditLimit IS NULL THROW 52134, 'Customer credit is not allowed.', 1;
            IF @AvailableCredit < @CustomerCreditAmount THROW 52135, 'Insufficient available customer credit.', 1;

            UPDATE dbo.CustomerCredit
            SET CurrentOutstandingAmount = CurrentOutstandingAmount + @CustomerCreditAmount,
                UpdatedDate = SYSUTCDATETIME(),
                UpdatedByUserId = @CreatedByUserId
            WHERE CustomerId = @CustomerId;

            INSERT dbo.CustomerCreditTransaction
            (
                CustomerId, SaleId, TransactionType, ReferenceType, ReferenceId, ReferenceNo, Amount,
                BalanceBefore, BalanceAfter, DueDate, Status, Remark, Description, CreatedByUserId
            )
            VALUES
            (
                @CustomerId, @SalesHeaderId, N'CreditUsed', N'Sale', @SalesHeaderId, @SaleNo, @CustomerCreditAmount,
                @UsedCreditBefore, @UsedCreditBefore + @CustomerCreditAmount, DATEADD(DAY, ISNULL(@CreditTermDays,0), CONVERT(date, SYSUTCDATETIME())),
                N'Unpaid', N'POS customer credit sale', N'Customer credit used for sale.', @CreatedByUserId
            );
        END

        INSERT INTO dbo.SalesItem (SalesHeaderId, ProductId, ProductCodeSnapshot, ProductNameSnapshot, BarcodeSnapshot, UnitId, UnitSymbolSnapshot, Quantity, UnitPrice, CostPriceSnapshot, ItemDiscountAmount, TaxAmount, LineSubtotal, LineTotal)
        SELECT @SalesHeaderId, p.ProductId, p.ProductCode, p.ProductName, p.Barcode, p.UnitId, u.UnitSymbol, i.Quantity, i.UnitPrice, p.CostPrice, i.ItemDiscountAmount, i.TaxAmount, i.Quantity*i.UnitPrice, i.Quantity*i.UnitPrice-i.ItemDiscountAmount
        FROM @Items i JOIN dbo.Product p ON p.ProductId=i.ProductId JOIN dbo.ProductUnit u ON u.UnitId=p.UnitId;

        INSERT INTO dbo.SalesPayment (SalesHeaderId, PaymentMethodId, PaymentAmount, ReferenceNo, CreatedByUserId)
        SELECT @SalesHeaderId, PaymentMethodId, PaymentAmount, NULLIF(LTRIM(RTRIM(ReferenceNo)),N''), @CreatedByUserId FROM @Payments;

        DECLARE @ProductId INT, @LocationId INT, @Qty DECIMAL(18,4), @Cost DECIMAL(18,4);
        DECLARE sale_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT i.ProductId, i.LocationId, i.Quantity, p.CostPrice FROM @Items i JOIN dbo.Product p ON p.ProductId=i.ProductId WHERE p.IsStockTracked=1;
        OPEN sale_cursor; FETCH NEXT FROM sale_cursor INTO @ProductId, @LocationId, @Qty, @Cost;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            EXEC dbo.spInventoryMovementCreate @ProductId=@ProductId, @LocationId=@LocationId, @MovementType=N'Sale', @Quantity=@Qty, @UnitCost=@Cost, @ReferenceType=N'Sale', @ReferenceId=@SalesHeaderId, @ReferenceNo=@SaleNo, @Reason=N'Sale completed', @AllowNegativeStock=@AllowNegativeStock, @CreatedByUserId=@CreatedByUserId;
            FETCH NEXT FROM sale_cursor INTO @ProductId, @LocationId, @Qty, @Cost;
        END
        CLOSE sale_cursor; DEALLOCATE sale_cursor;

        IF @HeldSaleHeaderId IS NOT NULL UPDATE dbo.HeldSaleHeader SET Status=N'Completed', UpdatedByUserId=@CreatedByUserId, UpdatedDate=SYSUTCDATETIME() WHERE HeldSaleHeaderId=@HeldSaleHeaderId AND Status IN (N'Held',N'Resumed');
        COMMIT TRANSACTION;
        SELECT @SalesHeaderId SalesHeaderId, @SaleNo SaleNo, @Net NetAmount, @Paid PaidAmount, @Change ChangeAmount;
    END TRY
    BEGIN CATCH
        IF CURSOR_STATUS('local','sale_cursor') >= -1 BEGIN CLOSE sale_cursor; DEALLOCATE sale_cursor; END
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spSalesGetById] @SalesHeaderId BIGINT AS
BEGIN
    SET NOCOUNT ON;
    SELECT h.*, c.CustomerName, u.DisplayName CashierName FROM dbo.SalesHeader h LEFT JOIN dbo.Customer c ON c.CustomerId=h.CustomerId JOIN dbo.AccessUser u ON u.UserId=h.CashierUserId WHERE h.SalesHeaderId=@SalesHeaderId;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spSalesGetBySaleNo] @SaleNo NVARCHAR(50) AS
BEGIN
    SET NOCOUNT ON;
    SELECT h.*, c.CustomerName, u.DisplayName CashierName FROM dbo.SalesHeader h LEFT JOIN dbo.Customer c ON c.CustomerId=h.CustomerId JOIN dbo.AccessUser u ON u.UserId=h.CashierUserId WHERE h.SaleNo=@SaleNo;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spSalesGetItemsBySalesHeaderId] @SalesHeaderId BIGINT AS BEGIN SET NOCOUNT ON; SELECT * FROM dbo.SalesItem WHERE SalesHeaderId=@SalesHeaderId ORDER BY SalesItemId; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spSalesGetPaymentsBySalesHeaderId] @SalesHeaderId BIGINT AS BEGIN SET NOCOUNT ON; SELECT p.*, pm.PaymentMethodName FROM dbo.SalesPayment p JOIN dbo.PaymentMethod pm ON pm.PaymentMethodId=p.PaymentMethodId WHERE p.SalesHeaderId=@SalesHeaderId ORDER BY p.SalesPaymentId; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spSalesGetDetail] @SalesHeaderId BIGINT AS BEGIN SET NOCOUNT ON; EXEC dbo.spSalesGetById @SalesHeaderId; EXEC dbo.spSalesGetItemsBySalesHeaderId @SalesHeaderId; EXEC dbo.spSalesGetPaymentsBySalesHeaderId @SalesHeaderId; END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spSalesGetPaged]
    @PageNumber INT=1, @PageSize INT=20, @SearchText NVARCHAR(200)=NULL, @CustomerId INT=NULL, @CashierUserId INT=NULL, @Status NVARCHAR(30)=NULL, @FromDate DATETIME2(0)=NULL, @ToDate DATETIME2(0)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH q AS (
        SELECT h.*, c.CustomerName, u.DisplayName CashierName, COUNT(1) OVER() TotalCount
        FROM dbo.SalesHeader h LEFT JOIN dbo.Customer c ON c.CustomerId=h.CustomerId JOIN dbo.AccessUser u ON u.UserId=h.CashierUserId
        WHERE (@SearchText IS NULL OR h.SaleNo LIKE N'%' + @SearchText + N'%' OR c.CustomerName LIKE N'%' + @SearchText + N'%')
          AND (@CustomerId IS NULL OR h.CustomerId=@CustomerId) AND (@CashierUserId IS NULL OR h.CashierUserId=@CashierUserId)
          AND (@Status IS NULL OR h.Status=@Status) AND (@FromDate IS NULL OR h.SaleDate>=@FromDate) AND (@ToDate IS NULL OR h.SaleDate<DATEADD(DAY,1,@ToDate))
    )
    SELECT * FROM q ORDER BY SaleDate DESC, SalesHeaderId DESC OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spSalesGetSummaryByDateRange] @FromDate DATETIME2(0), @ToDate DATETIME2(0), @CashierUserId INT=NULL AS
BEGIN
    SET NOCOUNT ON;
    ;WITH SaleScope AS
    (
        SELECT *
        FROM dbo.SalesHeader
        WHERE Status <> N'Voided'
          AND SaleDate >= @FromDate
          AND SaleDate < DATEADD(DAY, 1, @ToDate)
          AND (@CashierUserId IS NULL OR CashierUserId = @CashierUserId)
    ),
    HeaderSummary AS
    (
        SELECT
            CAST(SaleDate AS DATE) AS SaleDate,
            COUNT(1) AS TransactionCount,
            SUM(SubtotalAmount) AS GrossAmount,
            SUM(TotalDiscountAmount) AS DiscountAmount,
            SUM(TaxAmount) AS TaxAmount,
            SUM(NetAmount) AS NetAmount,
            SUM(OrderDiscountAmount) AS OrderDiscountAmount
        FROM SaleScope
        GROUP BY CAST(SaleDate AS DATE)
    ),
    ItemSummary AS
    (
        SELECT
            CAST(s.SaleDate AS DATE) AS SaleDate,
            SUM(si.Quantity * si.CostPriceSnapshot) AS CostOfGoodsSold,
            SUM(si.LineTotal - (si.Quantity * si.CostPriceSnapshot)) AS GrossProfitAmount,
            SUM(CASE
                WHEN ISNULL(p.VatMode, N'VatExcluded') = N'NoVat' THEN 0
                WHEN ISNULL(p.VatMode, N'VatExcluded') = N'VatIncluded' AND ISNULL(p.VatPercentage, 0) > 0 THEN si.Quantity * si.CostPriceSnapshot * (p.VatPercentage / (100.0 + p.VatPercentage))
                WHEN ISNULL(p.VatMode, N'VatExcluded') = N'VatIncluded' THEN 0
                ELSE si.Quantity * si.CostPriceSnapshot * (ISNULL(p.VatPercentage, 0) / 100.0)
            END) AS VatInAmount
        FROM SaleScope s
        JOIN dbo.SalesItem si ON si.SalesHeaderId = s.SalesHeaderId
        JOIN dbo.Product p ON p.ProductId = si.ProductId
        GROUP BY CAST(s.SaleDate AS DATE)
    ),
    RefundSummary AS
    (
        SELECT
            CAST(r.ReturnDate AS DATE) AS SaleDate,
            SUM(r.RefundNetAmount) AS RefundAmount
        FROM dbo.SalesReturnHeader r
        WHERE r.Status = N'Completed'
          AND r.ReturnDate >= @FromDate
          AND r.ReturnDate < DATEADD(DAY, 1, @ToDate)
          AND (@CashierUserId IS NULL OR r.CashierUserId = @CashierUserId)
        GROUP BY CAST(r.ReturnDate AS DATE)
    )
    SELECT
        h.SaleDate,
        h.TransactionCount,
        h.GrossAmount,
        h.DiscountAmount,
        h.TaxAmount,
        h.NetAmount,
        ISNULL(r.RefundAmount, 0) AS RefundAmount,
        ISNULL(i.CostOfGoodsSold, 0) AS CostOfGoodsSold,
        ISNULL(i.GrossProfitAmount, 0) - h.OrderDiscountAmount AS GrossProfitAmount,
        ISNULL(i.VatInAmount, 0) AS VatInAmount,
        h.TaxAmount AS VatOutAmount
    FROM HeaderSummary h
    LEFT JOIN ItemSummary i ON i.SaleDate = h.SaleDate
    LEFT JOIN RefundSummary r ON r.SaleDate = h.SaleDate
    ORDER BY h.SaleDate;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spSalesVatBillReportGet]
    @FromDate DATETIME2(0),
    @ToDate DATETIME2(0),
    @CashierUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH SaleScope AS
    (
        SELECT h.*, c.CustomerName, u.DisplayName AS CashierName
        FROM dbo.SalesHeader h
        LEFT JOIN dbo.Customer c ON c.CustomerId = h.CustomerId
        JOIN dbo.AccessUser u ON u.UserId = h.CashierUserId
        WHERE h.Status <> N'Voided'
          AND h.SaleDate >= @FromDate
          AND h.SaleDate < DATEADD(DAY, 1, @ToDate)
          AND (@CashierUserId IS NULL OR h.CashierUserId = @CashierUserId)
    ),
    ItemVat AS
    (
        SELECT
            s.SalesHeaderId,
            SUM(CASE
                WHEN ISNULL(p.VatMode, N'VatExcluded') = N'NoVat' THEN 0
                WHEN ISNULL(p.VatMode, N'VatExcluded') = N'VatIncluded' AND ISNULL(p.VatPercentage, 0) > 0 THEN si.Quantity * si.CostPriceSnapshot * (p.VatPercentage / (100.0 + p.VatPercentage))
                WHEN ISNULL(p.VatMode, N'VatExcluded') = N'VatIncluded' THEN 0
                ELSE si.Quantity * si.CostPriceSnapshot * (ISNULL(p.VatPercentage, 0) / 100.0)
            END) AS VatInAmount
        FROM SaleScope s
        JOIN dbo.SalesItem si ON si.SalesHeaderId = s.SalesHeaderId
        JOIN dbo.Product p ON p.ProductId = si.ProductId
        GROUP BY s.SalesHeaderId
    )
    SELECT
        s.SalesHeaderId,
        s.SaleNo,
        s.SaleDate,
        ISNULL(s.CustomerName, N'Walk-in') AS CustomerName,
        CAST(N'' AS NVARCHAR(50)) AS CustomerTaxId,
        s.CashierName,
        s.SubtotalAmount AS GrossAmount,
        s.TotalDiscountAmount AS DiscountAmount,
        CASE WHEN s.NetAmount >= s.TaxAmount THEN s.NetAmount - s.TaxAmount ELSE 0 END AS TaxableAmount,
        s.TaxAmount AS VatOutAmount,
        ISNULL(i.VatInAmount, 0) AS VatInAmount,
        s.TaxAmount - ISNULL(i.VatInAmount, 0) AS VatPayableAmount,
        s.NetAmount,
        s.Status
    FROM SaleScope s
    LEFT JOIN ItemVat i ON i.SalesHeaderId = s.SalesHeaderId
    ORDER BY s.SaleDate, s.SalesHeaderId;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spSalesVoid] @SalesHeaderId BIGINT, @Reason NVARCHAR(500), @UpdatedByUserId INT, @ReverseInventory BIT=1 AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF NULLIF(LTRIM(RTRIM(@Reason)),N'') IS NULL THROW 52120, 'Void reason is required.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.SalesHeader WHERE SalesHeaderId=@SalesHeaderId AND Status=N'Completed') THROW 52121, 'Only completed sales can be voided.', 1;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @SaleNo NVARCHAR(50) = (SELECT SaleNo FROM dbo.SalesHeader WHERE SalesHeaderId=@SalesHeaderId);
        UPDATE dbo.SalesHeader SET Status=N'Voided', Remark=CONCAT(Remark, CASE WHEN Remark=N'' THEN N'' ELSE N' | ' END, N'Voided: ', @Reason), UpdatedByUserId=@UpdatedByUserId, UpdatedDate=SYSUTCDATETIME() WHERE SalesHeaderId=@SalesHeaderId;
        IF @ReverseInventory=1
        BEGIN
            DECLARE @ProductId INT, @LocationId INT, @Qty DECIMAL(18,4), @Cost DECIMAL(18,4);
            DECLARE v CURSOR LOCAL FAST_FORWARD FOR SELECT m.ProductId, m.LocationId, m.Quantity, m.UnitCost FROM dbo.InventoryMovement m WHERE m.ReferenceType=N'Sale' AND m.ReferenceId=@SalesHeaderId AND m.MovementType=N'Sale';
            OPEN v; FETCH NEXT FROM v INTO @ProductId,@LocationId,@Qty,@Cost;
            WHILE @@FETCH_STATUS=0 BEGIN EXEC dbo.spInventoryMovementCreate @ProductId=@ProductId,@LocationId=@LocationId,@MovementType=N'Return',@Quantity=@Qty,@UnitCost=@Cost,@ReferenceType=N'SaleVoid',@ReferenceId=@SalesHeaderId,@ReferenceNo=@SaleNo,@Reason=@Reason,@AllowNegativeStock=1,@CreatedByUserId=@UpdatedByUserId; FETCH NEXT FROM v INTO @ProductId,@LocationId,@Qty,@Cost; END
            CLOSE v; DEALLOCATE v;
        END
        COMMIT TRANSACTION;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spReceiptPrintHistoryCreate] @SalesHeaderId BIGINT=NULL, @SalesReturnHeaderId BIGINT=NULL, @ReceiptNo NVARCHAR(50), @ReceiptType NVARCHAR(20), @PrintedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.ReceiptPrintHistory (SalesHeaderId, SalesReturnHeaderId, ReceiptNo, ReceiptType, PrintedByUserId) VALUES (@SalesHeaderId, @SalesReturnHeaderId, @ReceiptNo, @ReceiptType, @PrintedByUserId);
    SELECT CONVERT(BIGINT, SCOPE_IDENTITY());
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spSalesReprintReceipt] @SalesHeaderId BIGINT, @PrintedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ReceiptNo NVARCHAR(50)=(SELECT SaleNo FROM dbo.SalesHeader WHERE SalesHeaderId=@SalesHeaderId);
    EXEC dbo.spReceiptPrintHistoryCreate @SalesHeaderId=@SalesHeaderId, @ReceiptNo=@ReceiptNo, @ReceiptType=N'Sale', @PrintedByUserId=@PrintedByUserId;
    SELECT TOP (1) * FROM dbo.ReceiptPrintHistory WHERE SalesHeaderId=@SalesHeaderId ORDER BY ReceiptPrintHistoryId DESC;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spHeldSaleGetPaged] @PageNumber INT=1,@PageSize INT=20,@SearchText NVARCHAR(200)=NULL,@CashierUserId INT=NULL,@Status NVARCHAR(30)=NULL,@FromDate DATETIME2(0)=NULL,@ToDate DATETIME2(0)=NULL AS
BEGIN
    SET NOCOUNT ON;
    ;WITH q AS (SELECT h.*, c.CustomerName, u.DisplayName CashierName, COUNT(1) OVER() TotalCount FROM dbo.HeldSaleHeader h LEFT JOIN dbo.Customer c ON c.CustomerId=h.CustomerId JOIN dbo.AccessUser u ON u.UserId=h.CashierUserId WHERE (@SearchText IS NULL OR h.HeldSaleNo LIKE N'%'+@SearchText+N'%' OR c.CustomerName LIKE N'%'+@SearchText+N'%') AND (@CashierUserId IS NULL OR h.CashierUserId=@CashierUserId) AND (@Status IS NULL OR h.Status=@Status) AND (@FromDate IS NULL OR h.HeldDate>=@FromDate) AND (@ToDate IS NULL OR h.HeldDate<DATEADD(DAY,1,@ToDate)))
    SELECT * FROM q ORDER BY HeldDate DESC OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spHeldSaleGetById] @HeldSaleHeaderId BIGINT AS BEGIN SET NOCOUNT ON; SELECT h.*, c.CustomerName, u.DisplayName CashierName FROM dbo.HeldSaleHeader h LEFT JOIN dbo.Customer c ON c.CustomerId=h.CustomerId JOIN dbo.AccessUser u ON u.UserId=h.CashierUserId WHERE h.HeldSaleHeaderId=@HeldSaleHeaderId; SELECT * FROM dbo.HeldSaleItem WHERE HeldSaleHeaderId=@HeldSaleHeaderId ORDER BY HeldSaleItemId; END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spHeldSaleCreate] @CustomerId INT=NULL,@CashierUserId INT,@Note NVARCHAR(1000)=N'',@EstimatedTaxAmount DECIMAL(18,4)=0,@CreatedByUserId INT,@ItemsJson NVARCHAR(MAX) AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @Items TABLE (ProductId INT, LocationId INT, Quantity DECIMAL(18,4), UnitPrice DECIMAL(18,4), ItemDiscountAmount DECIMAL(18,4), TaxAmount DECIMAL(18,4));
    INSERT INTO @Items SELECT productId, locationId, quantity, unitPrice, itemDiscountAmount, taxAmount FROM OPENJSON(@ItemsJson) WITH (productId INT '$.productId', locationId INT '$.locationId', quantity DECIMAL(18,4) '$.quantity', unitPrice DECIMAL(18,4) '$.unitPrice', itemDiscountAmount DECIMAL(18,4) '$.itemDiscountAmount', taxAmount DECIMAL(18,4) '$.taxAmount');
    IF NOT EXISTS (SELECT 1 FROM @Items) THROW 52200, 'Held sale must have at least one item.', 1;
    IF EXISTS (SELECT 1 FROM @Items WHERE Quantity <= 0 OR ItemDiscountAmount < 0) THROW 52201, 'Invalid held sale item amount.', 1;
    IF EXISTS (SELECT 1 FROM @Items i WHERE NOT EXISTS (SELECT 1 FROM dbo.Product p WHERE p.ProductId=i.ProductId AND p.IsActive=1 AND p.Status=N'Active')) THROW 52202, 'Held sale product is not active.', 1;
    UPDATE i
    SET UnitPrice = CASE WHEN i.UnitPrice > 0 THEN i.UnitPrice ELSE p.SellingPrice END,
        TaxAmount = CASE WHEN p.TaxRate <= 0 THEN 0 ELSE ROUND(((i.Quantity * CASE WHEN i.UnitPrice > 0 THEN i.UnitPrice ELSE p.SellingPrice END) - i.ItemDiscountAmount) * (p.TaxRate / (100.0 + p.TaxRate)), 4) END
    FROM @Items i
    JOIN dbo.Product p ON p.ProductId = i.ProductId;
    IF EXISTS (SELECT 1 FROM @Items i JOIN dbo.Product p ON p.ProductId=i.ProductId WHERE i.ItemDiscountAmount > 0 AND p.DiscountAllowed = 0) THROW 52203, 'Discount is not allowed for one or more products.', 1;
    IF EXISTS (SELECT 1 FROM @Items WHERE ItemDiscountAmount > Quantity * UnitPrice) THROW 52204, 'Item discount cannot exceed line subtotal.', 1;
    DECLARE @Subtotal DECIMAL(18,4)=(SELECT SUM(Quantity*UnitPrice) FROM @Items), @Discount DECIMAL(18,4)=(SELECT SUM(ItemDiscountAmount) FROM @Items), @LineTax DECIMAL(18,4)=(SELECT SUM(TaxAmount) FROM @Items);
    DECLARE @HeldSaleNo NVARCHAR(50)=CONCAT(N'HELD',FORMAT(SYSUTCDATETIME(),'yyyyMMddHHmmssfff')), @Id BIGINT;
    BEGIN TRY
        BEGIN TRANSACTION;
        INSERT dbo.HeldSaleHeader (HeldSaleNo, CustomerId, CashierUserId, Note, EstimatedSubtotalAmount, EstimatedDiscountAmount, EstimatedTaxAmount, EstimatedNetAmount, Status, CreatedByUserId)
        VALUES (@HeldSaleNo, @CustomerId, @CashierUserId, ISNULL(@Note,N''), @Subtotal, @Discount, @LineTax+ISNULL(@EstimatedTaxAmount,0), @Subtotal-@Discount+ISNULL(@EstimatedTaxAmount,0), N'Held', @CreatedByUserId);
        SET @Id=SCOPE_IDENTITY();
        INSERT dbo.HeldSaleItem (HeldSaleHeaderId, ProductId, ProductCodeSnapshot, ProductNameSnapshot, BarcodeSnapshot, UnitId, UnitSymbolSnapshot, Quantity, UnitPrice, CostPriceSnapshot, ItemDiscountAmount, TaxAmount, LineSubtotal, LineTotal)
        SELECT @Id,p.ProductId,p.ProductCode,p.ProductName,p.Barcode,p.UnitId,u.UnitSymbol,i.Quantity,i.UnitPrice,p.CostPrice,i.ItemDiscountAmount,i.TaxAmount,i.Quantity*i.UnitPrice,i.Quantity*i.UnitPrice-i.ItemDiscountAmount FROM @Items i JOIN dbo.Product p ON p.ProductId=i.ProductId JOIN dbo.ProductUnit u ON u.UnitId=p.UnitId;
        COMMIT; SELECT @Id;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spHeldSaleCancel] @HeldSaleHeaderId BIGINT,@Reason NVARCHAR(500),@UpdatedByUserId INT AS BEGIN SET NOCOUNT ON; UPDATE dbo.HeldSaleHeader SET Status=N'Cancelled', Note=CONCAT(Note, CASE WHEN Note=N'' THEN N'' ELSE N' | ' END, N'Cancelled: ', @Reason), UpdatedByUserId=@UpdatedByUserId, UpdatedDate=SYSUTCDATETIME() WHERE HeldSaleHeaderId=@HeldSaleHeaderId AND Status IN (N'Held',N'Resumed'); END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spHeldSaleResume] @HeldSaleHeaderId BIGINT,@UpdatedByUserId INT AS BEGIN SET NOCOUNT ON; UPDATE dbo.HeldSaleHeader SET Status=N'Resumed', UpdatedByUserId=@UpdatedByUserId, UpdatedDate=SYSUTCDATETIME() WHERE HeldSaleHeaderId=@HeldSaleHeaderId AND Status=N'Held'; EXEC dbo.spHeldSaleGetById @HeldSaleHeaderId; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spHeldSaleExpireOld] @ExpireBeforeDate DATETIME2(0),@UpdatedByUserId INT AS BEGIN SET NOCOUNT ON; UPDATE dbo.HeldSaleHeader SET Status=N'Expired', UpdatedByUserId=@UpdatedByUserId, UpdatedDate=SYSUTCDATETIME() WHERE Status IN (N'Held',N'Resumed') AND HeldDate<@ExpireBeforeDate; SELECT @@ROWCOUNT; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spHeldSaleComplete] @HeldSaleHeaderId BIGINT AS BEGIN SET NOCOUNT ON; UPDATE dbo.HeldSaleHeader SET Status=N'Completed', UpdatedDate=SYSUTCDATETIME() WHERE HeldSaleHeaderId=@HeldSaleHeaderId; END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spSalesReturnGetPaged] @PageNumber INT=1,@PageSize INT=20,@SearchText NVARCHAR(200)=NULL,@SalesHeaderId BIGINT=NULL,@CustomerId INT=NULL,@CashierUserId INT=NULL,@Status NVARCHAR(30)=NULL,@FromDate DATETIME2(0)=NULL,@ToDate DATETIME2(0)=NULL AS
BEGIN
    SET NOCOUNT ON;
    ;WITH q AS (SELECT r.*, h.SaleNo, COUNT(1) OVER() TotalCount FROM dbo.SalesReturnHeader r JOIN dbo.SalesHeader h ON h.SalesHeaderId=r.SalesHeaderId WHERE (@SearchText IS NULL OR r.ReturnNo LIKE N'%'+@SearchText+N'%' OR h.SaleNo LIKE N'%'+@SearchText+N'%') AND (@SalesHeaderId IS NULL OR r.SalesHeaderId=@SalesHeaderId) AND (@CustomerId IS NULL OR r.CustomerId=@CustomerId) AND (@CashierUserId IS NULL OR r.CashierUserId=@CashierUserId) AND (@Status IS NULL OR r.Status=@Status) AND (@FromDate IS NULL OR r.ReturnDate>=@FromDate) AND (@ToDate IS NULL OR r.ReturnDate<DATEADD(DAY,1,@ToDate)))
    SELECT * FROM q ORDER BY ReturnDate DESC OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spSalesReturnGetItemsByReturnId] @SalesReturnHeaderId BIGINT AS BEGIN SET NOCOUNT ON; SELECT * FROM dbo.SalesReturnItem WHERE SalesReturnHeaderId=@SalesReturnHeaderId ORDER BY SalesReturnItemId; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spSalesReturnGetPaymentsByReturnId] @SalesReturnHeaderId BIGINT AS BEGIN SET NOCOUNT ON; SELECT p.*, pm.PaymentMethodName FROM dbo.SalesReturnPayment p JOIN dbo.PaymentMethod pm ON pm.PaymentMethodId=p.PaymentMethodId WHERE p.SalesReturnHeaderId=@SalesReturnHeaderId ORDER BY SalesReturnPaymentId; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spSalesReturnGetById] @SalesReturnHeaderId BIGINT AS BEGIN SET NOCOUNT ON; SELECT r.*, h.SaleNo FROM dbo.SalesReturnHeader r JOIN dbo.SalesHeader h ON h.SalesHeaderId=r.SalesHeaderId WHERE r.SalesReturnHeaderId=@SalesReturnHeaderId; EXEC dbo.spSalesReturnGetItemsByReturnId @SalesReturnHeaderId; EXEC dbo.spSalesReturnGetPaymentsByReturnId @SalesReturnHeaderId; END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spSalesReturnCreate] @SalesHeaderId BIGINT,@CashierUserId INT,@Reason NVARCHAR(500),@CreatedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.SalesHeader WHERE SalesHeaderId=@SalesHeaderId AND Status IN (N'Completed',N'PartiallyRefunded')) THROW 52300, 'Refund must reference an existing completed sale.', 1;
    DECLARE @ReturnNo NVARCHAR(50)=CONCAT(N'RET',FORMAT(SYSUTCDATETIME(),'yyyyMMddHHmmssfff'));
    INSERT dbo.SalesReturnHeader (ReturnNo, SalesHeaderId, CustomerId, CashierUserId, Reason, Status, CreatedByUserId) SELECT @ReturnNo, SalesHeaderId, CustomerId, @CashierUserId, @Reason, N'Draft', @CreatedByUserId FROM dbo.SalesHeader WHERE SalesHeaderId=@SalesHeaderId;
    SELECT CONVERT(BIGINT,SCOPE_IDENTITY());
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spSalesReturnAddItem] @SalesReturnHeaderId BIGINT,@SalesItemId BIGINT,@QuantityReturned DECIMAL(18,4),@RefundUnitPrice DECIMAL(18,4),@ReturnToStock BIT,@ReturnCondition NVARCHAR(30),@Reason NVARCHAR(500) AS
BEGIN
    SET NOCOUNT ON;
    IF @QuantityReturned <= 0 THROW 52301, 'Refund quantity must be greater than zero.', 1;
    IF @ReturnToStock=1 AND @ReturnCondition<>N'Good' THROW 52302, 'Return to stock is allowed only for Good condition.', 1;
    IF EXISTS (SELECT 1 FROM dbo.SalesReturnHeader WHERE SalesReturnHeaderId=@SalesReturnHeaderId AND Status<>N'Draft') THROW 52303, 'Only draft returns can be edited.', 1;
    DECLARE @Sold DECIMAL(18,4), @Returned DECIMAL(18,4);
    SELECT @Sold=Quantity, @Returned=ReturnedQty FROM dbo.SalesItem WHERE SalesItemId=@SalesItemId;
    IF @QuantityReturned > (@Sold - @Returned - ISNULL((SELECT SUM(QuantityReturned) FROM dbo.SalesReturnItem WHERE SalesItemId=@SalesItemId AND SalesReturnHeaderId<>@SalesReturnHeaderId),0)) THROW 52304, 'Refund quantity exceeds remaining refundable quantity.', 1;
    INSERT dbo.SalesReturnItem (SalesReturnHeaderId, SalesItemId, ProductId, ProductCodeSnapshot, ProductNameSnapshot, BarcodeSnapshot, UnitId, UnitSymbolSnapshot, QuantityReturned, RefundUnitPrice, RefundAmount, ReturnToStock, ReturnCondition, Reason)
    SELECT @SalesReturnHeaderId, SalesItemId, ProductId, ProductCodeSnapshot, ProductNameSnapshot, BarcodeSnapshot, UnitId, UnitSymbolSnapshot, @QuantityReturned, @RefundUnitPrice, @QuantityReturned*@RefundUnitPrice, @ReturnToStock, @ReturnCondition, @Reason FROM dbo.SalesItem WHERE SalesItemId=@SalesItemId;
    UPDATE dbo.SalesReturnHeader SET RefundSubtotalAmount=(SELECT SUM(RefundAmount) FROM dbo.SalesReturnItem WHERE SalesReturnHeaderId=@SalesReturnHeaderId), RefundNetAmount=(SELECT SUM(RefundAmount) FROM dbo.SalesReturnItem WHERE SalesReturnHeaderId=@SalesReturnHeaderId), UpdatedDate=SYSUTCDATETIME() WHERE SalesReturnHeaderId=@SalesReturnHeaderId;
    SELECT CONVERT(BIGINT,SCOPE_IDENTITY());
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spSalesReturnApprove] @SalesReturnHeaderId BIGINT,@ApprovedByUserId INT AS BEGIN SET NOCOUNT ON; UPDATE dbo.SalesReturnHeader SET Status=N'Approved', ApprovedByUserId=@ApprovedByUserId, ApprovedDate=SYSUTCDATETIME(), UpdatedByUserId=@ApprovedByUserId, UpdatedDate=SYSUTCDATETIME() WHERE SalesReturnHeaderId=@SalesReturnHeaderId AND Status=N'Draft'; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spSalesReturnReject] @SalesReturnHeaderId BIGINT,@Reason NVARCHAR(500),@UpdatedByUserId INT AS BEGIN SET NOCOUNT ON; UPDATE dbo.SalesReturnHeader SET Status=N'Rejected', Reason=CONCAT(Reason,N' | Rejected: ',@Reason), UpdatedByUserId=@UpdatedByUserId, UpdatedDate=SYSUTCDATETIME() WHERE SalesReturnHeaderId=@SalesReturnHeaderId AND Status IN (N'Draft',N'Approved'); END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spSalesReturnCancel] @SalesReturnHeaderId BIGINT,@Reason NVARCHAR(500),@UpdatedByUserId INT AS BEGIN SET NOCOUNT ON; UPDATE dbo.SalesReturnHeader SET Status=N'Cancelled', Reason=CONCAT(Reason,N' | Cancelled: ',@Reason), UpdatedByUserId=@UpdatedByUserId, UpdatedDate=SYSUTCDATETIME() WHERE SalesReturnHeaderId=@SalesReturnHeaderId AND Status IN (N'Draft',N'Approved'); END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spSalesReturnComplete] @SalesReturnHeaderId BIGINT,@CompletedByUserId INT,@PaymentsJson NVARCHAR(MAX) AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @Payments TABLE (PaymentMethodId INT, RefundAmount DECIMAL(18,4), ReferenceNo NVARCHAR(100));
    INSERT INTO @Payments SELECT paymentMethodId, refundAmount, referenceNo FROM OPENJSON(@PaymentsJson) WITH (paymentMethodId INT '$.paymentMethodId', refundAmount DECIMAL(18,4) '$.refundAmount', referenceNo NVARCHAR(100) '$.referenceNo');
    IF EXISTS (SELECT 1 FROM @Payments WHERE RefundAmount<=0) THROW 52310, 'Refund payment amount must be greater than zero.', 1;
    IF EXISTS (SELECT 1 FROM @Payments p JOIN dbo.PaymentMethod pm ON pm.PaymentMethodId=p.PaymentMethodId WHERE pm.RequireReferenceNo=1 AND NULLIF(LTRIM(RTRIM(ISNULL(p.ReferenceNo,N''))),N'') IS NULL) THROW 52311, 'Refund reference number is required.', 1;
    DECLARE @RefundNet DECIMAL(18,4)=(SELECT RefundNetAmount FROM dbo.SalesReturnHeader WHERE SalesReturnHeaderId=@SalesReturnHeaderId);
    DECLARE @OriginalSalesHeaderId BIGINT, @ReturnCustomerId INT, @ReturnNoForCredit NVARCHAR(50);
    SELECT @OriginalSalesHeaderId=SalesHeaderId, @ReturnCustomerId=CustomerId, @ReturnNoForCredit=ReturnNo FROM dbo.SalesReturnHeader WHERE SalesReturnHeaderId=@SalesReturnHeaderId;
    DECLARE @CreditRefundPaymentMethodId INT=(SELECT TOP 1 PaymentMethodId FROM dbo.PaymentMethod WHERE PaymentMethodCode=N'CREDIT' AND IsActive=1);
    DECLARE @OriginalCreditPaid DECIMAL(18,4)=
    (
        SELECT ISNULL(SUM(sp.PaymentAmount),0)
        FROM dbo.SalesPayment sp
        JOIN dbo.PaymentMethod pm ON pm.PaymentMethodId=sp.PaymentMethodId
        WHERE sp.SalesHeaderId=@OriginalSalesHeaderId AND pm.PaymentMethodCode=N'CREDIT'
    );
    DECLARE @CreditRefundAmount DECIMAL(18,4)=CASE WHEN @OriginalCreditPaid>0 AND @RefundNet>0 THEN IIF(@OriginalCreditPaid>@RefundNet,@RefundNet,@OriginalCreditPaid) ELSE 0 END;
    IF @CreditRefundAmount>0 AND @CreditRefundPaymentMethodId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM @Payments WHERE PaymentMethodId=@CreditRefundPaymentMethodId)
        INSERT INTO @Payments(PaymentMethodId,RefundAmount,ReferenceNo) VALUES(@CreditRefundPaymentMethodId,@CreditRefundAmount,NULL);
    IF (SELECT ISNULL(SUM(RefundAmount),0) FROM @Payments) < @RefundNet THROW 52312, 'Refund payment total must cover refund net amount.', 1;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.SalesReturnHeader WHERE SalesReturnHeaderId=@SalesReturnHeaderId AND Status IN (N'Draft',N'Approved')) THROW 52313, 'Return cannot be completed.', 1;
        INSERT dbo.SalesReturnPayment (SalesReturnHeaderId, PaymentMethodId, RefundAmount, ReferenceNo, CreatedByUserId) SELECT @SalesReturnHeaderId, PaymentMethodId, RefundAmount, NULLIF(LTRIM(RTRIM(ReferenceNo)),N''), @CompletedByUserId FROM @Payments;
        UPDATE si SET ReturnedQty=si.ReturnedQty+ri.QuantityReturned FROM dbo.SalesItem si JOIN dbo.SalesReturnItem ri ON ri.SalesItemId=si.SalesItemId WHERE ri.SalesReturnHeaderId=@SalesReturnHeaderId;
        DECLARE @ReturnNo NVARCHAR(50), @SalesHeaderId BIGINT; SELECT @ReturnNo=ReturnNo,@SalesHeaderId=SalesHeaderId FROM dbo.SalesReturnHeader WHERE SalesReturnHeaderId=@SalesReturnHeaderId;
        DECLARE @ProductId INT,@Qty DECIMAL(18,4),@Cost DECIMAL(18,4);
        DECLARE r CURSOR LOCAL FAST_FORWARD FOR SELECT ri.ProductId, ri.QuantityReturned, si.CostPriceSnapshot FROM dbo.SalesReturnItem ri JOIN dbo.SalesItem si ON si.SalesItemId=ri.SalesItemId JOIN dbo.Product p ON p.ProductId=ri.ProductId WHERE ri.SalesReturnHeaderId=@SalesReturnHeaderId AND ri.ReturnToStock=1 AND ri.ReturnCondition=N'Good' AND p.IsStockTracked=1;
        OPEN r; FETCH NEXT FROM r INTO @ProductId,@Qty,@Cost;
        WHILE @@FETCH_STATUS=0 BEGIN DECLARE @LocationId INT=(SELECT TOP 1 LocationId FROM dbo.InventoryMovement WHERE ReferenceType=N'Sale' AND ReferenceId=@SalesHeaderId AND ProductId=@ProductId ORDER BY InventoryMovementId); EXEC dbo.spInventoryMovementCreate @ProductId=@ProductId,@LocationId=@LocationId,@MovementType=N'Return',@Quantity=@Qty,@UnitCost=@Cost,@ReferenceType=N'SalesReturn',@ReferenceId=@SalesReturnHeaderId,@ReferenceNo=@ReturnNo,@Reason=N'Sales return completed',@AllowNegativeStock=1,@CreatedByUserId=@CompletedByUserId; FETCH NEXT FROM r INTO @ProductId,@Qty,@Cost; END
        CLOSE r; DEALLOCATE r;
        UPDATE dbo.SalesReturnHeader SET Status=N'Completed', UpdatedByUserId=@CompletedByUserId, UpdatedDate=SYSUTCDATETIME() WHERE SalesReturnHeaderId=@SalesReturnHeaderId;
        UPDATE h SET Status=CASE WHEN NOT EXISTS (SELECT 1 FROM dbo.SalesItem WHERE SalesHeaderId=h.SalesHeaderId AND ReturnedQty < Quantity) THEN N'Refunded' ELSE N'PartiallyRefunded' END, UpdatedByUserId=@CompletedByUserId, UpdatedDate=SYSUTCDATETIME() FROM dbo.SalesHeader h WHERE h.SalesHeaderId=@SalesHeaderId;
        IF @CreditRefundAmount>0 AND @ReturnCustomerId IS NOT NULL
        BEGIN
            DECLARE @CreditBefore DECIMAL(18,2);
            SELECT @CreditBefore=CurrentOutstandingAmount FROM dbo.CustomerCredit WITH(UPDLOCK,HOLDLOCK) WHERE CustomerId=@ReturnCustomerId;
            IF @CreditBefore IS NOT NULL
            BEGIN
                UPDATE dbo.CustomerCredit SET CurrentOutstandingAmount=CASE WHEN CurrentOutstandingAmount<@CreditRefundAmount THEN 0 ELSE CurrentOutstandingAmount-@CreditRefundAmount END,UpdatedDate=SYSUTCDATETIME(),UpdatedByUserId=@CompletedByUserId WHERE CustomerId=@ReturnCustomerId;
                INSERT dbo.CustomerCreditTransaction(CustomerId,TransactionType,ReferenceType,ReferenceId,ReferenceNo,Amount,BalanceBefore,BalanceAfter,Status,Remark,Description,CreatedByUserId)
                VALUES(@ReturnCustomerId,N'CreditRefunded',N'SalesReturn',@SalesReturnHeaderId,@ReturnNo,@CreditRefundAmount,@CreditBefore,CASE WHEN @CreditBefore<@CreditRefundAmount THEN 0 ELSE @CreditBefore-@CreditRefundAmount END,N'Paid',N'Customer credit restored from refund.',N'Customer credit restored from refund.',@CompletedByUserId);
            END
        END
        COMMIT;
        SELECT r.*, h.SaleNo FROM dbo.SalesReturnHeader r JOIN dbo.SalesHeader h ON h.SalesHeaderId=r.SalesHeaderId WHERE r.SalesReturnHeaderId=@SalesReturnHeaderId;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spReceiptPrintHistoryGetBySalesHeaderId] @SalesHeaderId BIGINT AS BEGIN SET NOCOUNT ON; SELECT * FROM dbo.ReceiptPrintHistory WHERE SalesHeaderId=@SalesHeaderId ORDER BY PrintedDate DESC; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spReceiptPrintHistoryGetBySalesReturnHeaderId] @SalesReturnHeaderId BIGINT AS BEGIN SET NOCOUNT ON; SELECT * FROM dbo.ReceiptPrintHistory WHERE SalesReturnHeaderId=@SalesReturnHeaderId ORDER BY PrintedDate DESC; END;
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
GO

CREATE OR ALTER PROCEDURE dbo.spSalesDocumentGetBySalesHeaderId @SalesHeaderId BIGINT AS
BEGIN
    SET NOCOUNT ON;
    SELECT d.*, h.SaleNo FROM dbo.SalesDocument d JOIN dbo.SalesHeader h ON h.SalesHeaderId=d.SalesHeaderId WHERE d.SalesHeaderId=@SalesHeaderId ORDER BY d.IssueDate DESC, d.SalesDocumentId DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.spSalesDocumentRecordPrint @SalesDocumentId BIGINT, @PrintedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.SalesDocument SET PrintedCount = PrintedCount + 1 WHERE SalesDocumentId=@SalesDocumentId;
    SELECT TOP (1) d.*, h.SaleNo FROM dbo.SalesDocument d JOIN dbo.SalesHeader h ON h.SalesHeaderId=d.SalesHeaderId WHERE d.SalesDocumentId=@SalesDocumentId;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spSalesDailyClosingGet]
    @ClosingDate DATE,
    @CashierUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartUtc DATETIME2(0) = CONVERT(DATETIME2(0), @ClosingDate);
    DECLARE @EndUtc DATETIME2(0) = DATEADD(DAY, 1, @StartUtc);

    ;WITH SaleScope AS
    (
        SELECT *
        FROM dbo.SalesHeader
        WHERE SaleDate >= @StartUtc
          AND SaleDate < @EndUtc
          AND Status <> N'Voided'
          AND (@CashierUserId IS NULL OR CashierUserId = @CashierUserId)
    ),
    PaymentSummary AS
    (
        SELECT
            SUM(CASE WHEN pm.PaymentMethodCode = N'CASH' OR pm.IsCash = 1 THEN sp.PaymentAmount ELSE 0 END) AS CashAmount,
            SUM(CASE WHEN pm.PaymentMethodCode IN (N'TRANSFER', N'QR') THEN sp.PaymentAmount ELSE 0 END) AS TransferAmount,
            SUM(CASE WHEN pm.PaymentMethodCode = N'CREDIT' THEN sp.PaymentAmount ELSE 0 END) AS CreditAmount,
            SUM(CASE WHEN pm.PaymentMethodCode NOT IN (N'CASH', N'TRANSFER', N'QR', N'CREDIT') AND pm.IsCash = 0 THEN sp.PaymentAmount ELSE 0 END) AS OtherPaymentAmount
        FROM SaleScope s
        JOIN dbo.SalesPayment sp ON sp.SalesHeaderId = s.SalesHeaderId
        JOIN dbo.PaymentMethod pm ON pm.PaymentMethodId = sp.PaymentMethodId
    ),
    ProfitSummary AS
    (
        SELECT
            SUM(si.LineTotal) AS SalesAfterItemDiscount,
            SUM(si.Quantity * si.CostPriceSnapshot) AS CostOfGoodsSold,
            SUM(si.LineTotal - (si.Quantity * si.CostPriceSnapshot)) AS GrossProfitAmount
        FROM SaleScope s
        JOIN dbo.SalesItem si ON si.SalesHeaderId = s.SalesHeaderId
    ),
    RefundSummary AS
    (
        SELECT
            SUM(rh.RefundNetAmount) AS RefundAmount
        FROM dbo.SalesReturnHeader rh
        WHERE rh.ReturnDate >= @StartUtc
          AND rh.ReturnDate < @EndUtc
          AND rh.Status = N'Completed'
          AND (@CashierUserId IS NULL OR rh.CashierUserId = @CashierUserId)
    ),
    StockSummary AS
    (
        SELECT
            COUNT(1) AS StockMovementCount,
            SUM(ABS(im.QuantitySigned) * im.UnitCost) AS StockMovementValue
        FROM dbo.InventoryMovement im
        WHERE im.CreatedDate >= @StartUtc
          AND im.CreatedDate < @EndUtc
          AND im.ReferenceType IN (N'Sale', N'SalesReturn', N'SaleVoid')
    )
    SELECT
        @ClosingDate AS ClosingDate,
        @CashierUserId AS CashierUserId,
        CAST(COUNT(s.SalesHeaderId) AS INT) AS TransactionCount,
        ISNULL(SUM(s.SubtotalAmount), 0) AS GrossSalesAmount,
        ISNULL(SUM(s.TotalDiscountAmount), 0) AS DiscountAmount,
        ISNULL(SUM(s.TaxAmount), 0) AS TaxAmount,
        ISNULL(SUM(s.NetAmount), 0) AS NetSalesAmount,
        ISNULL(MAX(r.RefundAmount), 0) AS RefundAmount,
        ISNULL(MAX(p.CashAmount), 0) AS CashAmount,
        ISNULL(MAX(p.TransferAmount), 0) AS TransferAmount,
        ISNULL(MAX(p.CreditAmount), 0) AS CreditAmount,
        ISNULL(MAX(p.OtherPaymentAmount), 0) AS OtherPaymentAmount,
        ISNULL(MAX(p.CashAmount), 0) - ISNULL(MAX(r.RefundAmount), 0) AS ExpectedCashAmount,
        ISNULL(MAX(ps.CostOfGoodsSold), 0) AS CostOfGoodsSold,
        ISNULL(MAX(ps.GrossProfitAmount), 0) - ISNULL(SUM(s.OrderDiscountAmount), 0) AS GrossProfitAmount,
        ISNULL(MAX(st.StockMovementCount), 0) AS StockMovementCount,
        ISNULL(MAX(st.StockMovementValue), 0) AS StockMovementValue,
        MAX(c.DailySalesClosingId) AS DailySalesClosingId,
        MAX(c.ActualCashAmount) AS ActualCashAmount,
        MAX(c.CashDifferenceAmount) AS CashDifferenceAmount,
        MAX(c.Notes) AS Notes,
        MAX(c.ClosedByUserId) AS ClosedByUserId,
        MAX(c.ClosedAtUtc) AS ClosedAtUtc,
        MAX(u.DisplayName) AS ClosedByName
    FROM SaleScope s
    CROSS JOIN PaymentSummary p
    CROSS JOIN ProfitSummary ps
    CROSS JOIN RefundSummary r
    CROSS JOIN StockSummary st
    LEFT JOIN dbo.DailySalesClosing c ON c.ClosingDate = @ClosingDate AND ISNULL(c.CashierUserId, 0) = ISNULL(@CashierUserId, 0)
    LEFT JOIN dbo.AccessUser u ON u.UserId = c.ClosedByUserId;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spSalesDailyClosingSave]
    @ClosingDate DATE,
    @CashierUserId INT = NULL,
    @ActualCashAmount DECIMAL(18,4),
    @Notes NVARCHAR(1000) = N'',
    @ClosedByUserId INT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;

    IF @ActualCashAmount < 0 THROW 52400, 'Actual cash amount cannot be negative.', 1;
    IF @ClosedByUserId <= 0 THROW 52401, 'Closed by user is required.', 1;

    DECLARE @Summary TABLE
    (
        ClosingDate DATE,
        CashierUserId INT NULL,
        TransactionCount INT,
        GrossSalesAmount DECIMAL(18,4),
        DiscountAmount DECIMAL(18,4),
        TaxAmount DECIMAL(18,4),
        NetSalesAmount DECIMAL(18,4),
        RefundAmount DECIMAL(18,4),
        CashAmount DECIMAL(18,4),
        TransferAmount DECIMAL(18,4),
        CreditAmount DECIMAL(18,4),
        OtherPaymentAmount DECIMAL(18,4),
        ExpectedCashAmount DECIMAL(18,4),
        CostOfGoodsSold DECIMAL(18,4),
        GrossProfitAmount DECIMAL(18,4),
        StockMovementCount INT,
        StockMovementValue DECIMAL(18,4),
        DailySalesClosingId BIGINT NULL,
        ActualCashAmount DECIMAL(18,4) NULL,
        CashDifferenceAmount DECIMAL(18,4) NULL,
        Notes NVARCHAR(1000) NULL,
        ClosedByUserId INT NULL,
        ClosedAtUtc DATETIME2(0) NULL,
        ClosedByName NVARCHAR(100) NULL
    );

    INSERT INTO @Summary
    EXEC dbo.spSalesDailyClosingGet @ClosingDate = @ClosingDate, @CashierUserId = @CashierUserId;

    MERGE dbo.DailySalesClosing AS target
    USING
    (
        SELECT TOP 1
            ClosingDate,
            CashierUserId,
            GrossSalesAmount,
            DiscountAmount,
            TaxAmount,
            NetSalesAmount,
            RefundAmount,
            ExpectedCashAmount,
            GrossProfitAmount
        FROM @Summary
    ) AS source
    ON target.ClosingDate = source.ClosingDate AND ISNULL(target.CashierUserId, 0) = ISNULL(source.CashierUserId, 0)
    WHEN MATCHED THEN
        UPDATE SET
            GrossSalesAmount = source.GrossSalesAmount,
            DiscountAmount = source.DiscountAmount,
            TaxAmount = source.TaxAmount,
            NetSalesAmount = source.NetSalesAmount,
            RefundAmount = source.RefundAmount,
            ExpectedCashAmount = source.ExpectedCashAmount,
            ActualCashAmount = @ActualCashAmount,
            GrossProfitAmount = source.GrossProfitAmount,
            Notes = ISNULL(@Notes, N''),
            UpdatedByUserId = @ClosedByUserId,
            UpdatedAtUtc = SYSUTCDATETIME()
    WHEN NOT MATCHED THEN
        INSERT
        (
            ClosingDate, CashierUserId, GrossSalesAmount, DiscountAmount, TaxAmount, NetSalesAmount,
            RefundAmount, ExpectedCashAmount, ActualCashAmount, GrossProfitAmount, Notes, ClosedByUserId
        )
        VALUES
        (
            source.ClosingDate, source.CashierUserId, source.GrossSalesAmount, source.DiscountAmount, source.TaxAmount,
            source.NetSalesAmount, source.RefundAmount, source.ExpectedCashAmount, @ActualCashAmount,
            source.GrossProfitAmount, ISNULL(@Notes, N''), @ClosedByUserId
        );

    EXEC dbo.spSalesDailyClosingGet @ClosingDate = @ClosingDate, @CashierUserId = @CashierUserId;
END;
GO

