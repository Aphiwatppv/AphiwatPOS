CREATE PROCEDURE [dbo].[spCustomerUpdate]
    @CustomerId INT,@CustomerName NVARCHAR(255),@PhoneNumber NVARCHAR(50),@Email NVARCHAR(255)=NULL,@MemberLevelId INT=NULL,@DateOfBirth DATE=NULL,
    @Gender NVARCHAR(20)=NULL,@Address NVARCHAR(1000)=NULL,@ApplyMemberLevelCreditDefault BIT=0,@UpdatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF EXISTS(SELECT 1 FROM dbo.Customer WHERE PhoneNumber=@PhoneNumber AND CustomerId<>@CustomerId) THROW 51002,'Phone number already exists.',1;
    IF @Email IS NOT NULL AND EXISTS(SELECT 1 FROM dbo.Customer WHERE Email=@Email AND CustomerId<>@CustomerId) THROW 51003,'Email already exists.',1;
    DECLARE @OldLevelId INT=(SELECT MemberLevelId FROM dbo.Customer WHERE CustomerId=@CustomerId);
    BEGIN TRAN;
    UPDATE dbo.Customer SET CustomerName=@CustomerName,PhoneNumber=@PhoneNumber,Email=@Email,MemberLevelId=@MemberLevelId,DateOfBirth=@DateOfBirth,Gender=@Gender,Address=@Address,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@UpdatedByUserId WHERE CustomerId=@CustomerId;
    IF ISNULL(@OldLevelId,-1)<>ISNULL(@MemberLevelId,-1) AND @MemberLevelId IS NOT NULL
        INSERT dbo.CustomerLevelHistory(CustomerId,OldMemberLevelId,NewMemberLevelId,ChangeReason,ChangedByUserId) VALUES(@CustomerId,@OldLevelId,@MemberLevelId,N'Member level changed from customer update.',@UpdatedByUserId);
    IF @ApplyMemberLevelCreditDefault=1 AND @MemberLevelId IS NOT NULL
        UPDATE cc SET AllowCredit=ml.AllowCredit,CreditLimit=CASE WHEN ml.AllowCredit=1 THEN ml.DefaultCreditLimit ELSE 0 END,CreditTermDays=CASE WHEN ml.AllowCredit=1 THEN ml.DefaultCreditTermDays ELSE 0 END,RequireManagerApproval=ml.RequireManagerApprovalForCredit,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@UpdatedByUserId
        FROM dbo.CustomerCredit cc JOIN dbo.MemberLevel ml ON ml.MemberLevelId=@MemberLevelId WHERE cc.CustomerId=@CustomerId;
    COMMIT;
END
