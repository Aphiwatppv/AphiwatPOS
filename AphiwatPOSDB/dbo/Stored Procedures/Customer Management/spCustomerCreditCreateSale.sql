CREATE PROCEDURE [dbo].[spCustomerCreditCreateSale] @CustomerId INT,@SaleId BIGINT,@Amount DECIMAL(18,2),@ReferenceNo NVARCHAR(100)=NULL,@ManagerApproved BIT=0,@Remark NVARCHAR(1000)=NULL,@CreatedByUserId INT AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @Available DECIMAL(18,2),@Require BIT,@Term INT;
    SELECT @Available=AvailableCredit,@Require=RequireManagerApproval,@Term=CreditTermDays FROM dbo.CustomerCredit WITH(UPDLOCK,ROWLOCK) WHERE CustomerId=@CustomerId AND AllowCredit=1 AND CreditStatus=N'Good';
    IF @Available IS NULL THROW 51400,'Credit is not allowed.',1;
    IF @Amount>@Available AND (@Require=0 OR @ManagerApproved=0) THROW 51401,'Credit sale exceeds available credit.',1;
    BEGIN TRAN;
    UPDATE dbo.CustomerCredit SET CurrentOutstandingAmount=CurrentOutstandingAmount+@Amount,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@CreatedByUserId WHERE CustomerId=@CustomerId;
    INSERT dbo.CustomerCreditTransaction(CustomerId,SaleId,TransactionType,Amount,DueDate,ReferenceNo,Status,Remark,CreatedByUserId) VALUES(@CustomerId,@SaleId,N'CreditSale',@Amount,DATEADD(DAY,@Term,CONVERT(date,SYSDATETIME())),@ReferenceNo,N'Unpaid',@Remark,@CreatedByUserId);
    DECLARE @Id BIGINT=SCOPE_IDENTITY(); COMMIT; SELECT @Id;
END
