CREATE TABLE [dbo].[StockAdjustment]
(
    [StockAdjustmentId] BIGINT IDENTITY(1,1) NOT NULL,
    [AdjustmentNo] NVARCHAR(50) NOT NULL,
    [LocationId] INT NOT NULL,
    [Status] NVARCHAR(20) NOT NULL CONSTRAINT [DF_StockAdjustment_Status] DEFAULT (N'Draft'),
    [Reason] NVARCHAR(500) NOT NULL,
    [Remarks] NVARCHAR(1000) NOT NULL CONSTRAINT [DF_StockAdjustment_Remarks] DEFAULT (N''),
    [CreatedByUserId] INT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_StockAdjustment_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    [UpdatedByUserId] INT NULL,
    [UpdatedDate] DATETIME2(0) NULL,
    CONSTRAINT [PK_StockAdjustment] PRIMARY KEY CLUSTERED ([StockAdjustmentId] ASC),
    CONSTRAINT [UQ_StockAdjustment_AdjustmentNo] UNIQUE ([AdjustmentNo]),
    CONSTRAINT [FK_StockAdjustment_Location] FOREIGN KEY ([LocationId]) REFERENCES [dbo].[InventoryLocation] ([LocationId]),
    CONSTRAINT [CK_StockAdjustment_Status] CHECK ([Status] IN (N'Draft',N'Approved',N'Rejected',N'Cancelled')),
    CONSTRAINT [CK_StockAdjustment_Reason] CHECK (LEN(LTRIM(RTRIM([Reason]))) > 0)
);
