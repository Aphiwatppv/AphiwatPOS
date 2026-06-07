/* Product Management backend database script.
   Run this against the POS database after the authentication schema has been deployed. */

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
GO

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
GO

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
GO

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
GO

CREATE TABLE [dbo].[Product]
(
    [ProductId] INT IDENTITY(1,1) NOT NULL,
    [ProductCode] NVARCHAR(50) NOT NULL,
    [SKU] NVARCHAR(100) NOT NULL CONSTRAINT [DF_Product_SKU] DEFAULT (N''),
    [Barcode] NVARCHAR(100) NULL,
    [ProductName] NVARCHAR(200) NOT NULL,
    [CategoryId] INT NOT NULL,
    [BrandId] INT NULL,
    [UnitId] INT NOT NULL,
    [CostPrice] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_Product_CostPrice] DEFAULT (0),
    [MinimumCost] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_Product_MinimumCost] DEFAULT (0),
    [VatPercentage] DECIMAL(9,4) NOT NULL CONSTRAINT [DF_Product_VatPercentage] DEFAULT (0),
    [VatAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_Product_VatAmount] DEFAULT (0),
    [MinimumSellingPrice] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_Product_MinimumSellingPrice] DEFAULT (0),
    [SellingPrice] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_Product_SellingPrice] DEFAULT (0),
    [WholesalePrice] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_Product_WholesalePrice] DEFAULT (0),
    [WholesaleMinQty] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_Product_WholesaleMinQty] DEFAULT (1),
    [TaxRate] DECIMAL(9,4) NOT NULL CONSTRAINT [DF_Product_TaxRate] DEFAULT (0),
    [DiscountAllowed] BIT NOT NULL CONSTRAINT [DF_Product_DiscountAllowed] DEFAULT (1),
    [IsStockTracked] BIT NOT NULL CONSTRAINT [DF_Product_IsStockTracked] DEFAULT (1),
    [MinimumStockLevel] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_Product_MinimumStockLevel] DEFAULT (0),
    [CurrentStock] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_Product_CurrentStock] DEFAULT (0),
    [ProductImageUrl] NVARCHAR(500) NOT NULL CONSTRAINT [DF_Product_ProductImageUrl] DEFAULT (N''),
    [Description] NVARCHAR(1000) NOT NULL CONSTRAINT [DF_Product_Description] DEFAULT (N''),
    [Status] NVARCHAR(30) NOT NULL CONSTRAINT [DF_Product_Status] DEFAULT (N'Active'),
    [IsActive] BIT NOT NULL CONSTRAINT [DF_Product_IsActive] DEFAULT (1),
    [CreatedByUserId] INT NULL,
    [UpdatedByUserId] INT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_Product_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    [UpdatedDate] DATETIME2(0) NULL,
    CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED ([ProductId] ASC),
    CONSTRAINT [FK_Product_Category] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[ProductCategory] ([CategoryId]),
    CONSTRAINT [FK_Product_Brand] FOREIGN KEY ([BrandId]) REFERENCES [dbo].[ProductBrand] ([BrandId]),
    CONSTRAINT [FK_Product_Unit] FOREIGN KEY ([UnitId]) REFERENCES [dbo].[ProductUnit] ([UnitId]),
    CONSTRAINT [UQ_Product_ProductCode] UNIQUE ([ProductCode]),
    CONSTRAINT [CK_Product_PricesNonNegative] CHECK ([CostPrice] >= 0 AND [MinimumCost] >= 0 AND [VatPercentage] >= 0 AND [VatAmount] >= 0 AND [MinimumSellingPrice] >= 0 AND [SellingPrice] >= 0 AND [WholesalePrice] >= 0 AND [WholesaleMinQty] >= 0),
    CONSTRAINT [CK_Product_SellingPriceMinimum] CHECK ([SellingPrice] >= [MinimumSellingPrice]),
    CONSTRAINT [CK_Product_StockNonNegative] CHECK ([MinimumStockLevel] >= 0 AND [CurrentStock] >= 0)
);
GO

CREATE TABLE [dbo].[ProductPriceHistory]
(
    [ProductPriceHistoryId] INT IDENTITY(1,1) NOT NULL,
    [ProductId] INT NOT NULL,
    [OldCostPrice] DECIMAL(18,4) NOT NULL,
    [NewCostPrice] DECIMAL(18,4) NOT NULL,
    [OldSellingPrice] DECIMAL(18,4) NOT NULL,
    [NewSellingPrice] DECIMAL(18,4) NOT NULL,
    [ProfitAmount] DECIMAL(18,4) NOT NULL,
    [ProfitMargin] DECIMAL(9,4) NOT NULL,
    [ChangeReason] NVARCHAR(500) NOT NULL CONSTRAINT [DF_ProductPriceHistory_ChangeReason] DEFAULT (N''),
    [ChangedByUserId] INT NULL,
    [ChangedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_ProductPriceHistory_ChangedDate] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_ProductPriceHistory] PRIMARY KEY CLUSTERED ([ProductPriceHistoryId] ASC),
    CONSTRAINT [FK_ProductPriceHistory_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([ProductId])
);
GO

