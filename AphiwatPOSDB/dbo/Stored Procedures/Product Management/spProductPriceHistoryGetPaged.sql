CREATE PROCEDURE [dbo].[spProductPriceHistoryGetPaged] @PageNumber INT=1, @PageSize INT=20, @SearchText NVARCHAR(200)=NULL, @ProductId INT=NULL, @FromDate DATETIME2(0)=NULL, @ToDate DATETIME2(0)=NULL AS
BEGIN SET NOCOUNT ON; SET @PageNumber=CASE WHEN @PageNumber<1 THEN 1 ELSE @PageNumber END; SET @PageSize=CASE WHEN @PageSize<1 THEN 20 ELSE @PageSize END;
    SELECT h.*, p.ProductCode, p.ProductName, COUNT(1) OVER() AS TotalCount
    FROM [dbo].[ProductPriceHistory] h INNER JOIN [dbo].[Product] p ON p.ProductId=h.ProductId
    WHERE (@ProductId IS NULL OR h.ProductId=@ProductId)
      AND (@FromDate IS NULL OR h.ChangedDate>=@FromDate)
      AND (@ToDate IS NULL OR h.ChangedDate<DATEADD(DAY,1,@ToDate))
      AND (@SearchText IS NULL OR p.ProductCode LIKE N'%'+@SearchText+N'%' OR p.ProductName LIKE N'%'+@SearchText+N'%' OR h.ChangeReason LIKE N'%'+@SearchText+N'%')
    ORDER BY h.ChangedDate DESC, h.ProductPriceHistoryId DESC OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
