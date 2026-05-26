CREATE TABLE [dbo].[InventoryLocation]
(
    [LocationId] INT IDENTITY(1,1) NOT NULL,
    [LocationCode] NVARCHAR(50) NOT NULL,
    [LocationName] NVARCHAR(150) NOT NULL,
    [Description] NVARCHAR(500) NOT NULL CONSTRAINT [DF_InventoryLocation_Description] DEFAULT (N''),
    [IsDefault] BIT NOT NULL CONSTRAINT [DF_InventoryLocation_IsDefault] DEFAULT (0),
    [IsActive] BIT NOT NULL CONSTRAINT [DF_InventoryLocation_IsActive] DEFAULT (1),
    [CreatedByUserId] INT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_InventoryLocation_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    [UpdatedByUserId] INT NULL,
    [UpdatedDate] DATETIME2(0) NULL,
    CONSTRAINT [PK_InventoryLocation] PRIMARY KEY CLUSTERED ([LocationId] ASC),
    CONSTRAINT [UQ_InventoryLocation_LocationCode] UNIQUE ([LocationCode])
);
