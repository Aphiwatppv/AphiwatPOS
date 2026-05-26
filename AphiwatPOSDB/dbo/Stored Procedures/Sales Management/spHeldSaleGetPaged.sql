CREATE PROCEDURE [dbo].[spHeldSaleGetPaged] @PageNumber INT=1,@PageSize INT=20,@SearchText NVARCHAR(200)=NULL,@CashierUserId INT=NULL,@Status NVARCHAR(30)=NULL,@FromDate DATETIME2(0)=NULL,@ToDate DATETIME2(0)=NULL AS
BEGIN
    SET NOCOUNT ON;
    ;WITH q AS (SELECT h.*, c.CustomerName, u.DisplayName CashierName, COUNT(1) OVER() TotalCount FROM dbo.HeldSaleHeader h LEFT JOIN dbo.Customer c ON c.CustomerId=h.CustomerId JOIN dbo.AccessUser u ON u.UserId=h.CashierUserId WHERE (@SearchText IS NULL OR h.HeldSaleNo LIKE N'%'+@SearchText+N'%' OR c.CustomerName LIKE N'%'+@SearchText+N'%') AND (@CashierUserId IS NULL OR h.CashierUserId=@CashierUserId) AND (@Status IS NULL OR h.Status=@Status) AND (@FromDate IS NULL OR h.HeldDate>=@FromDate) AND (@ToDate IS NULL OR h.HeldDate<DATEADD(DAY,1,@ToDate)))
    SELECT * FROM q ORDER BY HeldDate DESC OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;