CREATE UNIQUE INDEX [UX_Product_Barcode_NotNull] ON [dbo].[Product] ([Barcode]) WHERE [Barcode] IS NOT NULL;
CREATE INDEX [IX_Product_CategoryId] ON [dbo].[Product] ([CategoryId]);
CREATE INDEX [IX_Product_BrandId] ON [dbo].[Product] ([BrandId]);
CREATE INDEX [IX_Product_UnitId] ON [dbo].[Product] ([UnitId]);
CREATE INDEX [IX_Product_IsActive] ON [dbo].[Product] ([IsActive]);
CREATE INDEX [IX_Product_Status] ON [dbo].[Product] ([Status]);
CREATE INDEX [IX_Product_Barcode] ON [dbo].[Product] ([Barcode]);
CREATE INDEX [IX_Product_ProductName] ON [dbo].[Product] ([ProductName]);
CREATE UNIQUE INDEX [UX_ProductUnitConversion_ActivePair] ON [dbo].[ProductUnitConversion] ([FromUnitId], [ToUnitId]) WHERE [IsActive] = 1;
CREATE INDEX [IX_ProductPriceHistory_ProductId_ChangedDate] ON [dbo].[ProductPriceHistory] ([ProductId], [ChangedDate] DESC);
GO

CREATE OR ALTER PROCEDURE [dbo].[spProductCategoryGetPaged]
    @PageNumber INT = 1, @PageSize INT = 20, @SearchText NVARCHAR(100) = NULL, @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET @PageNumber = CASE WHEN @PageNumber < 1 THEN 1 ELSE @PageNumber END;
    SET @PageSize = CASE WHEN @PageSize < 1 THEN 20 ELSE @PageSize END;

    SELECT *, COUNT(1) OVER() AS TotalCount
    FROM [dbo].[ProductCategory]
    WHERE (@IsActive IS NULL OR IsActive = @IsActive)
      AND (@SearchText IS NULL OR CategoryCode LIKE N'%' + @SearchText + N'%' OR CategoryName LIKE N'%' + @SearchText + N'%')
    ORDER BY DisplayOrder, CategoryName
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductCategoryGetAllActive] AS BEGIN SET NOCOUNT ON; SELECT * FROM [dbo].[ProductCategory] WHERE IsActive = 1 ORDER BY DisplayOrder, CategoryName; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductCategoryGetById] @CategoryId INT AS BEGIN SET NOCOUNT ON; SELECT * FROM [dbo].[ProductCategory] WHERE CategoryId = @CategoryId; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductCategoryCreate] @CategoryCode NVARCHAR(50), @CategoryName NVARCHAR(100), @Description NVARCHAR(500), @DisplayOrder INT, @CreatedByUserId INT AS
BEGIN SET NOCOUNT ON;
    INSERT INTO [dbo].[ProductCategory] (CategoryCode, CategoryName, Description, DisplayOrder, CreatedByUserId)
    VALUES (@CategoryCode, @CategoryName, ISNULL(@Description, N''), @DisplayOrder, NULLIF(@CreatedByUserId, 0));
    SELECT CAST(SCOPE_IDENTITY() AS INT);
END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductCategoryUpdate] @CategoryId INT, @CategoryCode NVARCHAR(50), @CategoryName NVARCHAR(100), @Description NVARCHAR(500), @DisplayOrder INT, @IsActive BIT, @UpdatedByUserId INT AS
BEGIN SET NOCOUNT ON;
    UPDATE [dbo].[ProductCategory] SET CategoryCode=@CategoryCode, CategoryName=@CategoryName, Description=ISNULL(@Description,N''), DisplayOrder=@DisplayOrder, IsActive=@IsActive, UpdatedByUserId=NULLIF(@UpdatedByUserId,0), UpdatedDate=SYSUTCDATETIME() WHERE CategoryId=@CategoryId;
