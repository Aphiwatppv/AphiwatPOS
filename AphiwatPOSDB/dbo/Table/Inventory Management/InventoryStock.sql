CREATE TABLE [dbo].[InventoryStock]
(
    [InventoryStockId] BIGINT IDENTITY(1,1) NOT NULL,
    [ProductId] INT NOT NULL,
    [LocationId] INT NOT NULL,
    [CurrentStock] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_InventoryStock_CurrentStock] DEFAULT (0),
    [LastMovementDate] DATETIME2(0) NULL,
    [CreatedByUserId] INT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_InventoryStock_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    [UpdatedByUserId] INT NULL,
    [UpdatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_InventoryStock_UpdatedDate] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_InventoryStock] PRIMARY KEY CLUSTERED ([InventoryStockId] ASC),
    CONSTRAINT [UQ_InventoryStock_Product_Location] UNIQUE ([ProductId], [LocationId]),
    CONSTRAINT [FK_InventoryStock_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([ProductId]),
    CONSTRAINT [FK_InventoryStock_Location] FOREIGN KEY ([LocationId]) REFERENCES [dbo].[InventoryLocation] ([LocationId])
);
