CREATE PROCEDURE [dbo].[spInventoryDashboardGetRecentMovements]
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
        m.InventoryMovementId,
        MovementDate = m.CreatedDate,
        p.ProductId,
        p.ProductCode,
        p.ProductName,
        c.CategoryName,
        BrandName = ISNULL(b.BrandName, N''),
        l.LocationId,
        l.LocationName,
        m.MovementType,
        m.Quantity,
        m.QuantitySigned,
        m.UnitCost,
        MovementValue = ABS(m.QuantitySigned) * m.UnitCost,
        m.ReferenceType,
        m.ReferenceNo,
        CreatedByName = ISNULL(u.DisplayName, N'')
    FROM [dbo].[InventoryMovement] m
    INNER JOIN [dbo].[Product] p ON p.ProductId = m.ProductId
    INNER JOIN [dbo].[ProductCategory] c ON c.CategoryId = p.CategoryId
    LEFT JOIN [dbo].[ProductBrand] b ON b.BrandId = p.BrandId
    INNER JOIN [dbo].[InventoryLocation] l ON l.LocationId = m.LocationId
    LEFT JOIN [dbo].[AccessUser] u ON u.UserId = m.CreatedByUserId
    WHERE m.IsActive = 1
      AND p.IsActive = 1
      AND (@LocationId IS NULL OR m.LocationId = @LocationId)
      AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
      AND (@BrandId IS NULL OR p.BrandId = @BrandId)
      AND (@DateFrom IS NULL OR m.CreatedDate >= @DateFrom)
      AND (@DateToExclusive IS NULL OR m.CreatedDate < @DateToExclusive)
    ORDER BY m.CreatedDate DESC, m.InventoryMovementId DESC;
END;