END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductCategoryDeactivate] @CategoryId INT, @UpdatedByUserId INT AS BEGIN SET NOCOUNT ON; UPDATE [dbo].[ProductCategory] SET IsActive=0, UpdatedByUserId=NULLIF(@UpdatedByUserId,0), UpdatedDate=SYSUTCDATETIME() WHERE CategoryId=@CategoryId; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductCategoryCheckCodeExists] @CategoryCode NVARCHAR(50), @ExcludeCategoryId INT = NULL AS BEGIN SET NOCOUNT ON; SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM [dbo].[ProductCategory] WHERE CategoryCode=@CategoryCode AND (@ExcludeCategoryId IS NULL OR CategoryId<>@ExcludeCategoryId)) THEN 1 ELSE 0 END AS BIT); END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductCategoryCheckNameExists] @CategoryName NVARCHAR(100), @ExcludeCategoryId INT = NULL AS BEGIN SET NOCOUNT ON; SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM [dbo].[ProductCategory] WHERE CategoryName=@CategoryName AND (@ExcludeCategoryId IS NULL OR CategoryId<>@ExcludeCategoryId)) THEN 1 ELSE 0 END AS BIT); END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spProductBrandGetPaged] @PageNumber INT = 1, @PageSize INT = 20, @SearchText NVARCHAR(100) = NULL, @IsActive BIT = NULL AS
BEGIN SET NOCOUNT ON; SET @PageNumber=CASE WHEN @PageNumber<1 THEN 1 ELSE @PageNumber END; SET @PageSize=CASE WHEN @PageSize<1 THEN 20 ELSE @PageSize END;
    SELECT *, COUNT(1) OVER() AS TotalCount FROM [dbo].[ProductBrand]
    WHERE (@IsActive IS NULL OR IsActive=@IsActive) AND (@SearchText IS NULL OR BrandCode LIKE N'%'+@SearchText+N'%' OR BrandName LIKE N'%'+@SearchText+N'%')
    ORDER BY BrandName OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductBrandGetAllActive] AS BEGIN SET NOCOUNT ON; SELECT * FROM [dbo].[ProductBrand] WHERE IsActive=1 ORDER BY BrandName; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductBrandGetById] @BrandId INT AS BEGIN SET NOCOUNT ON; SELECT * FROM [dbo].[ProductBrand] WHERE BrandId=@BrandId; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductBrandCreate] @BrandCode NVARCHAR(50), @BrandName NVARCHAR(100), @Description NVARCHAR(500), @LogoUrl NVARCHAR(500), @CreatedByUserId INT AS BEGIN SET NOCOUNT ON; INSERT INTO [dbo].[ProductBrand] (BrandCode,BrandName,Description,LogoUrl,CreatedByUserId) VALUES (@BrandCode,@BrandName,ISNULL(@Description,N''),ISNULL(@LogoUrl,N''),NULLIF(@CreatedByUserId,0)); SELECT CAST(SCOPE_IDENTITY() AS INT); END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductBrandUpdate] @BrandId INT, @BrandCode NVARCHAR(50), @BrandName NVARCHAR(100), @Description NVARCHAR(500), @LogoUrl NVARCHAR(500), @IsActive BIT, @UpdatedByUserId INT AS BEGIN SET NOCOUNT ON; UPDATE [dbo].[ProductBrand] SET BrandCode=@BrandCode, BrandName=@BrandName, Description=ISNULL(@Description,N''), LogoUrl=ISNULL(@LogoUrl,N''), IsActive=@IsActive, UpdatedByUserId=NULLIF(@UpdatedByUserId,0), UpdatedDate=SYSUTCDATETIME() WHERE BrandId=@BrandId; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductBrandDeactivate] @BrandId INT, @UpdatedByUserId INT AS BEGIN SET NOCOUNT ON; UPDATE [dbo].[ProductBrand] SET IsActive=0, UpdatedByUserId=NULLIF(@UpdatedByUserId,0), UpdatedDate=SYSUTCDATETIME() WHERE BrandId=@BrandId; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductBrandCheckCodeExists] @BrandCode NVARCHAR(50), @ExcludeBrandId INT = NULL AS BEGIN SET NOCOUNT ON; SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM [dbo].[ProductBrand] WHERE BrandCode=@BrandCode AND (@ExcludeBrandId IS NULL OR BrandId<>@ExcludeBrandId)) THEN 1 ELSE 0 END AS BIT); END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductBrandCheckNameExists] @BrandName NVARCHAR(100), @ExcludeBrandId INT = NULL AS BEGIN SET NOCOUNT ON; SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM [dbo].[ProductBrand] WHERE BrandName=@BrandName AND (@ExcludeBrandId IS NULL OR BrandId<>@ExcludeBrandId)) THEN 1 ELSE 0 END AS BIT); END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spProductUnitGetPaged] @PageNumber INT = 1, @PageSize INT = 20, @SearchText NVARCHAR(100) = NULL, @IsActive BIT = NULL AS
