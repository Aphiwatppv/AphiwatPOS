CREATE TABLE [dbo].[ProductBrand]
(
    [BrandId] INT IDENTITY(1,1) NOT NULL,
    [BrandCode] NVARCHAR(50) NOT NULL,
    [BrandName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NOT NULL CONSTRAINT [DF_ProductBrand_Description] DEFAULT (N''),
    [LogoUrl] NVARCHAR(500) NOT NULL CONSTRAINT [DF_ProductBrand_LogoUrl] DEFAULT (N''),
    [IsActive] BIT NOT NULL CONSTRAINT [DF_ProductBrand_IsActive] DEFAULT (1),
    [CreatedByUserId] INT NULL,
    [UpdatedByUserId] INT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_ProductBrand_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    [UpdatedDate] DATETIME2(0) NULL,
    CONSTRAINT [PK_ProductBrand] PRIMARY KEY CLUSTERED ([BrandId] ASC),
    CONSTRAINT [UQ_ProductBrand_BrandCode] UNIQUE ([BrandCode])
);
