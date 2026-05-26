CREATE PROCEDURE [dbo].[spInventoryDashboardGetStockStatusSummary]
    @LocationId INT = NULL,
    @CategoryId INT = NULL,
    @BrandId INT = NULL,
    @DateFrom DATETIME2(0) = NULL,
    @DateTo DATETIME2(0) = NULL,
    @GroupBy NVARCHAR(20) = N'Daily'
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH ProductStock AS
    (
        SELECT
            p.ProductId,
            p.MinimumStockLevel,
            p.CostPrice,
            CurrentQty = SUM(ISNULL(s.CurrentStock, 0))
        FROM [dbo].[Product] p
        LEFT JOIN [dbo].[InventoryStock] s ON s.ProductId = p.ProductId
        WHERE p.IsActive = 1
          AND p.IsStockTracked = 1
          AND (@LocationId IS NULL OR s.LocationId = @LocationId)
          AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
          AND (@BrandId IS NULL OR p.BrandId = @BrandId)
        GROUP BY p.ProductId, p.MinimumStockLevel, p.CostPrice
    ),
    StatusRows AS
    (
        SELECT
            StockStatus = CASE
                WHEN CurrentQty <= 0 THEN N'Out of Stock'
                WHEN CurrentQty <= MinimumStockLevel THEN N'Low Stock'
                ELSE N'Normal'
            END,
            CurrentQty,
            CostPrice
        FROM ProductStock
    )
    SELECT
        StockStatus,
        ProductCount = COUNT(1),
        TotalQty = SUM(CurrentQty),
        TotalValue = SUM(CurrentQty * CostPrice)
    FROM StatusRows
    GROUP BY StockStatus
    ORDER BY CASE StockStatus WHEN N'Out of Stock' THEN 1 WHEN N'Low Stock' THEN 2 ELSE 3 END;
END;
