CREATE PROCEDURE [dbo].[spCashDrawerTransactionCreate]
    @SessionId BIGINT,
    @TransactionType NVARCHAR(30),
    @Amount DECIMAL(18,2),
    @Reason NVARCHAR(500) = N'',
    @ReferenceNo NVARCHAR(100) = N'',
    @SaleId BIGINT = NULL,
    @CreatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @Amount < 0 THROW 53104, 'Cash drawer transaction amount cannot be negative.', 1;
    IF NOT EXISTS (SELECT 1 FROM [dbo].[CashDrawerSession] WHERE [SessionId] = @SessionId AND [Status] = N'Open')
        THROW 53105, 'No active cash drawer session.', 1;

    INSERT INTO [dbo].[CashDrawerTransaction] ([SessionId], [TransactionType], [Amount], [Reason], [ReferenceNo], [SaleId], [CreatedByUserId])
    VALUES (@SessionId, @TransactionType, @Amount, ISNULL(@Reason,N''), ISNULL(@ReferenceNo,N''), @SaleId, @CreatedByUserId);

    UPDATE [dbo].[CashDrawerSession]
    SET [CashSales] = [CashSales] + CASE WHEN @TransactionType = N'CashSale' THEN @Amount ELSE 0 END,
        [CashIn] = [CashIn] + CASE WHEN @TransactionType = N'CashIn' THEN @Amount ELSE 0 END,
        [CashOut] = [CashOut] + CASE WHEN @TransactionType = N'CashOut' THEN @Amount ELSE 0 END,
        [CashRefund] = [CashRefund] + CASE WHEN @TransactionType = N'CashRefund' THEN @Amount ELSE 0 END
    WHERE [SessionId] = @SessionId;
END;
