CREATE PROCEDURE [dbo].[spProductGetPaged] @PageNumber INT=1, @PageSize INT=20, @SearchText NVARCHAR(200)=NULL, @IsActive BIT=NULL, @CategoryId INT=NULL, @BrandId INT=NULL, @UnitId INT=NULL, @Status NVARCHAR(30)=NULL, @LowStockOnly BIT=0 AS
BEGIN SET NOCOUNT ON; SET @PageNumber=CASE WHEN @PageNumber<1 THEN 1 ELSE @PageNumber END; SET @PageSize=CASE WHEN @PageSize<1 THEN 20 ELSE @PageSize END;
    SELECT p.*, c.CategoryName, b.BrandName, u.UnitName, COUNT(1) OVER() AS TotalCount
    FROM [dbo].[Product] p
    INNER JOIN [dbo].[ProductCategory] c ON c.CategoryId=p.CategoryId
    LEFT JOIN [dbo].[ProductBrand] b ON b.BrandId=p.BrandId
    INNER JOIN [dbo].[ProductUnit] u ON u.UnitId=p.UnitId
    WHERE (@IsActive IS NULL OR p.IsActive=@IsActive)
      AND (@CategoryId IS NULL OR p.CategoryId=@CategoryId)
      AND (@BrandId IS NULL OR p.BrandId=@BrandId)
      AND (@UnitId IS NULL OR p.UnitId=@UnitId)
      AND (@Status IS NULL OR p.Status=@Status)
      AND (@LowStockOnly=0 OR (p.IsStockTracked=1 AND p.CurrentStock<=p.MinimumStockLevel))
      AND (@SearchText IS NULL OR p.ProductCode LIKE N'%'+@SearchText+N'%' OR p.ProductName LIKE N'%'+@SearchText+N'%' OR p.SKU LIKE N'%'+@SearchText+N'%' OR p.Barcode LIKE N'%'+@SearchText+N'%')
    ORDER BY p.ProductName OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
