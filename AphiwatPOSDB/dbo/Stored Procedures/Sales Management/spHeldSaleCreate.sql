CREATE PROCEDURE [dbo].[spHeldSaleCreate] @CustomerId INT=NULL,@CashierUserId INT,@Note NVARCHAR(1000)=N'',@EstimatedTaxAmount DECIMAL(18,4)=0,@CreatedByUserId INT,@ItemsJson NVARCHAR(MAX) AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @Items TABLE (ProductId INT, LocationId INT, Quantity DECIMAL(18,4), UnitPrice DECIMAL(18,4), ItemDiscountAmount DECIMAL(18,4), TaxAmount DECIMAL(18,4));
    INSERT INTO @Items SELECT productId, locationId, quantity, unitPrice, itemDiscountAmount, taxAmount FROM OPENJSON(@ItemsJson) WITH (productId INT '$.productId', locationId INT '$.locationId', quantity DECIMAL(18,4) '$.quantity', unitPrice DECIMAL(18,4) '$.unitPrice', itemDiscountAmount DECIMAL(18,4) '$.itemDiscountAmount', taxAmount DECIMAL(18,4) '$.taxAmount');
    IF NOT EXISTS (SELECT 1 FROM @Items) THROW 52200, 'Held sale must have at least one item.', 1;
    IF EXISTS (SELECT 1 FROM @Items WHERE Quantity <= 0 OR ItemDiscountAmount < 0) THROW 52201, 'Invalid held sale item amount.', 1;
    IF EXISTS (SELECT 1 FROM @Items i WHERE NOT EXISTS (SELECT 1 FROM dbo.Product p WHERE p.ProductId=i.ProductId AND p.IsActive=1 AND p.Status=N'Active')) THROW 52202, 'Held sale product is not active.', 1;
    UPDATE i
    SET UnitPrice = CASE WHEN i.UnitPrice > 0 THEN i.UnitPrice ELSE p.SellingPrice END,
        TaxAmount = ROUND(((i.Quantity * CASE WHEN i.UnitPrice > 0 THEN i.UnitPrice ELSE p.SellingPrice END) - i.ItemDiscountAmount) * (p.TaxRate / 100.0), 4)
    FROM @Items i
    JOIN dbo.Product p ON p.ProductId = i.ProductId;
    IF EXISTS (SELECT 1 FROM @Items i JOIN dbo.Product p ON p.ProductId=i.ProductId WHERE i.ItemDiscountAmount > 0 AND p.DiscountAllowed = 0) THROW 52203, 'Discount is not allowed for one or more products.', 1;
    IF EXISTS (SELECT 1 FROM @Items WHERE ItemDiscountAmount > Quantity * UnitPrice) THROW 52204, 'Item discount cannot exceed line subtotal.', 1;
    DECLARE @Subtotal DECIMAL(18,4)=(SELECT SUM(Quantity*UnitPrice) FROM @Items), @Discount DECIMAL(18,4)=(SELECT SUM(ItemDiscountAmount) FROM @Items), @LineTax DECIMAL(18,4)=(SELECT SUM(TaxAmount) FROM @Items);
    DECLARE @HeldSaleNo NVARCHAR(50)=CONCAT(N'HELD',FORMAT(SYSUTCDATETIME(),'yyyyMMddHHmmssfff')), @Id BIGINT;
    BEGIN TRY
        BEGIN TRANSACTION;
        INSERT dbo.HeldSaleHeader (HeldSaleNo, CustomerId, CashierUserId, Note, EstimatedSubtotalAmount, EstimatedDiscountAmount, EstimatedTaxAmount, EstimatedNetAmount, Status, CreatedByUserId)
        VALUES (@HeldSaleNo, @CustomerId, @CashierUserId, ISNULL(@Note,N''), @Subtotal, @Discount, @LineTax+ISNULL(@EstimatedTaxAmount,0), @Subtotal-@Discount+@LineTax+ISNULL(@EstimatedTaxAmount,0), N'Held', @CreatedByUserId);
        SET @Id=SCOPE_IDENTITY();
        INSERT dbo.HeldSaleItem (HeldSaleHeaderId, ProductId, ProductCodeSnapshot, ProductNameSnapshot, BarcodeSnapshot, UnitId, UnitSymbolSnapshot, Quantity, UnitPrice, CostPriceSnapshot, ItemDiscountAmount, TaxAmount, LineSubtotal, LineTotal)
        SELECT @Id,p.ProductId,p.ProductCode,p.ProductName,p.Barcode,p.UnitId,u.UnitSymbol,i.Quantity,i.UnitPrice,p.CostPrice,i.ItemDiscountAmount,i.TaxAmount,i.Quantity*i.UnitPrice,i.Quantity*i.UnitPrice-i.ItemDiscountAmount+i.TaxAmount FROM @Items i JOIN dbo.Product p ON p.ProductId=i.ProductId JOIN dbo.ProductUnit u ON u.UnitId=p.UnitId;
        COMMIT; SELECT @Id;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH
END;

