CREATE PROCEDURE [dbo].[spSalesGetSummaryByDateRange] @FromDate DATETIME2(0), @ToDate DATETIME2(0), @CashierUserId INT=NULL AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(SaleDate AS DATE) SaleDate, COUNT(1) TransactionCount, SUM(SubtotalAmount) GrossAmount, SUM(TotalDiscountAmount) DiscountAmount, SUM(TaxAmount) TaxAmount, SUM(NetAmount) NetAmount,
           ISNULL((SELECT SUM(r.RefundNetAmount) FROM dbo.SalesReturnHeader r WHERE r.Status=N'Completed' AND r.ReturnDate>=@FromDate AND r.ReturnDate<DATEADD(DAY,1,@ToDate)),0) RefundAmount
    FROM dbo.SalesHeader
    WHERE Status <> N'Voided' AND SaleDate>=@FromDate AND SaleDate<DATEADD(DAY,1,@ToDate) AND (@CashierUserId IS NULL OR CashierUserId=@CashierUserId)
    GROUP BY CAST(SaleDate AS DATE) ORDER BY SaleDate;
END;

