IF COL_LENGTH('dbo.Product', 'WholesalePrice') IS NULL
BEGIN
    ALTER TABLE dbo.Product ADD WholesalePrice DECIMAL(18,4) NOT NULL CONSTRAINT DF_Product_WholesalePrice DEFAULT (0);
END;
GO

IF COL_LENGTH('dbo.Product', 'WholesaleMinQty') IS NULL
BEGIN
    ALTER TABLE dbo.Product ADD WholesaleMinQty DECIMAL(18,4) NOT NULL CONSTRAINT DF_Product_WholesaleMinQty DEFAULT (1);
END;
GO

UPDATE dbo.Product
SET WholesalePrice = SellingPrice
WHERE WholesalePrice = 0;
GO
