CREATE PROCEDURE [dbo].[spSalesReturnComplete] @SalesReturnHeaderId BIGINT,@CompletedByUserId INT,@PaymentsJson NVARCHAR(MAX) AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @Payments TABLE (PaymentMethodId INT, RefundAmount DECIMAL(18,4), ReferenceNo NVARCHAR(100));
    INSERT INTO @Payments SELECT paymentMethodId, refundAmount, referenceNo FROM OPENJSON(@PaymentsJson) WITH (paymentMethodId INT '$.paymentMethodId', refundAmount DECIMAL(18,4) '$.refundAmount', referenceNo NVARCHAR(100) '$.referenceNo');
    IF EXISTS (SELECT 1 FROM @Payments WHERE RefundAmount<=0) THROW 52310, 'Refund payment amount must be greater than zero.', 1;
    IF EXISTS (SELECT 1 FROM @Payments p JOIN dbo.PaymentMethod pm ON pm.PaymentMethodId=p.PaymentMethodId WHERE pm.RequireReferenceNo=1 AND NULLIF(LTRIM(RTRIM(ISNULL(p.ReferenceNo,N''))),N'') IS NULL) THROW 52311, 'Refund reference number is required.', 1;
    DECLARE @RefundNet DECIMAL(18,4)=(SELECT RefundNetAmount FROM dbo.SalesReturnHeader WHERE SalesReturnHeaderId=@SalesReturnHeaderId);
    DECLARE @OriginalSalesHeaderId BIGINT, @ReturnCustomerId INT, @ReturnNoForCredit NVARCHAR(50);
    SELECT @OriginalSalesHeaderId=SalesHeaderId, @ReturnCustomerId=CustomerId, @ReturnNoForCredit=ReturnNo FROM dbo.SalesReturnHeader WHERE SalesReturnHeaderId=@SalesReturnHeaderId;
    DECLARE @CreditRefundPaymentMethodId INT=(SELECT TOP 1 PaymentMethodId FROM dbo.PaymentMethod WHERE PaymentMethodCode=N'CREDIT' AND IsActive=1);
    DECLARE @OriginalCreditPaid DECIMAL(18,4)=
    (
        SELECT ISNULL(SUM(sp.PaymentAmount),0)
        FROM dbo.SalesPayment sp
        JOIN dbo.PaymentMethod pm ON pm.PaymentMethodId=sp.PaymentMethodId
        WHERE sp.SalesHeaderId=@OriginalSalesHeaderId AND pm.PaymentMethodCode=N'CREDIT'
    );
    DECLARE @CreditRefundAmount DECIMAL(18,4)=CASE WHEN @OriginalCreditPaid>0 AND @RefundNet>0 THEN IIF(@OriginalCreditPaid>@RefundNet,@RefundNet,@OriginalCreditPaid) ELSE 0 END;
    IF @CreditRefundAmount>0 AND @CreditRefundPaymentMethodId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM @Payments WHERE PaymentMethodId=@CreditRefundPaymentMethodId)
        INSERT INTO @Payments(PaymentMethodId,RefundAmount,ReferenceNo) VALUES(@CreditRefundPaymentMethodId,@CreditRefundAmount,NULL);
    IF (SELECT ISNULL(SUM(RefundAmount),0) FROM @Payments) < @RefundNet THROW 52312, 'Refund payment total must cover refund net amount.', 1;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.SalesReturnHeader WHERE SalesReturnHeaderId=@SalesReturnHeaderId AND Status IN (N'Draft',N'Approved')) THROW 52313, 'Return cannot be completed.', 1;
        INSERT dbo.SalesReturnPayment (SalesReturnHeaderId, PaymentMethodId, RefundAmount, ReferenceNo, CreatedByUserId) SELECT @SalesReturnHeaderId, PaymentMethodId, RefundAmount, NULLIF(LTRIM(RTRIM(ReferenceNo)),N''), @CompletedByUserId FROM @Payments;
        UPDATE si SET ReturnedQty=si.ReturnedQty+ri.QuantityReturned FROM dbo.SalesItem si JOIN dbo.SalesReturnItem ri ON ri.SalesItemId=si.SalesItemId WHERE ri.SalesReturnHeaderId=@SalesReturnHeaderId;
        DECLARE @ReturnNo NVARCHAR(50), @SalesHeaderId BIGINT; SELECT @ReturnNo=ReturnNo,@SalesHeaderId=SalesHeaderId FROM dbo.SalesReturnHeader WHERE SalesReturnHeaderId=@SalesReturnHeaderId;
        DECLARE @ProductId INT,@Qty DECIMAL(18,4),@Cost DECIMAL(18,4);
        DECLARE r CURSOR LOCAL FAST_FORWARD FOR SELECT ri.ProductId, ri.QuantityReturned, si.CostPriceSnapshot FROM dbo.SalesReturnItem ri JOIN dbo.SalesItem si ON si.SalesItemId=ri.SalesItemId JOIN dbo.Product p ON p.ProductId=ri.ProductId WHERE ri.SalesReturnHeaderId=@SalesReturnHeaderId AND ri.ReturnToStock=1 AND ri.ReturnCondition=N'Good' AND p.IsStockTracked=1;
        OPEN r; FETCH NEXT FROM r INTO @ProductId,@Qty,@Cost;
        WHILE @@FETCH_STATUS=0 BEGIN DECLARE @LocationId INT=(SELECT TOP 1 LocationId FROM dbo.InventoryMovement WHERE ReferenceType=N'Sale' AND ReferenceId=@SalesHeaderId AND ProductId=@ProductId ORDER BY InventoryMovementId); EXEC dbo.spInventoryMovementCreate @ProductId=@ProductId,@LocationId=@LocationId,@MovementType=N'Return',@Quantity=@Qty,@UnitCost=@Cost,@ReferenceType=N'SalesReturn',@ReferenceId=@SalesReturnHeaderId,@ReferenceNo=@ReturnNo,@Reason=N'Sales return completed',@AllowNegativeStock=1,@CreatedByUserId=@CompletedByUserId; FETCH NEXT FROM r INTO @ProductId,@Qty,@Cost; END
        CLOSE r; DEALLOCATE r;
        UPDATE dbo.SalesReturnHeader SET Status=N'Completed', UpdatedByUserId=@CompletedByUserId, UpdatedDate=SYSUTCDATETIME() WHERE SalesReturnHeaderId=@SalesReturnHeaderId;
        UPDATE h SET Status=CASE WHEN NOT EXISTS (SELECT 1 FROM dbo.SalesItem WHERE SalesHeaderId=h.SalesHeaderId AND ReturnedQty < Quantity) THEN N'Refunded' ELSE N'PartiallyRefunded' END, UpdatedByUserId=@CompletedByUserId, UpdatedDate=SYSUTCDATETIME() FROM dbo.SalesHeader h WHERE h.SalesHeaderId=@SalesHeaderId;
        IF @CreditRefundAmount>0 AND @ReturnCustomerId IS NOT NULL
        BEGIN
            DECLARE @CreditBefore DECIMAL(18,2);
            SELECT @CreditBefore=CurrentOutstandingAmount FROM dbo.CustomerCredit WITH(UPDLOCK,HOLDLOCK) WHERE CustomerId=@ReturnCustomerId;
            IF @CreditBefore IS NOT NULL
            BEGIN
                UPDATE dbo.CustomerCredit SET CurrentOutstandingAmount=CASE WHEN CurrentOutstandingAmount<@CreditRefundAmount THEN 0 ELSE CurrentOutstandingAmount-@CreditRefundAmount END,UpdatedDate=SYSUTCDATETIME(),UpdatedByUserId=@CompletedByUserId WHERE CustomerId=@ReturnCustomerId;
                INSERT dbo.CustomerCreditTransaction(CustomerId,TransactionType,ReferenceType,ReferenceId,ReferenceNo,Amount,BalanceBefore,BalanceAfter,Status,Remark,Description,CreatedByUserId)
                VALUES(@ReturnCustomerId,N'CreditRefunded',N'SalesReturn',@SalesReturnHeaderId,@ReturnNo,@CreditRefundAmount,@CreditBefore,CASE WHEN @CreditBefore<@CreditRefundAmount THEN 0 ELSE @CreditBefore-@CreditRefundAmount END,N'Paid',N'Customer credit restored from refund.',N'Customer credit restored from refund.',@CompletedByUserId);
            END
        END
        COMMIT;
        SELECT r.*, h.SaleNo FROM dbo.SalesReturnHeader r JOIN dbo.SalesHeader h ON h.SalesHeaderId=r.SalesHeaderId WHERE r.SalesReturnHeaderId=@SalesReturnHeaderId;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH
END;

