CREATE TABLE [dbo].[StockAdjustmentItem]
(
    [StockAdjustmentItemId] BIGINT IDENTITY(1,1) NOT NULL,
    [StockAdjustmentId] BIGINT NOT NULL,
    [ProductId] INT NOT NULL,
    [Quantity] DECIMAL(18,4) NOT NULL,
    [AdjustmentType] NVARCHAR(20) NOT NULL,
    [UnitCost] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_StockAdjustmentItem_UnitCost] DEFAULT (0),
    [Reason] NVARCHAR(500) NOT NULL,
    CONSTRAINT [PK_StockAdjustmentItem] PRIMARY KEY CLUSTERED ([StockAdjustmentItemId] ASC),
    CONSTRAINT [FK_StockAdjustmentItem_Adjustment] FOREIGN KEY ([StockAdjustmentId]) REFERENCES [dbo].[StockAdjustment] ([StockAdjustmentId]),
    CONSTRAINT [FK_StockAdjustmentItem_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([ProductId]),
    CONSTRAINT [CK_StockAdjustmentItem_AdjustmentType] CHECK ([AdjustmentType] IN (N'Increase',N'Decrease')),
    CONSTRAINT [CK_StockAdjustmentItem_QuantityPositive] CHECK ([Quantity] > 0),
    CONSTRAINT [CK_StockAdjustmentItem_Reason] CHECK (LEN(LTRIM(RTRIM([Reason]))) > 0)
);
