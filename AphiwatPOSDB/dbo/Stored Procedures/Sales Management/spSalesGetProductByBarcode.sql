CREATE PROCEDURE [dbo].[spSalesGetProductByBarcode] @Barcode NVARCHAR(100), @LocationId INT AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (1) p.ProductId, p.ProductCode, p.ProductName, p.Barcode, p.UnitId, u.UnitSymbol, p.CostPrice, p.SellingPrice, p.WholesalePrice, p.WholesaleMinQty, p.TaxRate, p.DiscountAllowed, p.IsStockTracked, p.IsActive, p.Status, ISNULL(s.CurrentStock,0) CurrentStock
    FROM dbo.Product p
    JOIN dbo.ProductUnit u ON u.UnitId = p.UnitId
    LEFT JOIN dbo.InventoryStock s ON s.ProductId = p.ProductId AND s.LocationId = @LocationId
    WHERE p.Barcode = @Barcode AND p.IsActive = 1 AND p.Status = N'Active';
END;

