CREATE PROCEDURE [dbo].[spSalesReturnGetPaymentsByReturnId] @SalesReturnHeaderId BIGINT AS BEGIN SET NOCOUNT ON; SELECT p.*, pm.PaymentMethodName FROM dbo.SalesReturnPayment p JOIN dbo.PaymentMethod pm ON pm.PaymentMethodId=p.PaymentMethodId WHERE p.SalesReturnHeaderId=@SalesReturnHeaderId ORDER BY SalesReturnPaymentId; END;

