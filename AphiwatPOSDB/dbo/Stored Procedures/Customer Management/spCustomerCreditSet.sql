CREATE PROCEDURE [dbo].[spCustomerCreditSet]
    @CustomerId INT,
    @AllowCredit BIT,
    @CreditLimit DECIMAL(18,2),
    @CreditTermDays INT,
    @CreditStatus NVARCHAR(30),
    @RequireManagerApproval BIT,
    @ApprovedByUserId INT=NULL,
    @Remark NVARCHAR(1000)=NULL,
    @UpdatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @OldAllowCredit BIT,@OldCreditLimit DECIMAL(18,2),@OldCreditTermDays INT,@OldCreditStatus NVARCHAR(30),@OldRequireManagerApproval BIT;

    SELECT @OldAllowCredit=AllowCredit,@OldCreditLimit=CreditLimit,@OldCreditTermDays=CreditTermDays,@OldCreditStatus=CreditStatus,@OldRequireManagerApproval=RequireManagerApproval
    FROM dbo.CustomerCredit
    WHERE CustomerId=@CustomerId;

    UPDATE dbo.CustomerCredit
    SET AllowCredit=@AllowCredit,
        CreditLimit=CASE WHEN @AllowCredit=1 THEN @CreditLimit ELSE 0 END,
        CreditTermDays=CASE WHEN @AllowCredit=1 THEN @CreditTermDays ELSE 0 END,
        CreditStatus=@CreditStatus,
        RequireManagerApproval=@RequireManagerApproval,
        ApprovedByUserId=@ApprovedByUserId,
        ApprovedDate=CASE WHEN @ApprovedByUserId IS NULL THEN ApprovedDate ELSE SYSDATETIME() END,
        Remark=@Remark,
        UpdatedDate=SYSDATETIME(),
        UpdatedByUserId=@UpdatedByUserId
    WHERE CustomerId=@CustomerId;

    INSERT dbo.CustomerAuditLog(CustomerId,ActionType,EntityName,EntityId,OldValue,NewValue,Remark,CreatedByUserId)
    VALUES(@CustomerId,N'CreditSettingsUpdated',N'CustomerCredit',@CustomerId,
        CONCAT(N'AllowCredit=',@OldAllowCredit,N'; CreditLimit=',@OldCreditLimit,N'; CreditTermDays=',@OldCreditTermDays,N'; CreditStatus=',@OldCreditStatus,N'; RequireManagerApproval=',@OldRequireManagerApproval),
        CONCAT(N'AllowCredit=',@AllowCredit,N'; CreditLimit=',CASE WHEN @AllowCredit=1 THEN @CreditLimit ELSE 0 END,N'; CreditTermDays=',CASE WHEN @AllowCredit=1 THEN @CreditTermDays ELSE 0 END,N'; CreditStatus=',@CreditStatus,N'; RequireManagerApproval=',@RequireManagerApproval),
        @Remark,@UpdatedByUserId);
END
