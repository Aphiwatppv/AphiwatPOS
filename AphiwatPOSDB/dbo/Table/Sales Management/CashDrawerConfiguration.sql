CREATE TABLE [dbo].[CashDrawerConfiguration]
(
    [ConfigurationId] INT IDENTITY(1,1) NOT NULL,
    [PrinterName] NVARCHAR(200) NOT NULL,
    [CashDrawerEnabled] BIT NOT NULL CONSTRAINT [DF_CashDrawerConfiguration_CashDrawerEnabled] DEFAULT (1),
    [DrawerKickCommand] NVARCHAR(100) NOT NULL CONSTRAINT [DF_CashDrawerConfiguration_DrawerKickCommand] DEFAULT (N'27,112,0,25,250'),
    [DrawerPin] INT NOT NULL CONSTRAINT [DF_CashDrawerConfiguration_DrawerPin] DEFAULT (2),
    [OpenDrawerAfterReceiptPrint] BIT NOT NULL CONSTRAINT [DF_CashDrawerConfiguration_OpenDrawerAfterReceiptPrint] DEFAULT (1),
    [AllowManualOpenDrawer] BIT NOT NULL CONSTRAINT [DF_CashDrawerConfiguration_AllowManualOpenDrawer] DEFAULT (1),
    [IsActive] BIT NOT NULL CONSTRAINT [DF_CashDrawerConfiguration_IsActive] DEFAULT (1),
    [CreatedByUserId] INT NULL,
    [UpdatedByUserId] INT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_CashDrawerConfiguration_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    [UpdatedDate] DATETIME2(0) NULL,
    CONSTRAINT [PK_CashDrawerConfiguration] PRIMARY KEY CLUSTERED ([ConfigurationId]),
    CONSTRAINT [CK_CashDrawerConfiguration_DrawerPin] CHECK ([DrawerPin] IN (2,5)),
    CONSTRAINT [FK_CashDrawerConfiguration_CreatedBy] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[AccessUser] ([UserId]),
    CONSTRAINT [FK_CashDrawerConfiguration_UpdatedBy] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[AccessUser] ([UserId])
);
