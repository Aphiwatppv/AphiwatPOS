CREATE PROCEDURE [dbo].[spCustomerIsPhoneNumberExists] @PhoneNumber NVARCHAR(50),@ExcludeCustomerId INT=NULL AS
BEGIN SELECT CONVERT(BIT,CASE WHEN EXISTS(SELECT 1 FROM dbo.Customer WHERE PhoneNumber=@PhoneNumber AND (@ExcludeCustomerId IS NULL OR CustomerId<>@ExcludeCustomerId)) THEN 1 ELSE 0 END); END
