CREATE PROCEDURE [dbo].[spProductCreate] @ProductCode NVARCHAR(50), @SKU NVARCHAR(100), @Barcode NVARCHAR(100), @ProductName NVARCHAR(200), @CategoryId INT, @BrandId INT=NULL, @UnitId INT, @CostPrice DECIMAL(18,4), @MinimumCost DECIMAL(18,4), @VatPercentage DECIMAL(9,4), @VatAmount DECIMAL(18,4), @MinimumSellingPrice DECIMAL(18,4), @SellingPrice DECIMAL(18,4), @WholesalePrice DECIMAL(18,4), @WholesaleMinQty DECIMAL(18,4), @TaxRate DECIMAL(9,4), @DiscountAllowed BIT, @IsStockTracked BIT, @MinimumStockLevel DECIMAL(18,4), @CurrentStock DECIMAL(18,4), @ProductImageUrl NVARCHAR(500), @Description NVARCHAR(1000), @Status NVARCHAR(30), @CreatedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    SET @VatAmount = @MinimumCost * @VatPercentage / 100;
    SET @MinimumSellingPrice = @MinimumCost + @VatAmount;
    SET @TaxRate = @VatPercentage;
    IF @SellingPrice < @MinimumSellingPrice THROW 51001, 'Selling price cannot be lower than minimum selling price.', 1;
    INSERT INTO [dbo].[Product] (ProductCode,SKU,Barcode,ProductName,CategoryId,BrandId,UnitId,CostPrice,MinimumCost,VatPercentage,VatAmount,MinimumSellingPrice,SellingPrice,WholesalePrice,WholesaleMinQty,TaxRate,DiscountAllowed,IsStockTracked,MinimumStockLevel,CurrentStock,ProductImageUrl,Description,Status,CreatedByUserId) VALUES (@ProductCode,ISNULL(@SKU,N''),NULLIF(@Barcode,N''),@ProductName,@CategoryId,@BrandId,@UnitId,@CostPrice,@MinimumCost,@VatPercentage,@VatAmount,@MinimumSellingPrice,@SellingPrice,@WholesalePrice,@WholesaleMinQty,@TaxRate,@DiscountAllowed,@IsStockTracked,@MinimumStockLevel,@CurrentStock,ISNULL(@ProductImageUrl,N''),ISNULL(@Description,N''),@Status,NULLIF(@CreatedByUserId,0));
    SELECT CAST(SCOPE_IDENTITY() AS INT);
END;
