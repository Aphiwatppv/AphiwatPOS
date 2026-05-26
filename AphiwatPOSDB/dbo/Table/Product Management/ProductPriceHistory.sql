CREATE TABLE [dbo].[ProductPriceHistory]
(
    [ProductPriceHistoryId] INT IDENTITY(1,1) NOT NULL,
    [ProductId] INT NOT NULL,
    [OldCostPrice] DECIMAL(18,4) NOT NULL,
    [NewCostPrice] DECIMAL(18,4) NOT NULL,
    [OldSellingPrice] DECIMAL(18,4) NOT NULL,
    [NewSellingPrice] DECIMAL(18,4) NOT NULL,
    [ProfitAmount] DECIMAL(18,4) NOT NULL,
    [ProfitMargin] DECIMAL(9,4) NOT NULL,
    [ChangeReason] NVARCHAR(500) NOT NULL CONSTRAINT [DF_ProductPriceHistory_ChangeReason] DEFAULT (N''),
    [ChangedByUserId] INT NULL,
    [ChangedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_ProductPriceHistory_ChangedDate] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_ProductPriceHistory] PRIMARY KEY CLUSTERED ([ProductPriceHistoryId] ASC),
    CONSTRAINT [FK_ProductPriceHistory_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([ProductId])
);
