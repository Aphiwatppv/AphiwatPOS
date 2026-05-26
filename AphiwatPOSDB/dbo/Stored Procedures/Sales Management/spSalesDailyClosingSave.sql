CREATE PROCEDURE [dbo].[spSalesDailyClosingSave]
    @ClosingDate DATE,
    @CashierUserId INT = NULL,
    @ActualCashAmount DECIMAL(18,4),
    @Notes NVARCHAR(1000) = N'',
    @ClosedByUserId INT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;

    IF @ActualCashAmount < 0 THROW 52400, 'Actual cash amount cannot be negative.', 1;
    IF @ClosedByUserId <= 0 THROW 52401, 'Closed by user is required.', 1;

    DECLARE @Summary TABLE
    (
        ClosingDate DATE,
        CashierUserId INT NULL,
        TransactionCount INT,
        GrossSalesAmount DECIMAL(18,4),
        DiscountAmount DECIMAL(18,4),
        TaxAmount DECIMAL(18,4),
        NetSalesAmount DECIMAL(18,4),
        RefundAmount DECIMAL(18,4),
        CashAmount DECIMAL(18,4),
        TransferAmount DECIMAL(18,4),
        CreditAmount DECIMAL(18,4),
        OtherPaymentAmount DECIMAL(18,4),
        ExpectedCashAmount DECIMAL(18,4),
        CostOfGoodsSold DECIMAL(18,4),
        GrossProfitAmount DECIMAL(18,4),
        StockMovementCount INT,
        StockMovementValue DECIMAL(18,4),
        DailySalesClosingId BIGINT NULL,
        ActualCashAmount DECIMAL(18,4) NULL,
        CashDifferenceAmount DECIMAL(18,4) NULL,
        Notes NVARCHAR(1000) NULL,
        ClosedByUserId INT NULL,
        ClosedAtUtc DATETIME2(0) NULL,
        ClosedByName NVARCHAR(100) NULL
    );

    INSERT INTO @Summary
    EXEC dbo.spSalesDailyClosingGet @ClosingDate = @ClosingDate, @CashierUserId = @CashierUserId;

    MERGE dbo.DailySalesClosing AS target
    USING
    (
        SELECT TOP 1
            ClosingDate,
            CashierUserId,
            GrossSalesAmount,
            DiscountAmount,
            TaxAmount,
            NetSalesAmount,
            RefundAmount,
            ExpectedCashAmount,
            GrossProfitAmount
        FROM @Summary
    ) AS source
    ON target.ClosingDate = source.ClosingDate AND ISNULL(target.CashierUserId, 0) = ISNULL(source.CashierUserId, 0)
    WHEN MATCHED THEN
        UPDATE SET
            GrossSalesAmount = source.GrossSalesAmount,
            DiscountAmount = source.DiscountAmount,
            TaxAmount = source.TaxAmount,
            NetSalesAmount = source.NetSalesAmount,
            RefundAmount = source.RefundAmount,
            ExpectedCashAmount = source.ExpectedCashAmount,
            ActualCashAmount = @ActualCashAmount,
            GrossProfitAmount = source.GrossProfitAmount,
            Notes = ISNULL(@Notes, N''),
            UpdatedByUserId = @ClosedByUserId,
            UpdatedAtUtc = SYSUTCDATETIME()
    WHEN NOT MATCHED THEN
        INSERT
        (
            ClosingDate, CashierUserId, GrossSalesAmount, DiscountAmount, TaxAmount, NetSalesAmount,
            RefundAmount, ExpectedCashAmount, ActualCashAmount, GrossProfitAmount, Notes, ClosedByUserId
        )
        VALUES
        (
            source.ClosingDate, source.CashierUserId, source.GrossSalesAmount, source.DiscountAmount, source.TaxAmount,
            source.NetSalesAmount, source.RefundAmount, source.ExpectedCashAmount, @ActualCashAmount,
            source.GrossProfitAmount, ISNULL(@Notes, N''), @ClosedByUserId
        );

    EXEC dbo.spSalesDailyClosingGet @ClosingDate = @ClosingDate, @CashierUserId = @CashierUserId;
END;

