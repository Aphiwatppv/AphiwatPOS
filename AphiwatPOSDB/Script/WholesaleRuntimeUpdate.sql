CREATE OR ALTER PROCEDURE [dbo].[spProductCreate] @ProductCode NVARCHAR(50), @SKU NVARCHAR(100), @Barcode NVARCHAR(100), @ProductName NVARCHAR(200), @CategoryId INT, @BrandId INT=NULL, @UnitId INT, @CostPrice DECIMAL(18,4), @SellingPrice DECIMAL(18,4), @WholesalePrice DECIMAL(18,4), @WholesaleMinQty DECIMAL(18,4), @TaxRate DECIMAL(9,4), @DiscountAllowed BIT, @IsStockTracked BIT, @MinimumStockLevel DECIMAL(18,4), @CurrentStock DECIMAL(18,4), @ProductImageUrl NVARCHAR(500), @Description NVARCHAR(1000), @Status NVARCHAR(30), @CreatedByUserId INT AS
BEGIN SET NOCOUNT ON; INSERT INTO [dbo].[Product] (ProductCode,SKU,Barcode,ProductName,CategoryId,BrandId,UnitId,CostPrice,SellingPrice,WholesalePrice,WholesaleMinQty,TaxRate,DiscountAllowed,IsStockTracked,MinimumStockLevel,CurrentStock,ProductImageUrl,Description,Status,CreatedByUserId) VALUES (@ProductCode,ISNULL(@SKU,N''),NULLIF(@Barcode,N''),@ProductName,@CategoryId,@BrandId,@UnitId,@CostPrice,@SellingPrice,@WholesalePrice,@WholesaleMinQty,@TaxRate,@DiscountAllowed,@IsStockTracked,@MinimumStockLevel,@CurrentStock,ISNULL(@ProductImageUrl,N''),ISNULL(@Description,N''),@Status,NULLIF(@CreatedByUserId,0)); SELECT CAST(SCOPE_IDENTITY() AS INT); END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spProductUpdate] @ProductId INT, @ProductCode NVARCHAR(50), @SKU NVARCHAR(100), @Barcode NVARCHAR(100), @ProductName NVARCHAR(200), @CategoryId INT, @BrandId INT=NULL, @UnitId INT, @CostPrice DECIMAL(18,4), @SellingPrice DECIMAL(18,4), @WholesalePrice DECIMAL(18,4), @WholesaleMinQty DECIMAL(18,4), @TaxRate DECIMAL(9,4), @DiscountAllowed BIT, @IsStockTracked BIT, @MinimumStockLevel DECIMAL(18,4), @CurrentStock DECIMAL(18,4), @ProductImageUrl NVARCHAR(500), @Description NVARCHAR(1000), @Status NVARCHAR(30), @IsActive BIT, @UpdatedByUserId INT AS
BEGIN SET NOCOUNT ON; UPDATE [dbo].[Product] SET ProductCode=@ProductCode, SKU=ISNULL(@SKU,N''), Barcode=NULLIF(@Barcode,N''), ProductName=@ProductName, CategoryId=@CategoryId, BrandId=@BrandId, UnitId=@UnitId, CostPrice=@CostPrice, SellingPrice=@SellingPrice, WholesalePrice=@WholesalePrice, WholesaleMinQty=@WholesaleMinQty, TaxRate=@TaxRate, DiscountAllowed=@DiscountAllowed, IsStockTracked=@IsStockTracked, MinimumStockLevel=@MinimumStockLevel, CurrentStock=@CurrentStock, ProductImageUrl=ISNULL(@ProductImageUrl,N''), Description=ISNULL(@Description,N''), Status=@Status, IsActive=@IsActive, UpdatedByUserId=NULLIF(@UpdatedByUserId,0), UpdatedDate=SYSUTCDATETIME() WHERE ProductId=@ProductId; END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spProductUpdatePrice] @ProductId INT, @NewCostPrice DECIMAL(18,4), @NewSellingPrice DECIMAL(18,4), @NewWholesalePrice DECIMAL(18,4), @NewWholesaleMinQty DECIMAL(18,4), @ChangeReason NVARCHAR(500), @ChangedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @OldCostPrice DECIMAL(18,4), @OldSellingPrice DECIMAL(18,4), @ProfitAmount DECIMAL(18,4), @ProfitMargin DECIMAL(9,4);
        SELECT @OldCostPrice=CostPrice, @OldSellingPrice=SellingPrice FROM [dbo].[Product] WITH (UPDLOCK, ROWLOCK) WHERE ProductId=@ProductId;
        IF @OldCostPrice IS NULL THROW 51000, 'Product was not found.', 1;
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