BEGIN SET NOCOUNT ON; SET @PageNumber=CASE WHEN @PageNumber<1 THEN 1 ELSE @PageNumber END; SET @PageSize=CASE WHEN @PageSize<1 THEN 20 ELSE @PageSize END;
    SELECT *, COUNT(1) OVER() AS TotalCount FROM [dbo].[ProductUnit]
    WHERE (@IsActive IS NULL OR IsActive=@IsActive) AND (@SearchText IS NULL OR UnitCode LIKE N'%'+@SearchText+N'%' OR UnitName LIKE N'%'+@SearchText+N'%' OR UnitSymbol LIKE N'%'+@SearchText+N'%')
    ORDER BY UnitName OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductUnitGetAllActive] AS BEGIN SET NOCOUNT ON; SELECT * FROM [dbo].[ProductUnit] WHERE IsActive=1 ORDER BY UnitName; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductUnitGetById] @UnitId INT AS BEGIN SET NOCOUNT ON; SELECT * FROM [dbo].[ProductUnit] WHERE UnitId=@UnitId; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductUnitCreate] @UnitCode NVARCHAR(50), @UnitName NVARCHAR(100), @UnitSymbol NVARCHAR(30), @AllowDecimal BIT, @IsBaseUnit BIT, @Description NVARCHAR(500), @CreatedByUserId INT AS BEGIN SET NOCOUNT ON; INSERT INTO [dbo].[ProductUnit] (UnitCode,UnitName,UnitSymbol,AllowDecimal,IsBaseUnit,Description,CreatedByUserId) VALUES (@UnitCode,@UnitName,ISNULL(@UnitSymbol,N''),@AllowDecimal,@IsBaseUnit,ISNULL(@Description,N''),NULLIF(@CreatedByUserId,0)); SELECT CAST(SCOPE_IDENTITY() AS INT); END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductUnitUpdate] @UnitId INT, @UnitCode NVARCHAR(50), @UnitName NVARCHAR(100), @UnitSymbol NVARCHAR(30), @AllowDecimal BIT, @IsBaseUnit BIT, @Description NVARCHAR(500), @IsActive BIT, @UpdatedByUserId INT AS BEGIN SET NOCOUNT ON; UPDATE [dbo].[ProductUnit] SET UnitCode=@UnitCode, UnitName=@UnitName, UnitSymbol=ISNULL(@UnitSymbol,N''), AllowDecimal=@AllowDecimal, IsBaseUnit=@IsBaseUnit, Description=ISNULL(@Description,N''), IsActive=@IsActive, UpdatedByUserId=NULLIF(@UpdatedByUserId,0), UpdatedDate=SYSUTCDATETIME() WHERE UnitId=@UnitId; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductUnitDeactivate] @UnitId INT, @UpdatedByUserId INT AS BEGIN SET NOCOUNT ON; UPDATE [dbo].[ProductUnit] SET IsActive=0, UpdatedByUserId=NULLIF(@UpdatedByUserId,0), UpdatedDate=SYSUTCDATETIME() WHERE UnitId=@UnitId; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductUnitCheckCodeExists] @UnitCode NVARCHAR(50), @ExcludeUnitId INT = NULL AS BEGIN SET NOCOUNT ON; SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM [dbo].[ProductUnit] WHERE UnitCode=@UnitCode AND (@ExcludeUnitId IS NULL OR UnitId<>@ExcludeUnitId)) THEN 1 ELSE 0 END AS BIT); END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductUnitCheckNameExists] @UnitName NVARCHAR(100), @ExcludeUnitId INT = NULL AS BEGIN SET NOCOUNT ON; SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM [dbo].[ProductUnit] WHERE UnitName=@UnitName AND (@ExcludeUnitId IS NULL OR UnitId<>@ExcludeUnitId)) THEN 1 ELSE 0 END AS BIT); END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spProductUnitConversionGetByUnitId] @UnitId INT AS BEGIN SET NOCOUNT ON; SELECT c.*, fu.UnitCode AS FromUnitCode, fu.UnitName AS FromUnitName, tu.UnitCode AS ToUnitCode, tu.UnitName AS ToUnitName FROM [dbo].[ProductUnitConversion] c INNER JOIN [dbo].[ProductUnit] fu ON fu.UnitId=c.FromUnitId INNER JOIN [dbo].[ProductUnit] tu ON tu.UnitId=c.ToUnitId WHERE (c.FromUnitId=@UnitId OR c.ToUnitId=@UnitId) AND c.IsActive=1 ORDER BY fu.UnitName, tu.UnitName; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductUnitConversionGetById] @UnitConversionId INT AS BEGIN SET NOCOUNT ON; SELECT c.*, fu.UnitCode AS FromUnitCode, fu.UnitName AS FromUnitName, tu.UnitCode AS ToUnitCode, tu.UnitName AS ToUnitName FROM [dbo].[ProductUnitConversion] c INNER JOIN [dbo].[ProductUnit] fu ON fu.UnitId=c.FromUnitId INNER JOIN [dbo].[ProductUnit] tu ON tu.UnitId=c.ToUnitId WHERE c.UnitConversionId=@UnitConversionId; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductUnitConversionCreate] @FromUnitId INT, @ToUnitId INT, @ConversionRate DECIMAL(18,6), @Description NVARCHAR(500), @CreatedByUserId INT AS BEGIN SET NOCOUNT ON; INSERT INTO [dbo].[ProductUnitConversion] (FromUnitId,ToUnitId,ConversionRate,Description,CreatedByUserId) VALUES (@FromUnitId,@ToUnitId,@ConversionRate,ISNULL(@Description,N''),NULLIF(@CreatedByUserId,0)); SELECT CAST(SCOPE_IDENTITY() AS INT); END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductUnitConversionUpdate] @UnitConversionId INT, @FromUnitId INT, @ToUnitId INT, @ConversionRate DECIMAL(18,6), @Description NVARCHAR(500), @IsActive BIT, @UpdatedByUserId INT AS BEGIN SET NOCOUNT ON; UPDATE [dbo].[ProductUnitConversion] SET FromUnitId=@FromUnitId, ToUnitId=@ToUnitId, ConversionRate=@ConversionRate, Description=ISNULL(@Description,N''), IsActive=@IsActive, UpdatedByUserId=NULLIF(@UpdatedByUserId,0), UpdatedDate=SYSUTCDATETIME() WHERE UnitConversionId=@UnitConversionId; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductUnitConversionDeactivate] @UnitConversionId INT, @UpdatedByUserId INT AS BEGIN SET NOCOUNT ON; UPDATE [dbo].[ProductUnitConversion] SET IsActive=0, UpdatedByUserId=NULLIF(@UpdatedByUserId,0), UpdatedDate=SYSUTCDATETIME() WHERE UnitConversionId=@UnitConversionId; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductUnitConversionCheckDuplicate] @FromUnitId INT, @ToUnitId INT, @ExcludeUnitConversionId INT = NULL AS BEGIN SET NOCOUNT ON; SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM [dbo].[ProductUnitConversion] WHERE FromUnitId=@FromUnitId AND ToUnitId=@ToUnitId AND IsActive=1 AND (@ExcludeUnitConversionId IS NULL OR UnitConversionId<>@ExcludeUnitConversionId)) THEN 1 ELSE 0 END AS BIT); END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spProductGetPaged] @PageNumber INT=1, @PageSize INT=20, @SearchText NVARCHAR(200)=NULL, @IsActive BIT=NULL, @CategoryId INT=NULL, @BrandId INT=NULL, @UnitId INT=NULL, @Status NVARCHAR(30)=NULL, @LowStockOnly BIT=0 AS
