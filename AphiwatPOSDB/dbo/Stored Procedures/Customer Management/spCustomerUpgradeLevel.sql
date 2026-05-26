CREATE PROCEDURE [dbo].[spCustomerUpgradeLevel] @CustomerId INT,@ChangedByUserId INT,@ApplyMemberLevelCreditDefault BIT=1,@ManagerApproved BIT=0 AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @OldLevelId INT=(SELECT MemberLevelId FROM dbo.Customer WHERE CustomerId=@CustomerId);
    DECLARE @NewLevelId INT,@RequireApproval BIT;
    SELECT TOP(1) @NewLevelId=ToMemberLevelId,@RequireApproval=RequireManagerApproval FROM dbo.MemberLevelUpgradeRule WHERE FromMemberLevelId=@OldLevelId AND IsActive=1;
    IF @NewLevelId IS NULL THROW 51200,'No active upgrade rule found.',1;
    IF @RequireApproval=1 AND @ManagerApproved=0 THROW 51201,'Manager approval is required.',1;
    BEGIN TRAN;
    UPDATE dbo.Customer SET MemberLevelId=@NewLevelId,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@ChangedByUserId WHERE CustomerId=@CustomerId;
    INSERT dbo.CustomerLevelHistory(CustomerId,OldMemberLevelId,NewMemberLevelId,ChangeReason,ChangedByUserId) VALUES(@CustomerId,@OldLevelId,@NewLevelId,N'Automatic level upgrade.',@ChangedByUserId);
    IF @ApplyMemberLevelCreditDefault=1
        UPDATE cc SET AllowCredit=ml.AllowCredit,CreditLimit=CASE WHEN ml.AllowCredit=1 THEN ml.DefaultCreditLimit ELSE 0 END,CreditTermDays=CASE WHEN ml.AllowCredit=1 THEN ml.DefaultCreditTermDays ELSE 0 END,RequireManagerApproval=ml.RequireManagerApprovalForCredit,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@ChangedByUserId
        FROM dbo.CustomerCredit cc JOIN dbo.MemberLevel ml ON ml.MemberLevelId=@NewLevelId WHERE cc.CustomerId=@CustomerId;
    COMMIT;
END
