CREATE TABLE [dbo].[ProductCategory]
(
    [CategoryId] INT IDENTITY(1,1) NOT NULL,
    [CategoryCode] NVARCHAR(50) NOT NULL,
    [CategoryName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NOT NULL CONSTRAINT [DF_ProductCategory_Description] DEFAULT (N''),
    [DisplayOrder] INT NOT NULL CONSTRAINT [DF_ProductCategory_DisplayOrder] DEFAULT (0),
    [IsActive] BIT NOT NULL CONSTRAINT [DF_ProductCategory_IsActive] DEFAULT (1),
    [CreatedByUserId] INT NULL,
    [UpdatedByUserId] INT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_ProductCategory_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    [UpdatedDate] DATETIME2(0) NULL,
    CONSTRAINT [PK_ProductCategory] PRIMARY KEY CLUSTERED ([CategoryId] ASC),
    CONSTRAINT [UQ_ProductCategory_CategoryCode] UNIQUE ([CategoryCode])
);
