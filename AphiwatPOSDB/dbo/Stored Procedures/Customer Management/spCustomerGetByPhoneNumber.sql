CREATE PROCEDURE [dbo].[spCustomerGetByPhoneNumber] @PhoneNumber NVARCHAR(50) AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @CustomerId INT=(SELECT CustomerId FROM dbo.Customer WHERE PhoneNumber=@PhoneNumber);
    EXEC dbo.spCustomerGetById @CustomerId;
END
