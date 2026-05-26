CREATE PROCEDURE [dbo].[spSalesSearchProducts] @SearchText NVARCHAR(200) = NULL, @LocationId INT, @Top INT = 20 AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@Top) p.ProductId, p.ProductCode, p.ProductName, p.Barcode, p.UnitId, u.UnitSymbol, p.CostPrice, p.SellingPrice, p.WholesalePrice, p.WholesaleMinQty, p.TaxRate, p.DiscountAllowed, p.IsStockTracked, p.IsActive, p.Status, ISNULL(s.CurrentStock,0) CurrentStock
    FROM dbo.Product p
    JOIN dbo.ProductUnit u ON u.UnitId = p.UnitId
    LEFT JOIN dbo.InventoryStock s ON s.ProductId = p.ProductId AND s.LocationId = @LocationId
    WHERE p.IsActive = 1 AND p.Status = N'Active'
      AND (@SearchText IS NULL OR p.ProductCode LIKE N'%' + @SearchText + N'%' OR p.ProductName LIKE N'%' + @SearchText + N'%' OR p.Barcode LIKE N'%' + @SearchText + N'%')
    ORDER BY p.ProductName;
END;

