CREATE PROCEDURE [dbo].[spPaymentMethodGetAllActive] AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM [dbo].[PaymentMethod] WHERE IsActive = 1 ORDER BY DisplayOrder, PaymentMethodName;
END;

