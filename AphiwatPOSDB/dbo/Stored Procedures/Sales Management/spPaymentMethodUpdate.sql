CREATE PROCEDURE [dbo].[spPaymentMethodUpdate]
    @PaymentMethodId INT, @PaymentMethodCode NVARCHAR(50), @PaymentMethodName NVARCHAR(100), @Description NVARCHAR(500) = N'', @RequireReferenceNo BIT = 0, @IsCash BIT = 0, @IsActive BIT = 1, @DisplayOrder INT = 0, @UpdatedByUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.PaymentMethod WHERE PaymentMethodCode = @PaymentMethodCode AND PaymentMethodId <> @PaymentMethodId) THROW 52001, 'Payment method code already exists.', 1;
    UPDATE dbo.PaymentMethod SET PaymentMethodCode=@PaymentMethodCode, PaymentMethodName=@PaymentMethodName, Description=ISNULL(@Description,N''), RequireReferenceNo=@RequireReferenceNo, IsCash=@IsCash, IsActive=@IsActive, DisplayOrder=@DisplayOrder, UpdatedByUserId=NULLIF(@UpdatedByUserId,0), UpdatedDate=SYSUTCDATETIME()
    WHERE PaymentMethodId=@PaymentMethodId;
END;

