/* Adds minimum selling price controls to existing Product tables. */

IF COL_LENGTH('dbo.Product', 'MinimumCost') IS NULL
    ALTER TABLE [dbo].[Product] ADD [MinimumCost] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_Product_MinimumCost] DEFAULT (0);
GO

IF COL_LENGTH('dbo.Product', 'VatMode') IS NULL
    ALTER TABLE [dbo].[Product] ADD [VatMode] NVARCHAR(20) NOT NULL CONSTRAINT [DF_Product_VatMode] DEFAULT (N'VatExcluded');
GO

IF COL_LENGTH('dbo.Product', 'VatPercentage') IS NULL
    ALTER TABLE [dbo].[Product] ADD [VatPercentage] DECIMAL(9,4) NOT NULL CONSTRAINT [DF_Product_VatPercentage] DEFAULT (0);
GO

IF COL_LENGTH('dbo.Product', 'VatAmount') IS NULL
    ALTER TABLE [dbo].[Product] ADD [VatAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_Product_VatAmount] DEFAULT (0);
GO

IF COL_LENGTH('dbo.Product', 'MinimumSellingPrice') IS NULL
    ALTER TABLE [dbo].[Product] ADD [MinimumSellingPrice] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_Product_MinimumSellingPrice] DEFAULT (0);
GO

UPDATE [dbo].[Product]
SET
    MinimumCost = CASE WHEN MinimumCost = 0 THEN CostPrice ELSE MinimumCost END,
    VatMode = CASE WHEN VatMode IN (N'NoVat', N'VatIncluded', N'VatExcluded') THEN VatMode ELSE N'VatExcluded' END,
    VatPercentage = CASE WHEN VatPercentage = 0 THEN TaxRate ELSE VatPercentage END,
    VatAmount = (CASE WHEN MinimumCost = 0 THEN CostPrice ELSE MinimumCost END) * (CASE WHEN VatPercentage = 0 THEN TaxRate ELSE VatPercentage END) / 100,
    MinimumSellingPrice = (CASE WHEN MinimumCost = 0 THEN CostPrice ELSE MinimumCost END) + ((CASE WHEN MinimumCost = 0 THEN CostPrice ELSE MinimumCost END) * (CASE WHEN VatPercentage = 0 THEN TaxRate ELSE VatPercentage END) / 100);
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Product_VatMode' AND parent_object_id = OBJECT_ID(N'dbo.Product'))
    ALTER TABLE [dbo].[Product] WITH NOCHECK ADD CONSTRAINT [CK_Product_VatMode] CHECK ([VatMode] IN (N'NoVat', N'VatIncluded', N'VatExcluded'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Product_SellingPriceMinimum' AND parent_object_id = OBJECT_ID(N'dbo.Product'))
    ALTER TABLE [dbo].[Product] WITH NOCHECK ADD CONSTRAINT [CK_Product_SellingPriceMinimum] CHECK ([SellingPrice] >= [MinimumSellingPrice]);
GO

UPDATE [dbo].[Product]
SET [Status] = CASE [Status]
    WHEN N'ใช้งาน' THEN N'Active'
    WHEN N'ไม่ใช้งาน' THEN N'Inactive'
    WHEN N'ยกเลิกจำหน่าย' THEN N'Discontinued'
    WHEN N'แบบร่าง' THEN N'Draft'
    ELSE [Status]
END
WHERE [Status] IN (N'ใช้งาน', N'ไม่ใช้งาน', N'ยกเลิกจำหน่าย', N'แบบร่าง');
GO
