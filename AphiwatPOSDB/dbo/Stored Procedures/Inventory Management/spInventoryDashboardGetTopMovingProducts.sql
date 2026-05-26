CREATE PROCEDURE [dbo].[spInventoryDashboardGetTopMovingProducts]
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

    DECLARE @DateToExclusive DATETIME2(0) = CASE WHEN @DateTo IS NULL THEN NULL ELSE DATEADD(DAY, 1, CONVERT(DATE, @DateTo)) END;

    SELECT TOP (@Top)
        p.ProductId,
        p.ProductCode,
        p.ProductName,
        c.CategoryName,
        BrandName = ISNULL(b.BrandName, N''),
        StockInQty = SUM(CASE WHEN m.QuantitySigned > 0 THEN m.QuantitySigned ELSE 0 END),
        StockOutQty = SUM(CASE WHEN m.QuantitySigned < 0 THEN ABS(m.QuantitySigned) ELSE 0 END),
        TotalMovedQty = SUM(ABS(m.QuantitySigned)),
        NetQty = SUM(m.QuantitySigned),
        MovementCount = COUNT(1)
    FROM [dbo].[InventoryMovement] m
    INNER JOIN [dbo].[Product] p ON p.ProductId = m.ProductId
    INNER JOIN [dbo].[ProductCategory] c ON c.CategoryId = p.CategoryId
    LEFT JOIN [dbo].[ProductBrand] b ON b.BrandId = p.BrandId
    WHERE m.IsActive = 1
      AND p.IsActive = 1
      AND (@LocationId IS NULL OR m.LocationId = @LocationId)
      AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
      AND (@BrandId IS NULL OR p.BrandId = @BrandId)
      AND (@DateFrom IS NULL OR m.CreatedDate >= @DateFrom)
      AND (@DateToExclusive IS NULL OR m.CreatedDate < @DateToExclusive)
    GROUP BY p.ProductId, p.ProductCode, p.ProductName, c.CategoryName, b.BrandName
    ORDER BY TotalMovedQty DESC, MovementCount DESC, p.ProductName;
END;
