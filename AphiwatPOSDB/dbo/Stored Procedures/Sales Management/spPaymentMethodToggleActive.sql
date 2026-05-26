CREATE PROCEDURE [dbo].[spPaymentMethodToggleActive] @PaymentMethodId INT, @IsActive BIT, @UpdatedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.PaymentMethod SET IsActive=@IsActive, UpdatedByUserId=NULLIF(@UpdatedByUserId,0), UpdatedDate=SYSUTCDATETIME() WHERE PaymentMethodId=@PaymentMethodId;
END;

