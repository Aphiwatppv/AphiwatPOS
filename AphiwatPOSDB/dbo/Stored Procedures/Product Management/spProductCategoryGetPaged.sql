CREATE PROCEDURE [dbo].[spProductCategoryGetPaged]
    @PageNumber INT = 1, @PageSize INT = 20, @SearchText NVARCHAR(100) = NULL, @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET @PageNumber = CASE WHEN @PageNumber < 1 THEN 1 ELSE @PageNumber END;
    SET @PageSize = CASE WHEN @PageSize < 1 THEN 20 ELSE @PageSize END;

    SELECT *, COUNT(1) OVER() AS TotalCount
    FROM [dbo].[ProductCategory]
    WHERE (@IsActive IS NULL OR IsActive = @IsActive)
      AND (@SearchText IS NULL OR CategoryCode LIKE N'%' + @SearchText + N'%' OR CategoryName LIKE N'%' + @SearchText + N'%')
    ORDER BY DisplayOrder, CategoryName
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
