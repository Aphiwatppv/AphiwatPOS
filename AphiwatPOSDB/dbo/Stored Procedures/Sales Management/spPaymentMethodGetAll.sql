CREATE PROCEDURE [dbo].[spPaymentMethodGetAll] AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM [dbo].[PaymentMethod] ORDER BY DisplayOrder, PaymentMethodName;
END;

