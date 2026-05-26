CREATE PROCEDURE [dbo].[spInventoryDashboardGetMovementTrend]
    @LocationId INT = NULL,
    @CategoryId INT = NULL,
    @BrandId INT = NULL,
    @DateFrom DATETIME2(0) = NULL,
    @DateTo DATETIME2(0) = NULL,
    @GroupBy NVARCHAR(20) = N'Daily'
AS
BEGIN
    SET NOCOUNT ON;
    SET @GroupBy = CASE WHEN @GroupBy IN (N'Daily', N'Weekly', N'Monthly') THEN @GroupBy ELSE N'Daily' END;

    DECLARE @DateToExclusive DATETIME2(0) = CASE WHEN @DateTo IS NULL THEN NULL ELSE DATEADD(DAY, 1, CONVERT(DATE, @DateTo)) END;

    ;WITH MovementRows AS
    (
        SELECT
            PeriodStart = CASE
                WHEN @GroupBy = N'Monthly' THEN DATEFROMPARTS(YEAR(m.CreatedDate), MONTH(m.CreatedDate), 1)
                WHEN @GroupBy = N'Weekly' THEN DATEADD(WEEK, DATEDIFF(WEEK, 0, CONVERT(DATE, m.CreatedDate)), 0)
                ELSE CONVERT(DATE, m.CreatedDate)
            END,
            m.QuantitySigned
        FROM [dbo].[InventoryMovement] m
        INNER JOIN [dbo].[Product] p ON p.ProductId = m.ProductId
        WHERE m.IsActive = 1
          AND p.IsActive = 1
          AND (@LocationId IS NULL OR m.LocationId = @LocationId)
          AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
          AND (@BrandId IS NULL OR p.BrandId = @BrandId)
          AND (@DateFrom IS NULL OR m.CreatedDate >= @DateFrom)
          AND (@DateToExclusive IS NULL OR m.CreatedDate < @DateToExclusive)
    )
    SELECT
        PeriodStart,
        PeriodLabel = CONVERT(NVARCHAR(10), PeriodStart, 120),
        StockInQty = SUM(CASE WHEN QuantitySigned > 0 THEN QuantitySigned ELSE 0 END),
        StockOutQty = SUM(CASE WHEN QuantitySigned < 0 THEN ABS(QuantitySigned) ELSE 0 END),
        NetQty = SUM(QuantitySigned),
        MovementCount = COUNT(1)
    FROM MovementRows
    GROUP BY PeriodStart
    ORDER BY PeriodStart;
END;
