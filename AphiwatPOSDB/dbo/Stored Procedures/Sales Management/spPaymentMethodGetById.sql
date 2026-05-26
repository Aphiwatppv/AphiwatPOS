CREATE PROCEDURE [dbo].[spPaymentMethodGetById] @PaymentMethodId INT AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM [dbo].[PaymentMethod] WHERE PaymentMethodId = @PaymentMethodId;
END;

