CREATE PROCEDURE [dbo].[spInventoryDashboardGetSummary]
    @LocationId INT = NULL,
    @CategoryId INT = NULL,
    @BrandId INT = NULL,
    @DateFrom DATETIME2(0) = NULL,
    @DateTo DATETIME2(0) = NULL,
    @GroupBy NVARCHAR(20) = N'Daily'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DateToExclusive DATETIME2(0) = CASE WHEN @DateTo IS NULL THEN NULL ELSE DATEADD(DAY, 1, CONVERT(DATE, @DateTo)) END;

    ;WITH StockRows AS
    (
        SELECT p.ProductId, p.IsStockTracked, p.MinimumStockLevel, p.CostPrice, s.CurrentStock
        FROM [dbo].[Product] p
        LEFT JOIN [dbo].[InventoryStock] s ON s.ProductId = p.ProductId
        WHERE p.IsActive = 1
          AND (@LocationId IS NULL OR s.LocationId = @LocationId)
          AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
          AND (@BrandId IS NULL OR p.BrandId = @BrandId)
    ),
    ProductRows AS
    (
        SELECT ProductId, IsStockTracked, MinimumStockLevel, CostPrice, SUM(ISNULL(CurrentStock, 0)) AS CurrentQty
        FROM StockRows
        GROUP BY ProductId, IsStockTracked, MinimumStockLevel, CostPrice
    ),
    MovementRows AS
    (
        SELECT m.QuantitySigned
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
        TotalProducts = (SELECT COUNT(1) FROM ProductRows),
        TrackedProducts = (SELECT COUNT(1) FROM ProductRows WHERE IsStockTracked = 1),
        ActiveLocations = (SELECT COUNT(1) FROM [dbo].[InventoryLocation] WHERE IsActive = 1 AND (@LocationId IS NULL OR LocationId = @LocationId)),
        TotalStockQty = ISNULL((SELECT SUM(CurrentQty) FROM ProductRows WHERE IsStockTracked = 1), 0),
        TotalStockValue = ISNULL((SELECT SUM(CurrentQty * CostPrice) FROM ProductRows WHERE IsStockTracked = 1), 0),
        LowStockProducts = (SELECT COUNT(1) FROM ProductRows WHERE IsStockTracked = 1 AND CurrentQty <= MinimumStockLevel AND CurrentQty > 0),
        OutOfStockProducts = (SELECT COUNT(1) FROM ProductRows WHERE IsStockTracked = 1 AND CurrentQty <= 0),
        MovementCount = (SELECT COUNT(1) FROM MovementRows),
        StockInQty = ISNULL((SELECT SUM(CASE WHEN QuantitySigned > 0 THEN QuantitySigned ELSE 0 END) FROM MovementRows), 0),
        StockOutQty = ISNULL((SELECT SUM(CASE WHEN QuantitySigned < 0 THEN ABS(QuantitySigned) ELSE 0 END) FROM MovementRows), 0),
        NetMovementQty = ISNULL((SELECT SUM(QuantitySigned) FROM MovementRows), 0),
        DraftAdjustments = (
            SELECT COUNT(1)
            FROM [dbo].[StockAdjustment] a
            WHERE a.Status = N'Draft'
              AND (@LocationId IS NULL OR a.LocationId = @LocationId)
              AND (@DateFrom IS NULL OR a.CreatedDate >= @DateFrom)
              AND (@DateToExclusive IS NULL OR a.CreatedDate < @DateToExclusive)
        ),
        OpenStockCounts = (
            SELECT COUNT(1)
            FROM [dbo].[StockCount] c
            WHERE c.Status = N'Draft'
              AND (@LocationId IS NULL OR c.LocationId = @LocationId)
              AND (@DateFrom IS NULL OR c.CreatedDate >= @DateFrom)
              AND (@DateToExclusive IS NULL OR c.CreatedDate < @DateToExclusive)
        ),
        OpenTransfers = (
            SELECT COUNT(1)
            FROM [dbo].[StockTransfer] t
            WHERE t.Status IN (N'Draft', N'Sent')
              AND (@LocationId IS NULL OR t.SourceLocationId = @LocationId OR t.DestinationLocationId = @LocationId)
              AND (@DateFrom IS NULL OR t.CreatedDate >= @DateFrom)
              AND (@DateToExclusive IS NULL OR t.CreatedDate < @DateToExclusive)
        );
END;
