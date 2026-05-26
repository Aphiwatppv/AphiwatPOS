CREATE PROCEDURE [dbo].[spProductBrandGetPaged] @PageNumber INT = 1, @PageSize INT = 20, @SearchText NVARCHAR(100) = NULL, @IsActive BIT = NULL AS
BEGIN SET NOCOUNT ON; SET @PageNumber=CASE WHEN @PageNumber<1 THEN 1 ELSE @PageNumber END; SET @PageSize=CASE WHEN @PageSize<1 THEN 20 ELSE @PageSize END;
    SELECT *, COUNT(1) OVER() AS TotalCount FROM [dbo].[ProductBrand]
    WHERE (@IsActive IS NULL OR IsActive=@IsActive) AND (@SearchText IS NULL OR BrandCode LIKE N'%'+@SearchText+N'%' OR BrandName LIKE N'%'+@SearchText+N'%')
    ORDER BY BrandName OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
