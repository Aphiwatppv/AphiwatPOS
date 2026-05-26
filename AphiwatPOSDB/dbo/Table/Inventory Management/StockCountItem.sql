CREATE TABLE [dbo].[StockCountItem]
(
    [StockCountItemId] BIGINT IDENTITY(1,1) NOT NULL,
    [StockCountId] BIGINT NOT NULL,
    [ProductId] INT NOT NULL,
    [SystemQty] DECIMAL(18,4) NOT NULL,
    [CountedQty] DECIMAL(18,4) NOT NULL,
    [VarianceQty] AS ([CountedQty] - [SystemQty]) PERSISTED,
    [Remarks] NVARCHAR(1000) NOT NULL CONSTRAINT [DF_StockCountItem_Remarks] DEFAULT (N''),
    CONSTRAINT [PK_StockCountItem] PRIMARY KEY CLUSTERED ([StockCountItemId] ASC),
    CONSTRAINT [UQ_StockCountItem_Count_Product] UNIQUE ([StockCountId], [ProductId]),
    CONSTRAINT [FK_StockCountItem_Count] FOREIGN KEY ([StockCountId]) REFERENCES [dbo].[StockCount] ([StockCountId]),
    CONSTRAINT [FK_StockCountItem_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([ProductId]),
    CONSTRAINT [CK_StockCountItem_CountedQtyNonNegative] CHECK ([CountedQty] >= 0)
);
