CREATE PROCEDURE [dbo].[spSalesReturnAddItem] @SalesReturnHeaderId BIGINT,@SalesItemId BIGINT,@QuantityReturned DECIMAL(18,4),@RefundUnitPrice DECIMAL(18,4),@ReturnToStock BIT,@ReturnCondition NVARCHAR(30),@Reason NVARCHAR(500) AS
BEGIN
    SET NOCOUNT ON;
    IF @QuantityReturned <= 0 THROW 52301, 'Refund quantity must be greater than zero.', 1;
    IF @ReturnToStock=1 AND @ReturnCondition<>N'Good' THROW 52302, 'Return to stock is allowed only for Good condition.', 1;
    IF EXISTS (SELECT 1 FROM dbo.SalesReturnHeader WHERE SalesReturnHeaderId=@SalesReturnHeaderId AND Status<>N'Draft') THROW 52303, 'Only draft returns can be edited.', 1;
    DECLARE @Sold DECIMAL(18,4), @Returned DECIMAL(18,4);
    SELECT @Sold=Quantity, @Returned=ReturnedQty FROM dbo.SalesItem WHERE SalesItemId=@SalesItemId;
    IF @QuantityReturned > (@Sold - @Returned - ISNULL((SELECT SUM(QuantityReturned) FROM dbo.SalesReturnItem WHERE SalesItemId=@SalesItemId AND SalesReturnHeaderId<>@SalesReturnHeaderId),0)) THROW 52304, 'Refund quantity exceeds remaining refundable quantity.', 1;
    INSERT dbo.SalesReturnItem (SalesReturnHeaderId, SalesItemId, ProductId, ProductCodeSnapshot, ProductNameSnapshot, BarcodeSnapshot, UnitId, UnitSymbolSnapshot, QuantityReturned, RefundUnitPrice, RefundAmount, ReturnToStock, ReturnCondition, Reason)
    SELECT @SalesReturnHeaderId, SalesItemId, ProductId, ProductCodeSnapshot, ProductNameSnapshot, BarcodeSnapshot, UnitId, UnitSymbolSnapshot, @QuantityReturned, @RefundUnitPrice, @QuantityReturned*@RefundUnitPrice, @ReturnToStock, @ReturnCondition, @Reason FROM dbo.SalesItem WHERE SalesItemId=@SalesItemId;
    UPDATE dbo.SalesReturnHeader SET RefundSubtotalAmount=(SELECT SUM(RefundAmount) FROM dbo.SalesReturnItem WHERE SalesReturnHeaderId=@SalesReturnHeaderId), RefundNetAmount=(SELECT SUM(RefundAmount) FROM dbo.SalesReturnItem WHERE SalesReturnHeaderId=@SalesReturnHeaderId), UpdatedDate=SYSUTCDATETIME() WHERE SalesReturnHeaderId=@SalesReturnHeaderId;
    SELECT CONVERT(BIGINT,SCOPE_IDENTITY());
END;

