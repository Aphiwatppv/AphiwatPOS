CREATE PROCEDURE [dbo].[spSalesGetById] @SalesHeaderId BIGINT AS
BEGIN
    SET NOCOUNT ON;
    SELECT h.*, c.CustomerName, u.DisplayName CashierName FROM dbo.SalesHeader h LEFT JOIN dbo.Customer c ON c.CustomerId=h.CustomerId JOIN dbo.AccessUser u ON u.UserId=h.CashierUserId WHERE h.SalesHeaderId=@SalesHeaderId;
END;

