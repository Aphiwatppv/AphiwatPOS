CREATE PROCEDURE [dbo].[spInventoryDashboardGetValueByLocation]
    @LocationId INT = NULL,
    @CategoryId INT = NULL,
    @BrandId INT = NULL,
    @DateFrom DATETIME2(0) = NULL,
    @DateTo DATETIME2(0) = NULL,
    @GroupBy NVARCHAR(20) = N'Daily'
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        l.LocationId,
        l.LocationCode,
        l.LocationName,
        ProductCount = COUNT(DISTINCT p.ProductId),
        TotalQty = ISNULL(SUM(s.CurrentStock), 0),
        TotalValue = ISNULL(SUM(s.CurrentStock * p.CostPrice), 0)
    FROM [dbo].[InventoryLocation] l
    LEFT JOIN [dbo].[InventoryStock] s ON s.LocationId = l.LocationId
    LEFT JOIN [dbo].[Product] p ON p.ProductId = s.ProductId
    WHERE l.IsActive = 1
      AND (@LocationId IS NULL OR l.LocationId = @LocationId)
      AND (p.ProductId IS NULL OR p.IsActive = 1)
      AND (p.ProductId IS NULL OR p.IsStockTracked = 1)
      AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
      AND (@BrandId IS NULL OR p.BrandId = @BrandId)
    GROUP BY l.LocationId, l.LocationCode, l.LocationName
    ORDER BY TotalValue DESC, l.LocationName;
END;
