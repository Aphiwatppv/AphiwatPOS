CREATE TABLE [dbo].[ProductUnitConversion]
(
    [UnitConversionId] INT IDENTITY(1,1) NOT NULL,
    [FromUnitId] INT NOT NULL,
    [ToUnitId] INT NOT NULL,
    [ConversionRate] DECIMAL(18,6) NOT NULL,
    [Description] NVARCHAR(500) NOT NULL CONSTRAINT [DF_ProductUnitConversion_Description] DEFAULT (N''),
    [IsActive] BIT NOT NULL CONSTRAINT [DF_ProductUnitConversion_IsActive] DEFAULT (1),
    [CreatedByUserId] INT NULL,
    [UpdatedByUserId] INT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_ProductUnitConversion_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    [UpdatedDate] DATETIME2(0) NULL,
    CONSTRAINT [PK_ProductUnitConversion] PRIMARY KEY CLUSTERED ([UnitConversionId] ASC),
    CONSTRAINT [FK_ProductUnitConversion_FromUnit] FOREIGN KEY ([FromUnitId]) REFERENCES [dbo].[ProductUnit] ([UnitId]),
    CONSTRAINT [FK_ProductUnitConversion_ToUnit] FOREIGN KEY ([ToUnitId]) REFERENCES [dbo].[ProductUnit] ([UnitId]),
    CONSTRAINT [CK_ProductUnitConversion_PositiveRate] CHECK ([ConversionRate] > 0),
    CONSTRAINT [CK_ProductUnitConversion_DifferentUnits] CHECK ([FromUnitId] <> [ToUnitId])
);
