CREATE PROCEDURE [dbo].[spCustomerCreditReceivePayment] @CustomerId INT,@Amount DECIMAL(18,2),@PaidDate DATETIME2=NULL,@ReferenceNo NVARCHAR(100)=NULL,@Remark NVARCHAR(1000)=NULL,@CreatedByUserId INT AS
BEGIN
    SET XACT_ABORT ON; DECLARE @Outstanding DECIMAL(18,2); SELECT @Outstanding=CurrentOutstandingAmount FROM dbo.CustomerCredit WITH(UPDLOCK,ROWLOCK) WHERE CustomerId=@CustomerId;
    IF @Amount>@Outstanding THROW 51402,'Payment cannot exceed outstanding amount.',1;
    BEGIN TRAN; UPDATE dbo.CustomerCredit SET CurrentOutstandingAmount=CurrentOutstandingAmount-@Amount,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@CreatedByUserId WHERE CustomerId=@CustomerId;
    INSERT dbo.CustomerCreditTransaction(CustomerId,TransactionType,Amount,PaidDate,ReferenceNo,Status,Remark,CreatedByUserId) VALUES(@CustomerId,N'Payment',@Amount,ISNULL(@PaidDate,SYSDATETIME()),@ReferenceNo,N'Paid',@Remark,@CreatedByUserId); COMMIT;
END
