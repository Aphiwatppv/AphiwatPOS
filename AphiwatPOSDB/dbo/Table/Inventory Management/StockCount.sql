CREATE TABLE [dbo].[StockCount]
(
    [StockCountId] BIGINT IDENTITY(1,1) NOT NULL,
    [StockCountNo] NVARCHAR(50) NOT NULL,
    [LocationId] INT NOT NULL,
    [Status] NVARCHAR(20) NOT NULL CONSTRAINT [DF_StockCount_Status] DEFAULT (N'Draft'),
    [Remarks] NVARCHAR(1000) NOT NULL CONSTRAINT [DF_StockCount_Remarks] DEFAULT (N''),
    [CreatedByUserId] INT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_StockCount_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    [UpdatedByUserId] INT NULL,
    [UpdatedDate] DATETIME2(0) NULL,
    CONSTRAINT [PK_StockCount] PRIMARY KEY CLUSTERED ([StockCountId] ASC),
    CONSTRAINT [UQ_StockCount_StockCountNo] UNIQUE ([StockCountNo]),
    CONSTRAINT [FK_StockCount_Location] FOREIGN KEY ([LocationId]) REFERENCES [dbo].[InventoryLocation] ([LocationId]),
    CONSTRAINT [CK_StockCount_Status] CHECK ([Status] IN (N'Draft',N'Approved',N'Cancelled'))
);
