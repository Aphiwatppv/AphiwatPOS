CREATE PROCEDURE [dbo].[spSalesCompleteTransaction]
    @CustomerId INT = NULL,
    @CashierUserId INT,
    @HeldSaleHeaderId BIGINT = NULL,
    @UseCustomerCredit BIT = 0,
    @CustomerCreditAmount DECIMAL(18,2) = 0,
    @OrderDiscountAmount DECIMAL(18,4) = 0,
    @TaxAmount DECIMAL(18,4) = 0,
    @Remark NVARCHAR(1000) = N'',
    @AllowNegativeStock BIT = 0,
    @CreatedByUserId INT,
    @ItemsJson NVARCHAR(MAX),
    @PaymentsJson NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @Items TABLE (ProductId INT, LocationId INT, Quantity DECIMAL(18,4), UnitPrice DECIMAL(18,4), ItemDiscountAmount DECIMAL(18,4), TaxAmount DECIMAL(18,4));
    DECLARE @Payments TABLE (PaymentMethodId INT, PaymentAmount DECIMAL(18,4), ReferenceNo NVARCHAR(100));
    INSERT INTO @Items SELECT productId, locationId, quantity, unitPrice, itemDiscountAmount, taxAmount FROM OPENJSON(@ItemsJson) WITH (productId INT '$.productId', locationId INT '$.locationId', quantity DECIMAL(18,4) '$.quantity', unitPrice DECIMAL(18,4) '$.unitPrice', itemDiscountAmount DECIMAL(18,4) '$.itemDiscountAmount', taxAmount DECIMAL(18,4) '$.taxAmount');
    INSERT INTO @Payments SELECT paymentMethodId, paymentAmount, referenceNo FROM OPENJSON(@PaymentsJson) WITH (paymentMethodId INT '$.paymentMethodId', paymentAmount DECIMAL(18,4) '$.paymentAmount', referenceNo NVARCHAR(100) '$.referenceNo');

    IF NOT EXISTS (SELECT 1 FROM @Items) THROW 52100, 'Sale must have at least one item.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.CashDrawerSession WHERE CashierUserId = @CashierUserId AND Status = N'Open') THROW 52136, 'Cashier must open a cash drawer shift before making sales.', 1;
    IF EXISTS (SELECT 1 FROM @Items WHERE Quantity <= 0 OR ItemDiscountAmount < 0) THROW 52101, 'Invalid sale item amount.', 1;
    IF EXISTS (SELECT 1 FROM @Payments WHERE PaymentAmount <= 0) THROW 52102, 'Invalid payment amount.', 1;
    IF EXISTS (SELECT 1 FROM @Items i JOIN dbo.Product p ON p.ProductId=i.ProductId WHERE p.IsActive = 0 OR p.Status <> N'Active') THROW 52103, 'Product is not active.', 1;
    IF EXISTS (SELECT 1 FROM @Items i WHERE NOT EXISTS (SELECT 1 FROM dbo.Product p WHERE p.ProductId=i.ProductId)) THROW 52104, 'Product does not exist.', 1;
    IF EXISTS (SELECT 1 FROM @Payments p JOIN dbo.PaymentMethod pm ON pm.PaymentMethodId=p.PaymentMethodId WHERE pm.RequireReferenceNo=1 AND NULLIF(LTRIM(RTRIM(ISNULL(p.ReferenceNo,N''))),N'') IS NULL) THROW 52105, 'Payment reference number is required.', 1;
    UPDATE i
    SET UnitPrice = CASE WHEN i.UnitPrice > 0 THEN i.UnitPrice ELSE p.SellingPrice END,
        TaxAmount = ROUND(((i.Quantity * CASE WHEN i.UnitPrice > 0 THEN i.UnitPrice ELSE p.SellingPrice END) - i.ItemDiscountAmount) * (p.TaxRate / 100.0), 4)
    FROM @Items i
    JOIN dbo.Product p ON p.ProductId = i.ProductId;

    IF EXISTS (SELECT 1 FROM @Items i JOIN dbo.Product p ON p.ProductId=i.ProductId WHERE i.ItemDiscountAmount > 0 AND p.DiscountAllowed = 0) THROW 52109, 'Discount is not allowed for one or more products.', 1;
    IF EXISTS (SELECT 1 FROM @Items WHERE ItemDiscountAmount > Quantity * UnitPrice) THROW 52110, 'Item discount cannot exceed line subtotal.', 1;

    DECLARE @Subtotal DECIMAL(18,4) = (SELECT SUM(Quantity * UnitPrice) FROM @Items);
    DECLARE @ItemDiscount DECIMAL(18,4) = (SELECT SUM(ItemDiscountAmount) FROM @Items);
    DECLARE @LineTax DECIMAL(18,4) = (SELECT SUM(TaxAmount) FROM @Items);
    DECLARE @TotalTax DECIMAL(18,4) = @LineTax + ISNULL(@TaxAmount,0);
    DECLARE @TotalDiscount DECIMAL(18,4) = @ItemDiscount + ISNULL(@OrderDiscountAmount,0);
    DECLARE @Net DECIMAL(18,4) = @Subtotal - @TotalDiscount + @TotalTax;
    IF @UseCustomerCredit = 1
    BEGIN
        IF @CustomerId IS NULL THROW 52130, 'Customer credit payment requires a selected customer.', 1;
        IF @CustomerCreditAmount IS NULL OR @CustomerCreditAmount <= 0 SET @CustomerCreditAmount = CONVERT(DECIMAL(18,2), @Net);
        IF @CustomerCreditAmount <= 0 THROW 52131, 'Customer credit amount must be greater than zero.', 1;
        DECLARE @CustomerCreditPaymentMethodId INT = (SELECT TOP 1 PaymentMethodId FROM dbo.PaymentMethod WHERE PaymentMethodCode = N'CREDIT' AND IsActive = 1);
        IF @CustomerCreditPaymentMethodId IS NULL THROW 52132, 'Customer credit payment method is not configured.', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.Customer WHERE CustomerId = @CustomerId AND IsActive = 1) THROW 52133, 'Customer does not exist or is inactive.', 1;
        INSERT INTO @Payments (PaymentMethodId, PaymentAmount, ReferenceNo)
        VALUES (@CustomerCreditPaymentMethodId, @CustomerCreditAmount, NULL);
    END
    DECLARE @Paid DECIMAL(18,4) = (SELECT SUM(PaymentAmount) FROM @Payments);
    DECLARE @CashPaid DECIMAL(18,4) = (SELECT ISNULL(SUM(p.PaymentAmount),0) FROM @Payments p JOIN dbo.PaymentMethod pm ON pm.PaymentMethodId=p.PaymentMethodId WHERE pm.IsCash=1);
    DECLARE @Change DECIMAL(18,4) = CASE WHEN @Paid > @Net AND @CashPaid > 0 THEN @Paid - @Net ELSE 0 END;
    IF @Net < 0 THROW 52106, 'Net amount cannot be negative.', 1;
    IF @Paid < @Net THROW 52107, 'Total payment amount must be greater than or equal to net amount.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;
        IF EXISTS (SELECT 1 FROM @Items i JOIN dbo.Product p ON p.ProductId=i.ProductId AND p.IsStockTracked=1 LEFT JOIN dbo.InventoryStock s WITH (UPDLOCK,HOLDLOCK) ON s.ProductId=i.ProductId AND s.LocationId=i.LocationId WHERE ISNULL(s.CurrentStock,0) < i.Quantity) AND @AllowNegativeStock=0 THROW 52108, 'Insufficient stock.', 1;

        DECLARE @SalesHeaderId BIGINT, @SaleNo NVARCHAR(50) = CONCAT(N'SAL', FORMAT(SYSUTCDATETIME(),'yyyyMMddHHmmssfff'));
        INSERT INTO dbo.SalesHeader (SaleNo, CustomerId, CashierUserId, SubtotalAmount, ItemDiscountAmount, OrderDiscountAmount, TotalDiscountAmount, TaxAmount, NetAmount, PaidAmount, ChangeAmount, Status, Remark, CreatedByUserId)
        VALUES (@SaleNo, @CustomerId, @CashierUserId, @Subtotal, @ItemDiscount, ISNULL(@OrderDiscountAmount,0), @TotalDiscount, @TotalTax, @Net, @Paid, @Change, N'Completed', ISNULL(@Remark,N''), NULLIF(@CreatedByUserId,0));
        SET @SalesHeaderId = CONVERT(BIGINT, SCOPE_IDENTITY());

        IF @UseCustomerCredit = 1
        BEGIN
            DECLARE @CreditLimit DECIMAL(18,2), @UsedCreditBefore DECIMAL(18,2), @AvailableCredit DECIMAL(18,2), @CreditTermDays INT;
            SELECT @CreditLimit = CreditLimit, @UsedCreditBefore = CurrentOutstandingAmount, @AvailableCredit = AvailableCredit, @CreditTermDays = CreditTermDays
            FROM dbo.CustomerCredit WITH (UPDLOCK, HOLDLOCK)
            WHERE CustomerId = @CustomerId AND AllowCredit = 1 AND CreditStatus = N'Good';

            IF @CreditLimit IS NULL THROW 52134, 'Customer credit is not allowed.', 1;
            IF @AvailableCredit < @CustomerCreditAmount THROW 52135, 'Insufficient available customer credit.', 1;

            UPDATE dbo.CustomerCredit
            SET CurrentOutstandingAmount = CurrentOutstandingAmount + @CustomerCreditAmount,
                UpdatedDate = SYSUTCDATETIME(),
                UpdatedByUserId = @CreatedByUserId
            WHERE CustomerId = @CustomerId;

            INSERT dbo.CustomerCreditTransaction
            (
                CustomerId, SaleId, TransactionType, ReferenceType, ReferenceId, ReferenceNo, Amount,
                BalanceBefore, BalanceAfter, DueDate, Status, Remark, Description, CreatedByUserId
            )
            VALUES
            (
                @CustomerId, @SalesHeaderId, N'CreditUsed', N'Sale', @SalesHeaderId, @SaleNo, @CustomerCreditAmount,
                @UsedCreditBefore, @UsedCreditBefore + @CustomerCreditAmount, DATEADD(DAY, ISNULL(@CreditTermDays,0), CONVERT(date, SYSUTCDATETIME())),
                N'Unpaid', N'POS customer credit sale', N'Customer credit used for sale.', @CreatedByUserId
            );
        END

        INSERT INTO dbo.SalesItem (SalesHeaderId, ProductId, ProductCodeSnapshot, ProductNameSnapshot, BarcodeSnapshot, UnitId, UnitSymbolSnapshot, Quantity, UnitPrice, CostPriceSnapshot, ItemDiscountAmount, TaxAmount, LineSubtotal, LineTotal)
        SELECT @SalesHeaderId, p.ProductId, p.ProductCode, p.ProductName, p.Barcode, p.UnitId, u.UnitSymbol, i.Quantity, i.UnitPrice, p.CostPrice, i.ItemDiscountAmount, i.TaxAmount, i.Quantity*i.UnitPrice, i.Quantity*i.UnitPrice-i.ItemDiscountAmount+i.TaxAmount
        FROM @Items i JOIN dbo.Product p ON p.ProductId=i.ProductId JOIN dbo.ProductUnit u ON u.UnitId=p.UnitId;

        INSERT INTO dbo.SalesPayment (SalesHeaderId, PaymentMethodId, PaymentAmount, ReferenceNo, CreatedByUserId)
        SELECT @SalesHeaderId, PaymentMethodId, PaymentAmount, NULLIF(LTRIM(RTRIM(ReferenceNo)),N''), @CreatedByUserId FROM @Payments;

        DECLARE @ProductId INT, @LocationId INT, @Qty DECIMAL(18,4), @Cost DECIMAL(18,4);
        DECLARE @InventoryMovementResult TABLE (InventoryMovementId BIGINT);
        DECLARE sale_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT i.ProductId, i.LocationId, i.Quantity, p.CostPrice FROM @Items i JOIN dbo.Product p ON p.ProductId=i.ProductId WHERE p.IsStockTracked=1;
        OPEN sale_cursor; FETCH NEXT FROM sale_cursor INTO @ProductId, @LocationId, @Qty, @Cost;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            DELETE FROM @InventoryMovementResult;
            INSERT INTO @InventoryMovementResult (InventoryMovementId)
            EXEC dbo.spInventoryMovementCreate @ProductId=@ProductId, @LocationId=@LocationId, @MovementType=N'Sale', @Quantity=@Qty, @UnitCost=@Cost, @ReferenceType=N'Sale', @ReferenceId=@SalesHeaderId, @ReferenceNo=@SaleNo, @Reason=N'Sale completed', @AllowNegativeStock=@AllowNegativeStock, @CreatedByUserId=@CreatedByUserId;
            FETCH NEXT FROM sale_cursor INTO @ProductId, @LocationId, @Qty, @Cost;
        END
        CLOSE sale_cursor; DEALLOCATE sale_cursor;

        IF @HeldSaleHeaderId IS NOT NULL UPDATE dbo.HeldSaleHeader SET Status=N'Completed', UpdatedByUserId=@CreatedByUserId, UpdatedDate=SYSUTCDATETIME() WHERE HeldSaleHeaderId=@HeldSaleHeaderId AND Status IN (N'Held',N'Resumed');
        COMMIT TRANSACTION;
        SELECT CONVERT(BIGINT, @SalesHeaderId) AS SalesHeaderId, @SaleNo AS SaleNo, @Net AS NetAmount, @Paid AS PaidAmount, @Change AS ChangeAmount;
    END TRY
    BEGIN CATCH
        IF CURSOR_STATUS('local','sale_cursor') >= -1 BEGIN CLOSE sale_cursor; DEALLOCATE sale_cursor; END
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;

