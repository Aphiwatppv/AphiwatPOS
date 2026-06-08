CREATE PROCEDURE [dbo].[spSalesVatBillReportGet]
    @FromDate DATETIME2(0),
    @ToDate DATETIME2(0),
    @CashierUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH SaleScope AS
    (
        SELECT h.*, c.CustomerName, u.DisplayName AS CashierName
        FROM dbo.SalesHeader h
        LEFT JOIN dbo.Customer c ON c.CustomerId = h.CustomerId
        JOIN dbo.AccessUser u ON u.UserId = h.CashierUserId
        WHERE h.Status <> N'Voided'
          AND h.SaleDate >= @FromDate
          AND h.SaleDate < DATEADD(DAY, 1, @ToDate)
          AND (@CashierUserId IS NULL OR h.CashierUserId = @CashierUserId)
    ),
    ItemVat AS
    (
        SELECT
            s.SalesHeaderId,
            SUM(CASE
                WHEN ISNULL(p.VatMode, N'VatExcluded') = N'NoVat' THEN 0
                WHEN ISNULL(p.VatMode, N'VatExcluded') = N'VatIncluded' AND ISNULL(p.VatPercentage, 0) > 0 THEN si.Quantity * si.CostPriceSnapshot * (p.VatPercentage / (100.0 + p.VatPercentage))
                WHEN ISNULL(p.VatMode, N'VatExcluded') = N'VatIncluded' THEN 0
                ELSE si.Quantity * si.CostPriceSnapshot * (ISNULL(p.VatPercentage, 0) / 100.0)
            END) AS VatInAmount
        FROM SaleScope s
        JOIN dbo.SalesItem si ON si.SalesHeaderId = s.SalesHeaderId
        JOIN dbo.Product p ON p.ProductId = si.ProductId
        GROUP BY s.SalesHeaderId
    )
    SELECT
        s.SalesHeaderId,
        s.SaleNo,
        s.SaleDate,
        ISNULL(s.CustomerName, N'Walk-in') AS CustomerName,
        CAST(N'' AS NVARCHAR(50)) AS CustomerTaxId,
        s.CashierName,
        s.SubtotalAmount AS GrossAmount,
        s.TotalDiscountAmount AS DiscountAmount,
        CASE WHEN s.NetAmount >= s.TaxAmount THEN s.NetAmount - s.TaxAmount ELSE 0 END AS TaxableAmount,
        s.TaxAmount AS VatOutAmount,
        ISNULL(i.VatInAmount, 0) AS VatInAmount,
        s.TaxAmount - ISNULL(i.VatInAmount, 0) AS VatPayableAmount,
        s.NetAmount,
        s.Status
    FROM SaleScope s
    LEFT JOIN ItemVat i ON i.SalesHeaderId = s.SalesHeaderId
    ORDER BY s.SaleDate, s.SalesHeaderId;
END;
