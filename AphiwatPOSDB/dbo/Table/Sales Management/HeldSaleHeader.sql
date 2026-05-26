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
