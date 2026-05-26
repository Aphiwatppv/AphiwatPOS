CREATE PROCEDURE [dbo].[spCustomerIsEmailExists] @Email NVARCHAR(255),@ExcludeCustomerId INT=NULL AS
BEGIN SELECT CONVERT(BIT,CASE WHEN @Email IS NOT NULL AND EXISTS(SELECT 1 FROM dbo.Customer WHERE Email=@Email AND (@ExcludeCustomerId IS NULL OR CustomerId<>@ExcludeCustomerId)) THEN 1 ELSE 0 END); END
