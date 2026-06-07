SET NOCOUNT ON;

IF COL_LENGTH(N'dbo.Customer', N'MemberType') IS NULL
BEGIN
    ALTER TABLE dbo.Customer
    ADD MemberType NVARCHAR(30) NOT NULL CONSTRAINT DF_Customer_MemberType DEFAULT(N'Retail') WITH VALUES;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Customer_MemberType' AND parent_object_id = OBJECT_ID(N'dbo.Customer'))
BEGIN
    ALTER TABLE dbo.Customer WITH CHECK
    ADD CONSTRAINT CK_Customer_MemberType CHECK (MemberType IN (N'Retail', N'Wholesale'));
END;
GO

IF OBJECT_ID(N'dbo.CustomerAuditLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CustomerAuditLog
    (
        CustomerAuditLogId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerAuditLog PRIMARY KEY,
        CustomerId INT NULL,
        ActionType NVARCHAR(50) NOT NULL,
        EntityName NVARCHAR(100) NOT NULL,
        EntityId BIGINT NULL,
        OldValue NVARCHAR(MAX) NULL,
        NewValue NVARCHAR(MAX) NULL,
        Remark NVARCHAR(1000) NULL,
        CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_CustomerAuditLog_CreatedDate DEFAULT(SYSUTCDATETIME()),
        CreatedByUserId INT NOT NULL,
        CONSTRAINT FK_CustomerAuditLog_Customer FOREIGN KEY(CustomerId) REFERENCES dbo.Customer(CustomerId)
    );
END;
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerAuditLogGetPaged
    @CustomerId INT = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @ActionType NVARCHAR(50) = NULL,
    @DateFrom DATETIME2 = NULL,
    @DateTo DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CustomerAuditLogId,CustomerId,ActionType,EntityName,EntityId,OldValue,NewValue,Remark,CreatedDate,CreatedByUserId,COUNT(1) OVER() AS TotalCount
    FROM dbo.CustomerAuditLog
    WHERE (@CustomerId IS NULL OR CustomerId=@CustomerId)
      AND (@ActionType IS NULL OR ActionType=@ActionType)
      AND (@DateFrom IS NULL OR CreatedDate>=@DateFrom)
      AND (@DateTo IS NULL OR CreatedDate<DATEADD(DAY,1,@DateTo))
    ORDER BY CreatedDate DESC, CustomerAuditLogId DESC
    OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerGetPaged
    @PageNumber INT, @PageSize INT, @SearchText NVARCHAR(255)=NULL, @MemberType NVARCHAR(30)=NULL, @MemberLevelId INT=NULL, @IsActive BIT=NULL, @CreditStatus NVARCHAR(30)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH q AS
    (
        SELECT c.CustomerId,c.CustomerCode,c.CustomerName,c.PhoneNumber,c.Email,c.MemberType,ml.LevelName AS MemberLevelName,
               ISNULL(pb.AvailablePoints,0) AvailablePoints,ISNULL(cc.CreditLimit,0) CreditLimit,
               ISNULL(cc.CurrentOutstandingAmount,0) CurrentOutstandingAmount,ISNULL(cc.AvailableCredit,0) AvailableCredit,
               ISNULL(cc.CreditStatus,N'Good') CreditStatus,c.TotalSpending,c.TotalPurchaseCount,c.LastPurchaseDate,c.IsActive,
               COUNT(1) OVER() TotalCount
        FROM dbo.Customer c
        LEFT JOIN dbo.MemberLevel ml ON ml.MemberLevelId=c.MemberLevelId
        LEFT JOIN dbo.CustomerPointBalance pb ON pb.CustomerId=c.CustomerId
        LEFT JOIN dbo.CustomerCredit cc ON cc.CustomerId=c.CustomerId
        WHERE (@SearchText IS NULL OR c.CustomerCode LIKE N'%'+@SearchText+N'%' OR c.CustomerName LIKE N'%'+@SearchText+N'%' OR c.PhoneNumber LIKE N'%'+@SearchText+N'%' OR c.Email LIKE N'%'+@SearchText+N'%')
          AND (@MemberType IS NULL OR c.MemberType=@MemberType)
          AND (@MemberLevelId IS NULL OR c.MemberLevelId=@MemberLevelId)
          AND (@IsActive IS NULL OR c.IsActive=@IsActive)
          AND (@CreditStatus IS NULL OR cc.CreditStatus=@CreditStatus)
    )
    SELECT * FROM q ORDER BY CustomerName OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerCreate
    @CustomerCode NVARCHAR(50)=NULL,@CustomerName NVARCHAR(255),@PhoneNumber NVARCHAR(50),@Email NVARCHAR(255)=NULL,@MemberType NVARCHAR(30)=N'Retail',@MemberLevelId INT=NULL,
    @DateOfBirth DATE=NULL,@Gender NVARCHAR(20)=NULL,@Address NVARCHAR(1000)=NULL,@CreatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @MemberType NOT IN (N'Retail',N'Wholesale') THROW 51004,'Member type must be Retail or Wholesale.',1;
    IF EXISTS(SELECT 1 FROM dbo.Customer WHERE PhoneNumber=@PhoneNumber) THROW 51000,'Phone number already exists.',1;
    IF @Email IS NOT NULL AND EXISTS(SELECT 1 FROM dbo.Customer WHERE Email=@Email) THROW 51001,'Email already exists.',1;
    BEGIN TRAN;
    IF NULLIF(LTRIM(RTRIM(@CustomerCode)),N'') IS NULL SET @CustomerCode=CONCAT(N'C',FORMAT(SYSDATETIME(),N'yyyyMMddHHmmss'),RIGHT(CONCAT(N'0000',ABS(CHECKSUM(NEWID()))%10000),4));
    IF EXISTS(SELECT 1 FROM dbo.Customer WHERE CustomerCode=@CustomerCode) SET @CustomerCode=CONCAT(@CustomerCode,N'-',CONVERT(NVARCHAR(10),ABS(CHECKSUM(NEWID()))%10000));
    INSERT dbo.Customer(CustomerCode,CustomerName,PhoneNumber,Email,MemberType,MemberLevelId,DateOfBirth,Gender,Address,CreatedByUserId)
    VALUES(@CustomerCode,@CustomerName,@PhoneNumber,@Email,@MemberType,@MemberLevelId,@DateOfBirth,@Gender,@Address,@CreatedByUserId);
    DECLARE @CustomerId INT=SCOPE_IDENTITY();
    INSERT dbo.CustomerPointBalance(CustomerId) VALUES(@CustomerId);
    INSERT dbo.CustomerCredit(CustomerId,AllowCredit,CreditLimit,CreditTermDays,CreditStatus,RequireManagerApproval,CreatedByUserId)
    SELECT @CustomerId,COALESCE(ml.AllowCredit,0),CASE WHEN COALESCE(ml.AllowCredit,0)=1 THEN ml.DefaultCreditLimit ELSE 0 END,CASE WHEN COALESCE(ml.AllowCredit,0)=1 THEN ml.DefaultCreditTermDays ELSE 0 END,N'Good',COALESCE(ml.RequireManagerApprovalForCredit,0),@CreatedByUserId
    FROM (SELECT 1 AS OneRow) d LEFT JOIN dbo.MemberLevel ml ON ml.MemberLevelId=@MemberLevelId;
    INSERT dbo.CustomerAuditLog(CustomerId,ActionType,EntityName,EntityId,NewValue,Remark,CreatedByUserId)
    VALUES(@CustomerId,N'CustomerCreated',N'Customer',@CustomerId,CONCAT(N'CustomerCode=',@CustomerCode,N'; MemberType=',@MemberType),N'Customer member profile created.',@CreatedByUserId);
    COMMIT;
    SELECT @CustomerId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerUpdate
    @CustomerId INT,@CustomerName NVARCHAR(255),@PhoneNumber NVARCHAR(50),@Email NVARCHAR(255)=NULL,@MemberType NVARCHAR(30)=N'Retail',@MemberLevelId INT=NULL,@DateOfBirth DATE=NULL,
    @Gender NVARCHAR(20)=NULL,@Address NVARCHAR(1000)=NULL,@ApplyMemberLevelCreditDefault BIT=0,@UpdatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @MemberType NOT IN (N'Retail',N'Wholesale') THROW 51004,'Member type must be Retail or Wholesale.',1;
    IF EXISTS(SELECT 1 FROM dbo.Customer WHERE PhoneNumber=@PhoneNumber AND CustomerId<>@CustomerId) THROW 51002,'Phone number already exists.',1;
    IF @Email IS NOT NULL AND EXISTS(SELECT 1 FROM dbo.Customer WHERE Email=@Email AND CustomerId<>@CustomerId) THROW 51003,'Email already exists.',1;
    DECLARE @OldLevelId INT,@OldMemberType NVARCHAR(30),@OldName NVARCHAR(255),@OldPhone NVARCHAR(50),@OldEmail NVARCHAR(255);
    SELECT @OldLevelId=MemberLevelId,@OldMemberType=MemberType,@OldName=CustomerName,@OldPhone=PhoneNumber,@OldEmail=Email FROM dbo.Customer WHERE CustomerId=@CustomerId;
    BEGIN TRAN;
    UPDATE dbo.Customer SET CustomerName=@CustomerName,PhoneNumber=@PhoneNumber,Email=@Email,MemberType=@MemberType,MemberLevelId=@MemberLevelId,DateOfBirth=@DateOfBirth,Gender=@Gender,Address=@Address,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@UpdatedByUserId WHERE CustomerId=@CustomerId;
    IF ISNULL(@OldLevelId,-1)<>ISNULL(@MemberLevelId,-1) AND @MemberLevelId IS NOT NULL
        INSERT dbo.CustomerLevelHistory(CustomerId,OldMemberLevelId,NewMemberLevelId,ChangeReason,ChangedByUserId) VALUES(@CustomerId,@OldLevelId,@MemberLevelId,N'Member level changed from customer update.',@UpdatedByUserId);
    IF @ApplyMemberLevelCreditDefault=1 AND @MemberLevelId IS NOT NULL
        UPDATE cc SET AllowCredit=ml.AllowCredit,CreditLimit=CASE WHEN ml.AllowCredit=1 THEN ml.DefaultCreditLimit ELSE 0 END,CreditTermDays=CASE WHEN ml.AllowCredit=1 THEN ml.DefaultCreditTermDays ELSE 0 END,RequireManagerApproval=ml.RequireManagerApprovalForCredit,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@UpdatedByUserId
        FROM dbo.CustomerCredit cc JOIN dbo.MemberLevel ml ON ml.MemberLevelId=@MemberLevelId WHERE cc.CustomerId=@CustomerId;
    INSERT dbo.CustomerAuditLog(CustomerId,ActionType,EntityName,EntityId,OldValue,NewValue,Remark,CreatedByUserId)
    VALUES(@CustomerId,N'CustomerUpdated',N'Customer',@CustomerId,
        CONCAT(N'Name=',@OldName,N'; Phone=',@OldPhone,N'; Email=',ISNULL(@OldEmail,N''),N'; MemberType=',@OldMemberType,N'; MemberLevelId=',ISNULL(CONVERT(NVARCHAR(20),@OldLevelId),N'')),
        CONCAT(N'Name=',@CustomerName,N'; Phone=',@PhoneNumber,N'; Email=',ISNULL(@Email,N''),N'; MemberType=',@MemberType,N'; MemberLevelId=',ISNULL(CONVERT(NVARCHAR(20),@MemberLevelId),N'')),
        N'Customer member profile updated.',@UpdatedByUserId);
    COMMIT;
END;
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerToggleActive @CustomerId INT,@IsActive BIT,@UpdatedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @OldActive BIT=(SELECT IsActive FROM dbo.Customer WHERE CustomerId=@CustomerId);
    UPDATE dbo.Customer SET IsActive=@IsActive,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@UpdatedByUserId WHERE CustomerId=@CustomerId;
    IF ISNULL(@OldActive,0)<>@IsActive
        INSERT dbo.CustomerAuditLog(CustomerId,ActionType,EntityName,EntityId,OldValue,NewValue,Remark,CreatedByUserId)
        VALUES(@CustomerId,CASE WHEN @IsActive=1 THEN N'CustomerActivated' ELSE N'CustomerDeactivated' END,N'Customer',@CustomerId,CONVERT(NVARCHAR(10),@OldActive),CONVERT(NVARCHAR(10),@IsActive),N'Customer active status changed.',@UpdatedByUserId);
END;
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerCreditSet @CustomerId INT,@AllowCredit BIT,@CreditLimit DECIMAL(18,2),@CreditTermDays INT,@CreditStatus NVARCHAR(30),@RequireManagerApproval BIT,@ApprovedByUserId INT=NULL,@Remark NVARCHAR(1000)=NULL,@UpdatedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @OldAllowCredit BIT,@OldCreditLimit DECIMAL(18,2),@OldCreditTermDays INT,@OldCreditStatus NVARCHAR(30),@OldRequireManagerApproval BIT;
    SELECT @OldAllowCredit=AllowCredit,@OldCreditLimit=CreditLimit,@OldCreditTermDays=CreditTermDays,@OldCreditStatus=CreditStatus,@OldRequireManagerApproval=RequireManagerApproval FROM dbo.CustomerCredit WHERE CustomerId=@CustomerId;
    UPDATE dbo.CustomerCredit SET AllowCredit=@AllowCredit,CreditLimit=CASE WHEN @AllowCredit=1 THEN @CreditLimit ELSE 0 END,CreditTermDays=CASE WHEN @AllowCredit=1 THEN @CreditTermDays ELSE 0 END,CreditStatus=@CreditStatus,RequireManagerApproval=@RequireManagerApproval,ApprovedByUserId=@ApprovedByUserId,ApprovedDate=CASE WHEN @ApprovedByUserId IS NULL THEN ApprovedDate ELSE SYSDATETIME() END,Remark=@Remark,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@UpdatedByUserId WHERE CustomerId=@CustomerId;
    INSERT dbo.CustomerAuditLog(CustomerId,ActionType,EntityName,EntityId,OldValue,NewValue,Remark,CreatedByUserId)
    VALUES(@CustomerId,N'CreditSettingsUpdated',N'CustomerCredit',@CustomerId,CONCAT(N'AllowCredit=',@OldAllowCredit,N'; CreditLimit=',@OldCreditLimit,N'; CreditTermDays=',@OldCreditTermDays,N'; CreditStatus=',@OldCreditStatus,N'; RequireManagerApproval=',@OldRequireManagerApproval),CONCAT(N'AllowCredit=',@AllowCredit,N'; CreditLimit=',CASE WHEN @AllowCredit=1 THEN @CreditLimit ELSE 0 END,N'; CreditTermDays=',CASE WHEN @AllowCredit=1 THEN @CreditTermDays ELSE 0 END,N'; CreditStatus=',@CreditStatus,N'; RequireManagerApproval=',@RequireManagerApproval),@Remark,@UpdatedByUserId);
END;
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerReportGetSummary @DateFrom DATETIME2=NULL,@DateTo DATETIME2=NULL,@MemberType NVARCHAR(30)=NULL,@MemberLevelId INT=NULL,@IsActive BIT=NULL,@Top INT=20,@NoPurchaseAfterDate DATETIME2=NULL AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1) TotalCustomers,SUM(CASE WHEN c.IsActive=1 THEN 1 ELSE 0 END) ActiveCustomers,SUM(CASE WHEN c.MemberType=N'Retail' THEN 1 ELSE 0 END) RetailMemberCount,SUM(CASE WHEN c.MemberType=N'Wholesale' THEN 1 ELSE 0 END) WholesaleMemberCount,SUM(CASE WHEN (@DateFrom IS NOT NULL AND c.CreatedDate>=@DateFrom AND (@DateTo IS NULL OR c.CreatedDate<DATEADD(DAY,1,@DateTo))) THEN 1 ELSE 0 END) NewCustomers,
           SUM(c.TotalSpending) TotalCustomerSpending,ISNULL(SUM(cc.CurrentOutstandingAmount),0) TotalOutstandingCredit,ISNULL(SUM(pb.AvailablePoints),0) TotalAvailablePoints,
           SUM(CASE WHEN cc.AllowCredit=1 THEN 1 ELSE 0 END) TotalCreditCustomers,SUM(CASE WHEN overdue.CustomerId IS NULL THEN 0 ELSE 1 END) TotalOverdueCustomers
    FROM dbo.Customer c LEFT JOIN dbo.CustomerCredit cc ON cc.CustomerId=c.CustomerId LEFT JOIN dbo.CustomerPointBalance pb ON pb.CustomerId=c.CustomerId
    LEFT JOIN (SELECT DISTINCT CustomerId FROM dbo.CustomerCreditTransaction WHERE Status=N'Overdue') overdue ON overdue.CustomerId=c.CustomerId
    WHERE (@MemberType IS NULL OR c.MemberType=@MemberType) AND (@MemberLevelId IS NULL OR c.MemberLevelId=@MemberLevelId) AND (@IsActive IS NULL OR c.IsActive=@IsActive);
END;
GO
