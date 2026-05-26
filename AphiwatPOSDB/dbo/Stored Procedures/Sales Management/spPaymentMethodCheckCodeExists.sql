CREATE PROCEDURE [dbo].[spPaymentMethodCheckCodeExists] @PaymentMethodCode NVARCHAR(50), @ExcludePaymentMethodId INT = NULL AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.PaymentMethod WHERE PaymentMethodCode=@PaymentMethodCode AND (@ExcludePaymentMethodId IS NULL OR PaymentMethodId<>@ExcludePaymentMethodId)) THEN 1 ELSE 0 END AS BIT);
END;

