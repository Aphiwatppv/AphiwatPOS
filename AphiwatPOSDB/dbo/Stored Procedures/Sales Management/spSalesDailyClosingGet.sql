CREATE PROCEDURE [dbo].[spSalesDailyClosingGet]
    @ClosingDate DATE,
    @CashierUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartUtc DATETIME2(0) = CONVERT(DATETIME2(0), @ClosingDate);
    DECLARE @EndUtc DATETIME2(0) = DATEADD(DAY, 1, @StartUtc);

    ;WITH SaleScope AS
    (
        SELECT *
        FROM dbo.SalesHeader
        WHERE SaleDate >= @StartUtc
          AND SaleDate < @EndUtc
          AND Status <> N'Voided'
          AND (@CashierUserId IS NULL OR CashierUserId = @CashierUserId)
    ),
    PaymentSummary AS
    (
        SELECT
            SUM(CASE WHEN pm.PaymentMethodCode = N'CASH' OR pm.IsCash = 1 THEN sp.PaymentAmount ELSE 0 END) AS CashAmount,
            SUM(CASE WHEN pm.PaymentMethodCode IN (N'TRANSFER', N'QR') THEN sp.PaymentAmount ELSE 0 END) AS TransferAmount,
            SUM(CASE WHEN pm.PaymentMethodCode = N'CREDIT' THEN sp.PaymentAmount ELSE 0 END) AS CreditAmount,
            SUM(CASE WHEN pm.PaymentMethodCode NOT IN (N'CASH', N'TRANSFER', N'QR', N'CREDIT') AND pm.IsCash = 0 THEN sp.PaymentAmount ELSE 0 END) AS OtherPaymentAmount
        FROM SaleScope s
        JOIN dbo.SalesPayment sp ON sp.SalesHeaderId = s.SalesHeaderId
        JOIN dbo.PaymentMethod pm ON pm.PaymentMethodId = sp.PaymentMethodId
    ),
    ProfitSummary AS
    (
        SELECT
            SUM(si.LineTotal) AS SalesAfterItemDiscount,
            SUM(si.Quantity * si.CostPriceSnapshot) AS CostOfGoodsSold,
            SUM(si.LineTotal - (si.Quantity * si.CostPriceSnapshot)) AS GrossProfitAmount
        FROM SaleScope s
        JOIN dbo.SalesItem si ON si.SalesHeaderId = s.SalesHeaderId
    ),
    RefundSummary AS
    (
        SELECT
            SUM(rh.RefundNetAmount) AS RefundAmount
        FROM dbo.SalesReturnHeader rh
        WHERE rh.ReturnDate >= @StartUtc
          AND rh.ReturnDate < @EndUtc
          AND rh.Status = N'Completed'
          AND (@CashierUserId IS NULL OR rh.CashierUserId = @CashierUserId)
    ),
    StockSummary AS
    (
        SELECT
            COUNT(1) AS StockMovementCount,
            SUM(ABS(im.QuantitySigned) * im.UnitCost) AS StockMovementValue
        FROM dbo.InventoryMovement im
        WHERE im.CreatedDate >= @StartUtc
          AND im.CreatedDate < @EndUtc
          AND im.ReferenceType IN (N'Sale', N'SalesReturn', N'SaleVoid')
    )
    SELECT
        @ClosingDate AS ClosingDate,
        @CashierUserId AS CashierUserId,
        CAST(COUNT(s.SalesHeaderId) AS INT) AS TransactionCount,
        ISNULL(SUM(s.SubtotalAmount), 0) AS GrossSalesAmount,
        ISNULL(SUM(s.TotalDiscountAmount), 0) AS DiscountAmount,
        ISNULL(SUM(s.TaxAmount), 0) AS TaxAmount,
        ISNULL(SUM(s.NetAmount), 0) AS NetSalesAmount,
        ISNULL(MAX(r.RefundAmount), 0) AS RefundAmount,
        ISNULL(MAX(p.CashAmount), 0) AS CashAmount,
        ISNULL(MAX(p.TransferAmount), 0) AS TransferAmount,
        ISNULL(MAX(p.CreditAmount), 0) AS CreditAmount,
        ISNULL(MAX(p.OtherPaymentAmount), 0) AS OtherPaymentAmount,
        ISNULL(MAX(p.CashAmount), 0) - ISNULL(MAX(r.RefundAmount), 0) AS ExpectedCashAmount,
        ISNULL(MAX(ps.CostOfGoodsSold), 0) AS CostOfGoodsSold,
        ISNULL(MAX(ps.GrossProfitAmount), 0) - ISNULL(SUM(s.OrderDiscountAmount), 0) AS GrossProfitAmount,
        ISNULL(MAX(st.StockMovementCount), 0) AS StockMovementCount,
        ISNULL(MAX(st.StockMovementValue), 0) AS StockMovementValue,
        MAX(c.DailySalesClosingId) AS DailySalesClosingId,
        MAX(c.ActualCashAmount) AS ActualCashAmount,
        MAX(c.CashDifferenceAmount) AS CashDifferenceAmount,
        MAX(c.Notes) AS Notes,
        MAX(c.ClosedByUserId) AS ClosedByUserId,
        MAX(c.ClosedAtUtc) AS ClosedAtUtc,
        MAX(u.DisplayName) AS ClosedByName
    FROM SaleScope s
    CROSS JOIN PaymentSummary p
    CROSS JOIN ProfitSummary ps
    CROSS JOIN RefundSummary r
    CROSS JOIN StockSummary st
    LEFT JOIN dbo.DailySalesClosing c ON c.ClosingDate = @ClosingDate AND ISNULL(c.CashierUserId, 0) = ISNULL(@CashierUserId, 0)
    LEFT JOIN dbo.AccessUser u ON u.UserId = c.ClosedByUserId;
END;