BEGIN SET NOCOUNT ON; SET @PageNumber=CASE WHEN @PageNumber<1 THEN 1 ELSE @PageNumber END; SET @PageSize=CASE WHEN @PageSize<1 THEN 20 ELSE @PageSize END;
    SELECT p.*, c.CategoryName, b.BrandName, u.UnitName, COUNT(1) OVER() AS TotalCount
    FROM [dbo].[Product] p
    INNER JOIN [dbo].[ProductCategory] c ON c.CategoryId=p.CategoryId
    LEFT JOIN [dbo].[ProductBrand] b ON b.BrandId=p.BrandId
    INNER JOIN [dbo].[ProductUnit] u ON u.UnitId=p.UnitId
    WHERE (@IsActive IS NULL OR p.IsActive=@IsActive)
      AND (@CategoryId IS NULL OR p.CategoryId=@CategoryId)
      AND (@BrandId IS NULL OR p.BrandId=@BrandId)
      AND (@UnitId IS NULL OR p.UnitId=@UnitId)
      AND (@Status IS NULL OR p.Status=@Status)
      AND (@LowStockOnly=0 OR (p.IsStockTracked=1 AND p.CurrentStock<=p.MinimumStockLevel))
      AND (@SearchText IS NULL OR p.ProductCode LIKE N'%'+@SearchText+N'%' OR p.ProductName LIKE N'%'+@SearchText+N'%' OR p.SKU LIKE N'%'+@SearchText+N'%' OR p.Barcode LIKE N'%'+@SearchText+N'%')
    ORDER BY p.ProductName OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductGetAllActive] AS BEGIN SET NOCOUNT ON; SELECT p.*, c.CategoryName, b.BrandName, u.UnitName FROM [dbo].[Product] p INNER JOIN [dbo].[ProductCategory] c ON c.CategoryId=p.CategoryId LEFT JOIN [dbo].[ProductBrand] b ON b.BrandId=p.BrandId INNER JOIN [dbo].[ProductUnit] u ON u.UnitId=p.UnitId WHERE p.IsActive=1 ORDER BY p.ProductName; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductGetById] @ProductId INT AS BEGIN SET NOCOUNT ON; SELECT p.*, c.CategoryName, b.BrandName, u.UnitName FROM [dbo].[Product] p INNER JOIN [dbo].[ProductCategory] c ON c.CategoryId=p.CategoryId LEFT JOIN [dbo].[ProductBrand] b ON b.BrandId=p.BrandId INNER JOIN [dbo].[ProductUnit] u ON u.UnitId=p.UnitId WHERE p.ProductId=@ProductId; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductCreate] @ProductCode NVARCHAR(50), @SKU NVARCHAR(100), @Barcode NVARCHAR(100), @ProductName NVARCHAR(200), @CategoryId INT, @BrandId INT=NULL, @UnitId INT, @CostPrice DECIMAL(18,4), @MinimumCost DECIMAL(18,4), @VatPercentage DECIMAL(9,4), @VatAmount DECIMAL(18,4), @MinimumSellingPrice DECIMAL(18,4), @SellingPrice DECIMAL(18,4), @WholesalePrice DECIMAL(18,4), @WholesaleMinQty DECIMAL(18,4), @TaxRate DECIMAL(9,4), @DiscountAllowed BIT, @IsStockTracked BIT, @MinimumStockLevel DECIMAL(18,4), @CurrentStock DECIMAL(18,4), @ProductImageUrl NVARCHAR(500), @Description NVARCHAR(1000), @Status NVARCHAR(30), @CreatedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    SET @VatAmount = @MinimumCost * @VatPercentage / 100;
    SET @MinimumSellingPrice = @MinimumCost + @VatAmount;
    SET @TaxRate = @VatPercentage;
    IF @SellingPrice < @MinimumSellingPrice THROW 51001, 'Selling price cannot be lower than minimum selling price.', 1;
    INSERT INTO [dbo].[Product] (ProductCode,SKU,Barcode,ProductName,CategoryId,BrandId,UnitId,CostPrice,MinimumCost,VatPercentage,VatAmount,MinimumSellingPrice,SellingPrice,WholesalePrice,WholesaleMinQty,TaxRate,DiscountAllowed,IsStockTracked,MinimumStockLevel,CurrentStock,ProductImageUrl,Description,Status,CreatedByUserId) VALUES (@ProductCode,ISNULL(@SKU,N''),NULLIF(@Barcode,N''),@ProductName,@CategoryId,@BrandId,@UnitId,@CostPrice,@MinimumCost,@VatPercentage,@VatAmount,@MinimumSellingPrice,@SellingPrice,@WholesalePrice,@WholesaleMinQty,@TaxRate,@DiscountAllowed,@IsStockTracked,@MinimumStockLevel,@CurrentStock,ISNULL(@ProductImageUrl,N''),ISNULL(@Description,N''),@Status,NULLIF(@CreatedByUserId,0));
    SELECT CAST(SCOPE_IDENTITY() AS INT);
