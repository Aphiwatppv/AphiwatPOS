CREATE PROCEDURE [dbo].[spProductUpdatePrice] @ProductId INT, @NewCostPrice DECIMAL(18,4), @NewSellingPrice DECIMAL(18,4), @NewWholesalePrice DECIMAL(18,4), @NewWholesaleMinQty DECIMAL(18,4), @ChangeReason NVARCHAR(500), @ChangedByUserId INT AS
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
