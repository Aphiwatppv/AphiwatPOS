CREATE PROCEDURE [dbo].[spCustomerUpdatePurchaseSummary] @CustomerId INT,@SaleAmount DECIMAL(18,2),@PurchaseDate DATETIME2 AS
BEGIN UPDATE dbo.Customer SET TotalSpending=TotalSpending+@SaleAmount,TotalPurchaseCount=TotalPurchaseCount+1,LastPurchaseDate=@PurchaseDate,UpdatedDate=SYSDATETIME() WHERE CustomerId=@CustomerId; END