END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductUpdate] @ProductId INT, @ProductCode NVARCHAR(50), @SKU NVARCHAR(100), @Barcode NVARCHAR(100), @ProductName NVARCHAR(200), @CategoryId INT, @BrandId INT=NULL, @UnitId INT, @CostPrice DECIMAL(18,4), @MinimumCost DECIMAL(18,4), @VatPercentage DECIMAL(9,4), @VatAmount DECIMAL(18,4), @MinimumSellingPrice DECIMAL(18,4), @SellingPrice DECIMAL(18,4), @WholesalePrice DECIMAL(18,4), @WholesaleMinQty DECIMAL(18,4), @TaxRate DECIMAL(9,4), @DiscountAllowed BIT, @IsStockTracked BIT, @MinimumStockLevel DECIMAL(18,4), @CurrentStock DECIMAL(18,4), @ProductImageUrl NVARCHAR(500), @Description NVARCHAR(1000), @Status NVARCHAR(30), @IsActive BIT, @UpdatedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    SET @VatAmount = @MinimumCost * @VatPercentage / 100;
    SET @MinimumSellingPrice = @MinimumCost + @VatAmount;
    SET @TaxRate = @VatPercentage;
    IF @SellingPrice < @MinimumSellingPrice THROW 51001, 'Selling price cannot be lower than minimum selling price.', 1;
    UPDATE [dbo].[Product] SET ProductCode=@ProductCode, SKU=ISNULL(@SKU,N''), Barcode=NULLIF(@Barcode,N''), ProductName=@ProductName, CategoryId=@CategoryId, BrandId=@BrandId, UnitId=@UnitId, CostPrice=@CostPrice, MinimumCost=@MinimumCost, VatPercentage=@VatPercentage, VatAmount=@VatAmount, MinimumSellingPrice=@MinimumSellingPrice, SellingPrice=@SellingPrice, WholesalePrice=@WholesalePrice, WholesaleMinQty=@WholesaleMinQty, TaxRate=@TaxRate, DiscountAllowed=@DiscountAllowed, IsStockTracked=@IsStockTracked, MinimumStockLevel=@MinimumStockLevel, CurrentStock=@CurrentStock, ProductImageUrl=ISNULL(@ProductImageUrl,N''), Description=ISNULL(@Description,N''), Status=@Status, IsActive=@IsActive, UpdatedByUserId=NULLIF(@UpdatedByUserId,0), UpdatedDate=SYSUTCDATETIME() WHERE ProductId=@ProductId;
