CREATE PROCEDURE [dbo].[spCustomerPointGetBalance] @CustomerId INT AS BEGIN SELECT * FROM dbo.CustomerPointBalance WHERE CustomerId=@CustomerId; END
