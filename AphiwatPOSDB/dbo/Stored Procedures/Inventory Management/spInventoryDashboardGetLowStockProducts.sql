CREATE PROCEDURE [dbo].[spInventoryDashboardGetLowStockProducts]
    @LocationId INT = NULL,
    @CategoryId INT = NULL,
    @BrandId INT = NULL,
    @DateFrom DATETIME2(0) = NULL,
    @DateTo DATETIME2(0) = NULL,
    @GroupBy NVARCHAR(20) = N'Daily',
    @Top INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    SET @Top = CASE WHEN @Top IS NULL OR @Top < 1 THEN 10 WHEN @Top > 100 THEN 100 ELSE @Top END;

    SELECT TOP (@Top)
        p.ProductId,
        p.ProductCode,
        p.SKU,
        Barcode = ISNULL(p.Barcode, N''),
        p.ProductName,
        c.CategoryName,
        BrandName = ISNULL(b.BrandName, N''),
        u.UnitName,
        u.UnitSymbol,
        l.LocationId,
        l.LocationName,
        CurrentQty = ISNULL(s.CurrentStock, 0),
        p.MinimumStockLevel,
        ShortageQty = CASE WHEN p.MinimumStockLevel - ISNULL(s.CurrentStock, 0) > 0 THEN p.MinimumStockLevel - ISNULL(s.CurrentStock, 0) ELSE 0 END,
        EstimatedReorderValue = CASE WHEN p.MinimumStockLevel - ISNULL(s.CurrentStock, 0) > 0 THEN (p.MinimumStockLevel - ISNULL(s.CurrentStock, 0)) * p.CostPrice ELSE 0 END,
        StockStatus = CASE WHEN ISNULL(s.CurrentStock, 0) <= 0 THEN N'Out of Stock' ELSE N'Low Stock' END,
        LastMovementDate = s.LastMovementDate
    FROM [dbo].[InventoryStock] s
    INNER JOIN [dbo].[Product] p ON p.ProductId = s.ProductId
    INNER JOIN [dbo].[ProductCategory] c ON c.CategoryId = p.CategoryId
    LEFT JOIN [dbo].[ProductBrand] b ON b.BrandId = p.BrandId
    INNER JOIN [dbo].[ProductUnit] u ON u.UnitId = p.UnitId
    INNER JOIN [dbo].[InventoryLocation] l ON l.LocationId = s.LocationId
    WHERE p.IsActive = 1
      AND p.IsStockTracked = 1
      AND s.CurrentStock <= p.MinimumStockLevel
      AND (@LocationId IS NULL OR s.LocationId = @LocationId)
      AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
      AND (@BrandId IS NULL OR p.BrandId = @BrandId)
    ORDER BY CASE WHEN s.CurrentStock <= 0 THEN 0 ELSE 1 END, ShortageQty DESC, p.ProductName;
END;
