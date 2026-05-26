CREATE TABLE [dbo].[ProductUnit]
(
    [UnitId] INT IDENTITY(1,1) NOT NULL,
    [UnitCode] NVARCHAR(50) NOT NULL,
    [UnitName] NVARCHAR(100) NOT NULL,
    [UnitSymbol] NVARCHAR(30) NOT NULL CONSTRAINT [DF_ProductUnit_UnitSymbol] DEFAULT (N''),
    [AllowDecimal] BIT NOT NULL CONSTRAINT [DF_ProductUnit_AllowDecimal] DEFAULT (0),
    [IsBaseUnit] BIT NOT NULL CONSTRAINT [DF_ProductUnit_IsBaseUnit] DEFAULT (0),
    [Description] NVARCHAR(500) NOT NULL CONSTRAINT [DF_ProductUnit_Description] DEFAULT (N''),
    [IsActive] BIT NOT NULL CONSTRAINT [DF_ProductUnit_IsActive] DEFAULT (1),
    [CreatedByUserId] INT NULL,
    [UpdatedByUserId] INT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_ProductUnit_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    [UpdatedDate] DATETIME2(0) NULL,
    CONSTRAINT [PK_ProductUnit] PRIMARY KEY CLUSTERED ([UnitId] ASC),
    CONSTRAINT [UQ_ProductUnit_UnitCode] UNIQUE ([UnitCode])
);
