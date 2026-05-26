CREATE PROCEDURE [dbo].[spPaymentMethodCreate]
    @PaymentMethodCode NVARCHAR(50), @PaymentMethodName NVARCHAR(100), @Description NVARCHAR(500) = N'', @RequireReferenceNo BIT = 0, @IsCash BIT = 0, @DisplayOrder INT = 0, @CreatedByUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.PaymentMethod WHERE PaymentMethodCode = @PaymentMethodCode) THROW 52000, 'Payment method code already exists.', 1;
    INSERT INTO dbo.PaymentMethod (PaymentMethodCode, PaymentMethodName, Description, RequireReferenceNo, IsCash, DisplayOrder, CreatedByUserId)
    VALUES (@PaymentMethodCode, @PaymentMethodName, ISNULL(@Description,N''), @RequireReferenceNo, @IsCash, @DisplayOrder, NULLIF(@CreatedByUserId,0));
    SELECT CONVERT(INT, SCOPE_IDENTITY());
END;

