CREATE PROCEDURE [dbo].[spCustomerCreditReceivePayment]
    @CustomerId INT,
    @Amount DECIMAL(18,2),
    @PaidDate DATETIME2=NULL,
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

    IF @Amount>@Outstanding THROW 51402,'Payment cannot exceed outstanding amount.',1;

    SET @BalanceAfter=@Outstanding-@Amount;

    BEGIN TRAN;

    UPDATE dbo.CustomerCredit
    SET CurrentOutstandingAmount=@BalanceAfter,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@CreatedByUserId
    WHERE CustomerId=@CustomerId;

    INSERT dbo.CustomerCreditTransaction(CustomerId,TransactionType,Amount,BalanceBefore,BalanceAfter,PaidDate,ReferenceType,ReferenceNo,Status,Remark,Description,CreatedByUserId)
    VALUES(@CustomerId,N'Payment',@Amount,@Outstanding,@BalanceAfter,ISNULL(@PaidDate,SYSDATETIME()),N'CreditRepayment',@ReferenceNo,N'Paid',@Remark,N'Customer credit repayment received.',@CreatedByUserId);

    INSERT dbo.CustomerAuditLog(CustomerId,ActionType,EntityName,EntityId,OldValue,NewValue,Remark,CreatedByUserId)
    VALUES(@CustomerId,N'CreditPaymentReceived',N'CustomerCredit',@CustomerId,CONCAT(N'Outstanding=',@Outstanding),CONCAT(N'Outstanding=',@BalanceAfter,N'; Payment=',@Amount),@Remark,@CreatedByUserId);

    COMMIT;
END
