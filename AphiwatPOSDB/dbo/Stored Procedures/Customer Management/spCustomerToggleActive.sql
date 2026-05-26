CREATE PROCEDURE [dbo].[spCustomerToggleActive] @CustomerId INT,@IsActive BIT,@UpdatedByUserId INT AS
BEGIN UPDATE dbo.Customer SET IsActive=@IsActive,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@UpdatedByUserId WHERE CustomerId=@CustomerId; END
