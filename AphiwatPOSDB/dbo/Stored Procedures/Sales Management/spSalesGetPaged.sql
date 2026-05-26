CREATE PROCEDURE [dbo].[spSalesGetPaged]
    @PageNumber INT=1, @PageSize INT=20, @SearchText NVARCHAR(200)=NULL, @CustomerId INT=NULL, @CashierUserId INT=NULL, @Status NVARCHAR(30)=NULL, @FromDate DATETIME2(0)=NULL, @ToDate DATETIME2(0)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH q AS (
        SELECT h.*, c.CustomerName, u.DisplayName CashierName, COUNT(1) OVER() TotalCount
        FROM dbo.SalesHeader h LEFT JOIN dbo.Customer c ON c.CustomerId=h.CustomerId JOIN dbo.AccessUser u ON u.UserId=h.CashierUserId
        WHERE (@SearchText IS NULL OR h.SaleNo LIKE N'%' + @SearchText + N'%' OR c.CustomerName LIKE N'%' + @SearchText + N'%')
          AND (@CustomerId IS NULL OR h.CustomerId=@CustomerId) AND (@CashierUserId IS NULL OR h.CashierUserId=@CashierUserId)
          AND (@Status IS NULL OR h.Status=@Status) AND (@FromDate IS NULL OR h.SaleDate>=@FromDate) AND (@ToDate IS NULL OR h.SaleDate<DATEADD(DAY,1,@ToDate))
    )
    SELECT * FROM q ORDER BY SaleDate DESC, SalesHeaderId DESC OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;

