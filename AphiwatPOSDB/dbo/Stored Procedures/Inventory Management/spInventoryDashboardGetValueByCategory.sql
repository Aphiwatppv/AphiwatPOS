CREATE PROCEDURE [dbo].[spInventoryDashboardGetValueByCategory]
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
        c.CategoryId,
        c.CategoryCode,
        c.CategoryName,
        ProductCount = COUNT(DISTINCT p.ProductId),
        TotalQty = ISNULL(SUM(s.CurrentStock), 0),
        TotalValue = ISNULL(SUM(s.CurrentStock * p.CostPrice), 0)
    FROM [dbo].[ProductCategory] c
    INNER JOIN [dbo].[Product] p ON p.CategoryId = c.CategoryId
    LEFT JOIN [dbo].[InventoryStock] s ON s.ProductId = p.ProductId
    WHERE p.IsActive = 1
      AND p.IsStockTracked = 1
      AND (@LocationId IS NULL OR s.LocationId = @LocationId)
      AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
      AND (@BrandId IS NULL OR p.BrandId = @BrandId)
    GROUP BY c.CategoryId, c.CategoryCode, c.CategoryName
    ORDER BY TotalValue DESC, c.CategoryName;
END;
