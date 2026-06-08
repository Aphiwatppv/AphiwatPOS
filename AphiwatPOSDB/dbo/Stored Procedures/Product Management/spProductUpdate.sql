CREATE PROCEDURE [dbo].[spProductUpdate] @ProductId INT, @ProductCode NVARCHAR(50), @SKU NVARCHAR(100), @Barcode NVARCHAR(100), @ProductName NVARCHAR(200), @CategoryId INT, @BrandId INT=NULL, @UnitId INT, @CostPrice DECIMAL(18,4), @MinimumCost DECIMAL(18,4), @VatMode NVARCHAR(20), @VatPercentage DECIMAL(9,4), @VatAmount DECIMAL(18,4), @MinimumSellingPrice DECIMAL(18,4), @SellingPrice DECIMAL(18,4), @WholesalePrice DECIMAL(18,4), @WholesaleMinQty DECIMAL(18,4), @TaxRate DECIMAL(9,4), @DiscountAllowed BIT, @IsStockTracked BIT, @MinimumStockLevel DECIMAL(18,4), @CurrentStock DECIMAL(18,4), @ProductImageUrl NVARCHAR(500), @Description NVARCHAR(1000), @Status NVARCHAR(30), @IsActive BIT, @UpdatedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    SET @VatMode = CASE WHEN @VatMode IN (N'NoVat', N'VatIncluded', N'VatExcluded') THEN @VatMode ELSE N'VatExcluded' END;
    SET @VatPercentage = CASE WHEN @VatMode = N'NoVat' THEN 0 ELSE @VatPercentage END;
    SET @VatAmount = CASE
        WHEN @VatMode = N'NoVat' THEN 0
        WHEN @VatMode = N'VatIncluded' AND @VatPercentage > 0 THEN @MinimumCost * @VatPercentage / (100 + @VatPercentage)
        WHEN @VatMode = N'VatIncluded' THEN 0
        ELSE @MinimumCost * @VatPercentage / 100
    END;
    SET @MinimumSellingPrice = CASE WHEN @VatMode = N'VatExcluded' THEN @MinimumCost + @VatAmount ELSE @MinimumCost END;
    SET @TaxRate = CASE WHEN @VatMode = N'NoVat' THEN 0 ELSE @VatPercentage END;
    IF @SellingPrice < @MinimumSellingPrice THROW 51001, 'Selling price cannot be lower than minimum selling price.', 1;
    UPDATE [dbo].[Product] SET ProductCode=@ProductCode, SKU=ISNULL(@SKU,N''), Barcode=NULLIF(@Barcode,N''), ProductName=@ProductName, CategoryId=@CategoryId, BrandId=@BrandId, UnitId=@UnitId, CostPrice=@CostPrice, MinimumCost=@MinimumCost, VatMode=@VatMode, VatPercentage=@VatPercentage, VatAmount=@VatAmount, MinimumSellingPrice=@MinimumSellingPrice, SellingPrice=@SellingPrice, WholesalePrice=@WholesalePrice, WholesaleMinQty=@WholesaleMinQty, TaxRate=@TaxRate, DiscountAllowed=@DiscountAllowed, IsStockTracked=@IsStockTracked, MinimumStockLevel=@MinimumStockLevel, CurrentStock=@CurrentStock, ProductImageUrl=ISNULL(@ProductImageUrl,N''), Description=ISNULL(@Description,N''), Status=@Status, IsActive=@IsActive, UpdatedByUserId=NULLIF(@UpdatedByUserId,0), UpdatedDate=SYSUTCDATETIME() WHERE ProductId=@ProductId;
END;
