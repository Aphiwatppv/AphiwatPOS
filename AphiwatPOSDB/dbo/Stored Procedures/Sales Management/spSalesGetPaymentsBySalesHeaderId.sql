CREATE PROCEDURE [dbo].[spSalesGetPaymentsBySalesHeaderId] @SalesHeaderId BIGINT AS BEGIN SET NOCOUNT ON; SELECT p.*, pm.PaymentMethodName FROM dbo.SalesPayment p JOIN dbo.PaymentMethod pm ON pm.PaymentMethodId=p.PaymentMethodId WHERE p.SalesHeaderId=@SalesHeaderId ORDER BY p.SalesPaymentId; END;

