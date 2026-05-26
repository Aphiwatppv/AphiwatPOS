CREATE PROCEDURE [dbo].[spCustomerCreditGetByCustomerId] @CustomerId INT AS BEGIN SELECT * FROM dbo.CustomerCredit WHERE CustomerId=@CustomerId; END
