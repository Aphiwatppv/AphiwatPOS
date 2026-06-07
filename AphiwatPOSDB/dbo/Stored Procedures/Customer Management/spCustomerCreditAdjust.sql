CREATE PROCEDURE [dbo].[spCustomerCreditAdjust]
    @CustomerId INT,
    @AdjustmentType NVARCHAR(30),
    @Amount DECIMAL(18,2),
    @ReferenceNo NVARCHAR(100)=NULL,
    @Remark NVARCHAR(1000)=NULL,
    @CreatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Outstanding DECIMAL(18,2);
    DECLARE @BalanceAfter DECIMAL(18,2);

    SELECT @Outstanding=CurrentOutstandingAmount
    FROM dbo.CustomerCredit WITH(UPDLOCK,ROWLOCK)
    WHERE CustomerId=@CustomerId;

    IF @AdjustmentType=N'AdjustmentOut' AND @Amount>@Outstanding THROW 51403,'Adjustment cannot make outstanding negative.',1;

    SET @BalanceAfter=@Outstanding+CASE WHEN @AdjustmentType=N'AdjustmentIn' THEN @Amount ELSE -@Amount END;

    BEGIN TRAN;

    UPDATE dbo.CustomerCredit
    SET CurrentOutstandingAmount=@BalanceAfter,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@CreatedByUserId
    WHERE CustomerId=@CustomerId;

    INSERT dbo.CustomerCreditTransaction(CustomerId,TransactionType,Amount,BalanceBefore,BalanceAfter,ReferenceType,ReferenceNo,Status,Remark,Description,CreatedByUserId)
    VALUES(@CustomerId,@AdjustmentType,@Amount,@Outstanding,@BalanceAfter,N'ManualAdjustment',@ReferenceNo,N'Paid',@Remark,N'Manual customer credit adjustment.',@CreatedByUserId);

    INSERT dbo.CustomerAuditLog(CustomerId,ActionType,EntityName,EntityId,OldValue,NewValue,Remark,CreatedByUserId)
    VALUES(@CustomerId,N'CreditAdjusted',N'CustomerCredit',@CustomerId,CONCAT(N'Outstanding=',@Outstanding),CONCAT(N'Outstanding=',@BalanceAfter,N'; AdjustmentType=',@AdjustmentType,N'; Amount=',@Amount),@Remark,@CreatedByUserId);

    COMMIT;
END
