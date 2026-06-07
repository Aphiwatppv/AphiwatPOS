CREATE PROCEDURE [dbo].[spCustomerCreate]
    @CustomerCode NVARCHAR(50)=NULL,
    @CustomerName NVARCHAR(255),
    @PhoneNumber NVARCHAR(50),
    @Email NVARCHAR(255)=NULL,
    @MemberType NVARCHAR(30)=N'Retail',
    @MemberLevelId INT=NULL,
    @DateOfBirth DATE=NULL,
    @Gender NVARCHAR(20)=NULL,
    @Address NVARCHAR(1000)=NULL,
    @CreatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @MemberType NOT IN (N'Retail',N'Wholesale') THROW 51004,'Member type must be Retail or Wholesale.',1;
    IF EXISTS(SELECT 1 FROM dbo.Customer WHERE PhoneNumber=@PhoneNumber) THROW 51000,'Phone number already exists.',1;
    IF @Email IS NOT NULL AND EXISTS(SELECT 1 FROM dbo.Customer WHERE Email=@Email) THROW 51001,'Email already exists.',1;

    BEGIN TRAN;

    IF NULLIF(LTRIM(RTRIM(@CustomerCode)),N'') IS NULL
        SET @CustomerCode=CONCAT(N'C',FORMAT(SYSDATETIME(),N'yyyyMMddHHmmss'),RIGHT(CONCAT(N'0000',ABS(CHECKSUM(NEWID()))%10000),4));

    IF EXISTS(SELECT 1 FROM dbo.Customer WHERE CustomerCode=@CustomerCode)
        SET @CustomerCode=CONCAT(@CustomerCode,N'-',CONVERT(NVARCHAR(10),ABS(CHECKSUM(NEWID()))%10000));

    INSERT dbo.Customer(CustomerCode,CustomerName,PhoneNumber,Email,MemberType,MemberLevelId,DateOfBirth,Gender,Address,CreatedByUserId)
    VALUES(@CustomerCode,@CustomerName,@PhoneNumber,@Email,@MemberType,@MemberLevelId,@DateOfBirth,@Gender,@Address,@CreatedByUserId);

    DECLARE @CustomerId INT=SCOPE_IDENTITY();

    INSERT dbo.CustomerPointBalance(CustomerId) VALUES(@CustomerId);

    INSERT dbo.CustomerCredit(CustomerId,AllowCredit,CreditLimit,CreditTermDays,CreditStatus,RequireManagerApproval,CreatedByUserId)
    SELECT @CustomerId,COALESCE(ml.AllowCredit,0),CASE WHEN COALESCE(ml.AllowCredit,0)=1 THEN ml.DefaultCreditLimit ELSE 0 END,CASE WHEN COALESCE(ml.AllowCredit,0)=1 THEN ml.DefaultCreditTermDays ELSE 0 END,N'Good',COALESCE(ml.RequireManagerApprovalForCredit,0),@CreatedByUserId
    FROM (SELECT 1 AS OneRow) d
    LEFT JOIN dbo.MemberLevel ml ON ml.MemberLevelId=@MemberLevelId;

    INSERT dbo.CustomerAuditLog(CustomerId,ActionType,EntityName,EntityId,NewValue,Remark,CreatedByUserId)
    VALUES(@CustomerId,N'CustomerCreated',N'Customer',@CustomerId,CONCAT(N'CustomerCode=',@CustomerCode,N'; MemberType=',@MemberType),N'Customer member profile created.',@CreatedByUserId);

    COMMIT;

    SELECT @CustomerId;
END
