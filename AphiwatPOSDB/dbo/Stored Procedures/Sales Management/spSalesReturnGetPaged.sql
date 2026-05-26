CREATE PROCEDURE [dbo].[spSalesReturnGetPaged] @PageNumber INT=1,@PageSize INT=20,@SearchText NVARCHAR(200)=NULL,@SalesHeaderId BIGINT=NULL,@CustomerId INT=NULL,@CashierUserId INT=NULL,@Status NVARCHAR(30)=NULL,@FromDate DATETIME2(0)=NULL,@ToDate DATETIME2(0)=NULL AS
BEGIN
    SET NOCOUNT ON;
    ;WITH q AS (SELECT r.*, h.SaleNo, COUNT(1) OVER() TotalCount FROM dbo.SalesReturnHeader r JOIN dbo.SalesHeader h ON h.SalesHeaderId=r.SalesHeaderId WHERE (@SearchText IS NULL OR r.ReturnNo LIKE N'%'+@SearchText+N'%' OR h.SaleNo LIKE N'%'+@SearchText+N'%') AND (@SalesHeaderId IS NULL OR r.SalesHeaderId=@SalesHeaderId) AND (@CustomerId IS NULL OR r.CustomerId=@CustomerId) AND (@CashierUserId IS NULL OR r.CashierUserId=@CashierUserId) AND (@Status IS NULL OR r.Status=@Status) AND (@FromDate IS NULL OR r.ReturnDate>=@FromDate) AND (@ToDate IS NULL OR r.ReturnDate<DATEADD(DAY,1,@ToDate)))
    SELECT * FROM q ORDER BY ReturnDate DESC OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;

