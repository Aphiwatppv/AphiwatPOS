CREATE TABLE [dbo].[StockTransfer]
(
    [StockTransferId] BIGINT IDENTITY(1,1) NOT NULL,
    [TransferNo] NVARCHAR(50) NOT NULL,
    [SourceLocationId] INT NOT NULL,
    [DestinationLocationId] INT NOT NULL,
    [Status] NVARCHAR(20) NOT NULL CONSTRAINT [DF_StockTransfer_Status] DEFAULT (N'Draft'),
    [Remarks] NVARCHAR(1000) NOT NULL CONSTRAINT [DF_StockTransfer_Remarks] DEFAULT (N''),
    [SentDate] DATETIME2(0) NULL,
    [ReceivedDate] DATETIME2(0) NULL,
    [CreatedByUserId] INT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_StockTransfer_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    [UpdatedByUserId] INT NULL,
    [UpdatedDate] DATETIME2(0) NULL,
    CONSTRAINT [PK_StockTransfer] PRIMARY KEY CLUSTERED ([StockTransferId] ASC),
    CONSTRAINT [UQ_StockTransfer_TransferNo] UNIQUE ([TransferNo]),
    CONSTRAINT [FK_StockTransfer_SourceLocation] FOREIGN KEY ([SourceLocationId]) REFERENCES [dbo].[InventoryLocation] ([LocationId]),
    CONSTRAINT [FK_StockTransfer_DestinationLocation] FOREIGN KEY ([DestinationLocationId]) REFERENCES [dbo].[InventoryLocation] ([LocationId]),
    CONSTRAINT [CK_StockTransfer_Status] CHECK ([Status] IN (N'Draft',N'Sent',N'Received',N'Cancelled')),
    CONSTRAINT [CK_StockTransfer_DifferentLocations] CHECK ([SourceLocationId] <> [DestinationLocationId])
);