END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductDeactivate] @ProductId INT, @UpdatedByUserId INT AS BEGIN SET NOCOUNT ON; UPDATE [dbo].[Product] SET IsActive=0, Status=N'Inactive', UpdatedByUserId=NULLIF(@UpdatedByUserId,0), UpdatedDate=SYSUTCDATETIME() WHERE ProductId=@ProductId; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductUpdatePrice] @ProductId INT, @NewCostPrice DECIMAL(18,4), @NewSellingPrice DECIMAL(18,4), @NewWholesalePrice DECIMAL(18,4), @NewWholesaleMinQty DECIMAL(18,4), @ChangeReason NVARCHAR(500), @ChangedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @OldCostPrice DECIMAL(18,4), @OldSellingPrice DECIMAL(18,4), @MinimumSellingPrice DECIMAL(18,4), @ProfitAmount DECIMAL(18,4), @ProfitMargin DECIMAL(9,4);
        SELECT @OldCostPrice=CostPrice, @OldSellingPrice=SellingPrice, @MinimumSellingPrice=MinimumSellingPrice FROM [dbo].[Product] WITH (UPDLOCK, ROWLOCK) WHERE ProductId=@ProductId;
        IF @OldCostPrice IS NULL THROW 51000, 'Product was not found.', 1;
        IF @NewSellingPrice < @MinimumSellingPrice THROW 51001, 'Selling price cannot be lower than minimum selling price.', 1;
        SET @ProfitAmount = @NewSellingPrice - @NewCostPrice;
        SET @ProfitMargin = CASE WHEN @NewSellingPrice = 0 THEN 0 ELSE ((@NewSellingPrice - @NewCostPrice) / @NewSellingPrice) * 100 END;
        UPDATE [dbo].[Product] SET CostPrice=@NewCostPrice, SellingPrice=@NewSellingPrice, WholesalePrice=@NewWholesalePrice, WholesaleMinQty=@NewWholesaleMinQty, UpdatedByUserId=NULLIF(@ChangedByUserId,0), UpdatedDate=SYSUTCDATETIME() WHERE ProductId=@ProductId;
        INSERT INTO [dbo].[ProductPriceHistory] (ProductId, OldCostPrice, NewCostPrice, OldSellingPrice, NewSellingPrice, ProfitAmount, ProfitMargin, ChangeReason, ChangedByUserId)
        VALUES (@ProductId, @OldCostPrice, @NewCostPrice, @OldSellingPrice, @NewSellingPrice, @ProfitAmount, @ProfitMargin, ISNULL(@ChangeReason,N''), NULLIF(@ChangedByUserId,0));
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductUpdateImage] @ProductId INT, @ProductImageUrl NVARCHAR(500), @UpdatedByUserId INT AS BEGIN SET NOCOUNT ON; UPDATE [dbo].[Product] SET ProductImageUrl=ISNULL(@ProductImageUrl,N''), UpdatedByUserId=NULLIF(@UpdatedByUserId,0), UpdatedDate=SYSUTCDATETIME() WHERE ProductId=@ProductId; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductUpdateBarcode] @ProductId INT, @Barcode NVARCHAR(100), @UpdatedByUserId INT AS BEGIN SET NOCOUNT ON; UPDATE [dbo].[Product] SET Barcode=NULLIF(@Barcode,N''), UpdatedByUserId=NULLIF(@UpdatedByUserId,0), UpdatedDate=SYSUTCDATETIME() WHERE ProductId=@ProductId; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductCheckCodeExists] @ProductCode NVARCHAR(50), @ExcludeProductId INT = NULL AS BEGIN SET NOCOUNT ON; SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM [dbo].[Product] WHERE ProductCode=@ProductCode AND (@ExcludeProductId IS NULL OR ProductId<>@ExcludeProductId)) THEN 1 ELSE 0 END AS BIT); END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductCheckBarcodeExists] @Barcode NVARCHAR(100), @ExcludeProductId INT = NULL AS BEGIN SET NOCOUNT ON; SELECT CAST(CASE WHEN @Barcode IS NOT NULL AND EXISTS(SELECT 1 FROM [dbo].[Product] WHERE Barcode=@Barcode AND (@ExcludeProductId IS NULL OR ProductId<>@ExcludeProductId)) THEN 1 ELSE 0 END AS BIT); END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductGetLowStock] AS BEGIN SET NOCOUNT ON; SELECT p.*, c.CategoryName, b.BrandName, u.UnitName FROM [dbo].[Product] p INNER JOIN [dbo].[ProductCategory] c ON c.CategoryId=p.CategoryId LEFT JOIN [dbo].[ProductBrand] b ON b.BrandId=p.BrandId INNER JOIN [dbo].[ProductUnit] u ON u.UnitId=p.UnitId WHERE p.IsActive=1 AND p.IsStockTracked=1 AND p.CurrentStock<=p.MinimumStockLevel ORDER BY p.ProductName; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductGetByBarcode] @Barcode NVARCHAR(100) AS BEGIN SET NOCOUNT ON; SELECT p.*, c.CategoryName, b.BrandName, u.UnitName FROM [dbo].[Product] p INNER JOIN [dbo].[ProductCategory] c ON c.CategoryId=p.CategoryId LEFT JOIN [dbo].[ProductBrand] b ON b.BrandId=p.BrandId INNER JOIN [dbo].[ProductUnit] u ON u.UnitId=p.UnitId WHERE p.Barcode=@Barcode AND p.IsActive=1; END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spProductPriceHistoryGetByProductId] @ProductId INT AS BEGIN SET NOCOUNT ON; SELECT h.*, p.ProductCode, p.ProductName FROM [dbo].[ProductPriceHistory] h INNER JOIN [dbo].[Product] p ON p.ProductId=h.ProductId WHERE h.ProductId=@ProductId ORDER BY h.ChangedDate DESC, h.ProductPriceHistoryId DESC; END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductPriceHistoryGetPaged] @PageNumber INT=1, @PageSize INT=20, @SearchText NVARCHAR(200)=NULL, @ProductId INT=NULL, @FromDate DATETIME2(0)=NULL, @ToDate DATETIME2(0)=NULL AS
BEGIN SET NOCOUNT ON; SET @PageNumber=CASE WHEN @PageNumber<1 THEN 1 ELSE @PageNumber END; SET @PageSize=CASE WHEN @PageSize<1 THEN 20 ELSE @PageSize END;
    SELECT h.*, p.ProductCode, p.ProductName, COUNT(1) OVER() AS TotalCount
    FROM [dbo].[ProductPriceHistory] h INNER JOIN [dbo].[Product] p ON p.ProductId=h.ProductId
    WHERE (@ProductId IS NULL OR h.ProductId=@ProductId)
      AND (@FromDate IS NULL OR h.ChangedDate>=@FromDate)
      AND (@ToDate IS NULL OR h.ChangedDate<DATEADD(DAY,1,@ToDate))
      AND (@SearchText IS NULL OR p.ProductCode LIKE N'%'+@SearchText+N'%' OR p.ProductName LIKE N'%'+@SearchText+N'%' OR h.ChangeReason LIKE N'%'+@SearchText+N'%')
    ORDER BY h.ChangedDate DESC, h.ProductPriceHistoryId DESC OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO
CREATE OR ALTER PROCEDURE [dbo].[spProductPriceHistoryCreate] @ProductId INT, @OldCostPrice DECIMAL(18,4), @NewCostPrice DECIMAL(18,4), @OldSellingPrice DECIMAL(18,4), @NewSellingPrice DECIMAL(18,4), @ProfitAmount DECIMAL(18,4), @ProfitMargin DECIMAL(9,4), @ChangeReason NVARCHAR(500), @ChangedByUserId INT AS
BEGIN SET NOCOUNT ON; INSERT INTO [dbo].[ProductPriceHistory] (ProductId, OldCostPrice, NewCostPrice, OldSellingPrice, NewSellingPrice, ProfitAmount, ProfitMargin, ChangeReason, ChangedByUserId) VALUES (@ProductId,@OldCostPrice,@NewCostPrice,@OldSellingPrice,@NewSellingPrice,@ProfitAmount,@ProfitMargin,ISNULL(@ChangeReason,N''),NULLIF(@ChangedByUserId,0)); SELECT CAST(SCOPE_IDENTITY() AS INT); END;
GO


