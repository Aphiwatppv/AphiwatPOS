CREATE TABLE [dbo].[StockTransferItem]
(
    [StockTransferItemId] BIGINT IDENTITY(1,1) NOT NULL,
    [StockTransferId] BIGINT NOT NULL,
    [ProductId] INT NOT NULL,
    [Quantity] DECIMAL(18,4) NOT NULL,
    [UnitCost] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_StockTransferItem_UnitCost] DEFAULT (0),
    CONSTRAINT [PK_StockTransferItem] PRIMARY KEY CLUSTERED ([StockTransferItemId] ASC),
    CONSTRAINT [UQ_StockTransferItem_Transfer_Product] UNIQUE ([StockTransferId], [ProductId]),
    CONSTRAINT [FK_StockTransferItem_Transfer] FOREIGN KEY ([StockTransferId]) REFERENCES [dbo].[StockTransfer] ([StockTransferId]),
    CONSTRAINT [FK_StockTransferItem_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([ProductId]),
    CONSTRAINT [CK_StockTransferItem_QuantityPositive] CHECK ([Quantity] > 0)
);
