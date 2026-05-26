CREATE PROCEDURE [dbo].[spCustomerCreditAdjust] @CustomerId INT,@AdjustmentType NVARCHAR(30),@Amount DECIMAL(18,2),@ReferenceNo NVARCHAR(100)=NULL,@Remark NVARCHAR(1000)=NULL,@CreatedByUserId INT AS
BEGIN
    SET XACT_ABORT ON; DECLARE @Outstanding DECIMAL(18,2); SELECT @Outstanding=CurrentOutstandingAmount FROM dbo.CustomerCredit WITH(UPDLOCK,ROWLOCK) WHERE CustomerId=@CustomerId;
    IF @AdjustmentType=N'AdjustmentOut' AND @Amount>@Outstanding THROW 51403,'Adjustment cannot make outstanding negative.',1;
    BEGIN TRAN; UPDATE dbo.CustomerCredit SET CurrentOutstandingAmount=CurrentOutstandingAmount+CASE WHEN @AdjustmentType=N'AdjustmentIn' THEN @Amount ELSE -@Amount END,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@CreatedByUserId WHERE CustomerId=@CustomerId;
    INSERT dbo.CustomerCreditTransaction(CustomerId,TransactionType,Amount,ReferenceNo,Status,Remark,CreatedByUserId) VALUES(@CustomerId,@AdjustmentType,@Amount,@ReferenceNo,N'Paid',@Remark,@CreatedByUserId); COMMIT;
END
