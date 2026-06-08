CREATE PROCEDURE [dbo].[spSalesGetSummaryByDateRange] @FromDate DATETIME2(0), @ToDate DATETIME2(0), @CashierUserId INT=NULL AS
BEGIN
    SET NOCOUNT ON;
    ;WITH SaleScope AS
    (
        SELECT *
        FROM dbo.SalesHeader
        WHERE Status <> N'Voided'
          AND SaleDate >= @FromDate
          AND SaleDate < DATEADD(DAY, 1, @ToDate)
          AND (@CashierUserId IS NULL OR CashierUserId = @CashierUserId)
    ),
    HeaderSummary AS
    (
        SELECT
            CAST(SaleDate AS DATE) AS SaleDate,
            COUNT(1) AS TransactionCount,
            SUM(SubtotalAmount) AS GrossAmount,
            SUM(TotalDiscountAmount) AS DiscountAmount,
            SUM(TaxAmount) AS TaxAmount,
            SUM(NetAmount) AS NetAmount,
            SUM(OrderDiscountAmount) AS OrderDiscountAmount
        FROM SaleScope
        GROUP BY CAST(SaleDate AS DATE)
    ),
    ItemSummary AS
    (
        SELECT
            CAST(s.SaleDate AS DATE) AS SaleDate,
            SUM(si.Quantity * si.CostPriceSnapshot) AS CostOfGoodsSold,
            SUM(si.LineTotal - (si.Quantity * si.CostPriceSnapshot)) AS GrossProfitAmount,
            SUM(CASE
                WHEN ISNULL(p.VatMode, N'VatExcluded') = N'NoVat' THEN 0
                WHEN ISNULL(p.VatMode, N'VatExcluded') = N'VatIncluded' AND ISNULL(p.VatPercentage, 0) > 0 THEN si.Quantity * si.CostPriceSnapshot * (p.VatPercentage / (100.0 + p.VatPercentage))
                WHEN ISNULL(p.VatMode, N'VatExcluded') = N'VatIncluded' THEN 0
                ELSE si.Quantity * si.CostPriceSnapshot * (ISNULL(p.VatPercentage, 0) / 100.0)
            END) AS VatInAmount
        FROM SaleScope s
        JOIN dbo.SalesItem si ON si.SalesHeaderId = s.SalesHeaderId
        JOIN dbo.Product p ON p.ProductId = si.ProductId
        GROUP BY CAST(s.SaleDate AS DATE)
    ),
    RefundSummary AS
    (
        SELECT
            CAST(r.ReturnDate AS DATE) AS SaleDate,
            SUM(r.RefundNetAmount) AS RefundAmount
        FROM dbo.SalesReturnHeader r
        WHERE r.Status = N'Completed'
          AND r.ReturnDate >= @FromDate
          AND r.ReturnDate < DATEADD(DAY, 1, @ToDate)
          AND (@CashierUserId IS NULL OR r.CashierUserId = @CashierUserId)
        GROUP BY CAST(r.ReturnDate AS DATE)
    )
    SELECT
        h.SaleDate,
        h.TransactionCount,
        h.GrossAmount,
        h.DiscountAmount,
        h.TaxAmount,
        h.NetAmount,
        ISNULL(r.RefundAmount, 0) AS RefundAmount,
        ISNULL(i.CostOfGoodsSold, 0) AS CostOfGoodsSold,
        ISNULL(i.GrossProfitAmount, 0) - h.OrderDiscountAmount AS GrossProfitAmount,
        ISNULL(i.VatInAmount, 0) AS VatInAmount,
        h.TaxAmount AS VatOutAmount
    FROM HeaderSummary h
    LEFT JOIN ItemSummary i ON i.SaleDate = h.SaleDate
    LEFT JOIN RefundSummary r ON r.SaleDate = h.SaleDate
    ORDER BY h.SaleDate;
END;

