SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.CustomerNote', N'U') IS NULL
CREATE TABLE dbo.CustomerNote
(
    CustomerNoteId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerNote PRIMARY KEY,
    CustomerId INT NOT NULL,
    NoteType NVARCHAR(30) NOT NULL,
    NoteText NVARCHAR(2000) NOT NULL,
    IsImportant BIT NOT NULL CONSTRAINT DF_CustomerNote_IsImportant DEFAULT(0),
    IsActive BIT NOT NULL CONSTRAINT DF_CustomerNote_IsActive DEFAULT(1),
    CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_CustomerNote_CreatedDate DEFAULT(SYSDATETIME()),
    CreatedByUserId INT NOT NULL,
    UpdatedDate DATETIME2 NULL,
    UpdatedByUserId INT NULL,
    CONSTRAINT CK_CustomerNote_NoteType CHECK (NoteType IN (N'General',N'Warning',N'Credit',N'Service',N'Complaint',N'FollowUp',N'Other')),
    CONSTRAINT CK_CustomerNote_NoteText CHECK (LEN(LTRIM(RTRIM(NoteText))) > 0)
);
GO

IF OBJECT_ID(N'dbo.MemberLevel', N'U') IS NULL
CREATE TABLE dbo.MemberLevel
(
    MemberLevelId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MemberLevel PRIMARY KEY,
    LevelCode NVARCHAR(50) NOT NULL,
    LevelName NVARCHAR(255) NOT NULL,
    Description NVARCHAR(500) NULL,
    MinSpendingAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_MemberLevel_MinSpendingAmount DEFAULT(0),
    DiscountPercent DECIMAL(9,2) NOT NULL CONSTRAINT DF_MemberLevel_DiscountPercent DEFAULT(0),
    PointEarnAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_MemberLevel_PointEarnAmount DEFAULT(100),
    PointEarnPoint DECIMAL(18,2) NOT NULL CONSTRAINT DF_MemberLevel_PointEarnPoint DEFAULT(1),
    PointMultiplier DECIMAL(9,2) NOT NULL CONSTRAINT DF_MemberLevel_PointMultiplier DEFAULT(1),
    AllowCredit BIT NOT NULL CONSTRAINT DF_MemberLevel_AllowCredit DEFAULT(0),
    DefaultCreditLimit DECIMAL(18,2) NOT NULL CONSTRAINT DF_MemberLevel_DefaultCreditLimit DEFAULT(0),
    DefaultCreditTermDays INT NOT NULL CONSTRAINT DF_MemberLevel_DefaultCreditTermDays DEFAULT(0),
    RequireManagerApprovalForCredit BIT NOT NULL CONSTRAINT DF_MemberLevel_RequireManagerApprovalForCredit DEFAULT(0),
    MaxOverdueDaysAllowed INT NOT NULL CONSTRAINT DF_MemberLevel_MaxOverdueDaysAllowed DEFAULT(0),
    DisplayOrder INT NOT NULL CONSTRAINT DF_MemberLevel_DisplayOrder DEFAULT(0),
    IsActive BIT NOT NULL CONSTRAINT DF_MemberLevel_IsActive DEFAULT(1),
    CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_MemberLevel_CreatedDate DEFAULT(SYSDATETIME()),
    CreatedByUserId INT NOT NULL,
    UpdatedDate DATETIME2 NULL,
    UpdatedByUserId INT NULL,
    CONSTRAINT UQ_MemberLevel_LevelCode UNIQUE(LevelCode),
    CONSTRAINT CK_MemberLevel_DiscountPercent CHECK (DiscountPercent BETWEEN 0 AND 100),
    CONSTRAINT CK_MemberLevel_NonNegative CHECK (MinSpendingAmount >= 0 AND DefaultCreditLimit >= 0 AND DefaultCreditTermDays >= 0 AND MaxOverdueDaysAllowed >= 0),
    CONSTRAINT CK_MemberLevel_PointSettings CHECK (PointEarnAmount > 0 AND PointEarnPoint >= 0 AND PointMultiplier > 0),
    CONSTRAINT CK_MemberLevel_NoCreditDefaultsWhenDisabled CHECK (AllowCredit = 1 OR (DefaultCreditLimit = 0 AND DefaultCreditTermDays = 0))
);
GO

IF OBJECT_ID(N'dbo.Customer', N'U') IS NULL
CREATE TABLE dbo.Customer
(
    CustomerId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Customer PRIMARY KEY,
    CustomerCode NVARCHAR(50) NOT NULL,
    CustomerName NVARCHAR(255) NOT NULL,
    PhoneNumber NVARCHAR(50) NOT NULL,
    Email NVARCHAR(255) NULL,
    MemberLevelId INT NULL,
    DateOfBirth DATE NULL,
    Gender NVARCHAR(20) NULL,
    Address NVARCHAR(1000) NULL,
    TotalSpending DECIMAL(18,2) NOT NULL CONSTRAINT DF_Customer_TotalSpending DEFAULT(0),
    TotalPurchaseCount INT NOT NULL CONSTRAINT DF_Customer_TotalPurchaseCount DEFAULT(0),
    LastPurchaseDate DATETIME2 NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Customer_IsActive DEFAULT(1),
    CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_Customer_CreatedDate DEFAULT(SYSDATETIME()),
    CreatedByUserId INT NOT NULL,
    UpdatedDate DATETIME2 NULL,
    UpdatedByUserId INT NULL,
    CONSTRAINT UQ_Customer_CustomerCode UNIQUE(CustomerCode),
    CONSTRAINT UQ_Customer_PhoneNumber UNIQUE(PhoneNumber),
    CONSTRAINT FK_Customer_MemberLevel FOREIGN KEY(MemberLevelId) REFERENCES dbo.MemberLevel(MemberLevelId),
    CONSTRAINT CK_Customer_TotalValues CHECK (TotalSpending >= 0 AND TotalPurchaseCount >= 0),
    CONSTRAINT CK_Customer_Gender CHECK (Gender IS NULL OR Gender IN (N'Male',N'Female',N'Other',N'Unspecified'))
);
GO

IF OBJECT_ID(N'dbo.MemberLevelUpgradeRule', N'U') IS NULL
CREATE TABLE dbo.MemberLevelUpgradeRule
(
    MemberLevelUpgradeRuleId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MemberLevelUpgradeRule PRIMARY KEY,
    FromMemberLevelId INT NOT NULL,
    ToMemberLevelId INT NOT NULL,
    RequiredTotalSpending DECIMAL(18,2) NOT NULL CONSTRAINT DF_MemberLevelUpgradeRule_RequiredTotalSpending DEFAULT(0),
    RequiredPurchaseCount INT NOT NULL CONSTRAINT DF_MemberLevelUpgradeRule_RequiredPurchaseCount DEFAULT(0),
    RequiredMembershipDays INT NOT NULL CONSTRAINT DF_MemberLevelUpgradeRule_RequiredMembershipDays DEFAULT(0),
    RequireNoOverduePayment BIT NOT NULL CONSTRAINT DF_MemberLevelUpgradeRule_RequireNoOverduePayment DEFAULT(1),
    RequireManagerApproval BIT NOT NULL CONSTRAINT DF_MemberLevelUpgradeRule_RequireManagerApproval DEFAULT(0),
    IsActive BIT NOT NULL CONSTRAINT DF_MemberLevelUpgradeRule_IsActive DEFAULT(1),
    CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_MemberLevelUpgradeRule_CreatedDate DEFAULT(SYSDATETIME()),
    CreatedByUserId INT NOT NULL,
    UpdatedDate DATETIME2 NULL,
    UpdatedByUserId INT NULL,
    CONSTRAINT FK_MemberLevelUpgradeRule_From FOREIGN KEY(FromMemberLevelId) REFERENCES dbo.MemberLevel(MemberLevelId),
    CONSTRAINT FK_MemberLevelUpgradeRule_To FOREIGN KEY(ToMemberLevelId) REFERENCES dbo.MemberLevel(MemberLevelId),
    CONSTRAINT CK_MemberLevelUpgradeRule_DifferentLevels CHECK (FromMemberLevelId <> ToMemberLevelId),
    CONSTRAINT CK_MemberLevelUpgradeRule_NonNegative CHECK (RequiredTotalSpending >= 0 AND RequiredPurchaseCount >= 0 AND RequiredMembershipDays >= 0)
);
GO

IF OBJECT_ID(N'dbo.CustomerPointBalance', N'U') IS NULL
CREATE TABLE dbo.CustomerPointBalance
(
    CustomerPointBalanceId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerPointBalance PRIMARY KEY,
    CustomerId INT NOT NULL,
    AvailablePoints DECIMAL(18,2) NOT NULL CONSTRAINT DF_CustomerPointBalance_AvailablePoints DEFAULT(0),
    LifetimeEarnedPoints DECIMAL(18,2) NOT NULL CONSTRAINT DF_CustomerPointBalance_LifetimeEarnedPoints DEFAULT(0),
    LifetimeRedeemedPoints DECIMAL(18,2) NOT NULL CONSTRAINT DF_CustomerPointBalance_LifetimeRedeemedPoints DEFAULT(0),
    LastMovementDate DATETIME2 NULL,
    CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_CustomerPointBalance_CreatedDate DEFAULT(SYSDATETIME()),
    UpdatedDate DATETIME2 NULL,
    CONSTRAINT UQ_CustomerPointBalance_CustomerId UNIQUE(CustomerId),
    CONSTRAINT FK_CustomerPointBalance_Customer FOREIGN KEY(CustomerId) REFERENCES dbo.Customer(CustomerId),
    CONSTRAINT CK_CustomerPointBalance_NonNegative CHECK (AvailablePoints >= 0 AND LifetimeEarnedPoints >= 0 AND LifetimeRedeemedPoints >= 0)
);
GO

IF OBJECT_ID(N'dbo.CustomerPointMovement', N'U') IS NULL
CREATE TABLE dbo.CustomerPointMovement
(
    CustomerPointMovementId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerPointMovement PRIMARY KEY,
    CustomerId INT NOT NULL,
    MovementType NVARCHAR(30) NOT NULL,
    PointsIn DECIMAL(18,2) NOT NULL CONSTRAINT DF_CustomerPointMovement_PointsIn DEFAULT(0),
    PointsOut DECIMAL(18,2) NOT NULL CONSTRAINT DF_CustomerPointMovement_PointsOut DEFAULT(0),
    BalanceAfter DECIMAL(18,2) NOT NULL CONSTRAINT DF_CustomerPointMovement_BalanceAfter DEFAULT(0),
    ReferenceType NVARCHAR(50) NULL,
    ReferenceId BIGINT NULL,
    ReferenceNo NVARCHAR(100) NULL,
    ExpiryDate DATE NULL,
    Remark NVARCHAR(1000) NULL,
    CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_CustomerPointMovement_CreatedDate DEFAULT(SYSDATETIME()),
    CreatedByUserId INT NOT NULL,
    CONSTRAINT FK_CustomerPointMovement_Customer FOREIGN KEY(CustomerId) REFERENCES dbo.Customer(CustomerId),
    CONSTRAINT CK_CustomerPointMovement_Type CHECK (MovementType IN (N'Earn',N'Redeem',N'AdjustIn',N'AdjustOut',N'Expire',N'Reverse')),
    CONSTRAINT CK_CustomerPointMovement_Points CHECK (PointsIn >= 0 AND PointsOut >= 0 AND BalanceAfter >= 0 AND ((PointsIn > 0 AND PointsOut = 0) OR (PointsIn = 0 AND PointsOut > 0)))
);
GO

IF OBJECT_ID(N'dbo.CustomerCredit', N'U') IS NULL
CREATE TABLE dbo.CustomerCredit
(
    CustomerCreditId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerCredit PRIMARY KEY,
    CustomerId INT NOT NULL,
    AllowCredit BIT NOT NULL CONSTRAINT DF_CustomerCredit_AllowCredit DEFAULT(0),
    CreditLimit DECIMAL(18,2) NOT NULL CONSTRAINT DF_CustomerCredit_CreditLimit DEFAULT(0),
    CreditTermDays INT NOT NULL CONSTRAINT DF_CustomerCredit_CreditTermDays DEFAULT(0),
    CurrentOutstandingAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_CustomerCredit_CurrentOutstandingAmount DEFAULT(0),
    AvailableCredit AS (CreditLimit - CurrentOutstandingAmount) PERSISTED,
    CreditStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_CustomerCredit_CreditStatus DEFAULT(N'Good'),
    RequireManagerApproval BIT NOT NULL CONSTRAINT DF_CustomerCredit_RequireManagerApproval DEFAULT(0),
    ApprovedByUserId INT NULL,
    ApprovedDate DATETIME2 NULL,
    Remark NVARCHAR(1000) NULL,
    CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_CustomerCredit_CreatedDate DEFAULT(SYSDATETIME()),
    CreatedByUserId INT NOT NULL,
    UpdatedDate DATETIME2 NULL,
    UpdatedByUserId INT NULL,
    CONSTRAINT UQ_CustomerCredit_CustomerId UNIQUE(CustomerId),
    CONSTRAINT FK_CustomerCredit_Customer FOREIGN KEY(CustomerId) REFERENCES dbo.Customer(CustomerId),
    CONSTRAINT CK_CustomerCredit_Status CHECK (CreditStatus IN (N'Good',N'Hold',N'Blocked')),
    CONSTRAINT CK_CustomerCredit_NonNegative CHECK (CreditLimit >= 0 AND CreditTermDays >= 0 AND CurrentOutstandingAmount >= 0)
);
GO

IF OBJECT_ID(N'dbo.CustomerCreditTransaction', N'U') IS NULL
CREATE TABLE dbo.CustomerCreditTransaction
(
    CustomerCreditTransactionId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerCreditTransaction PRIMARY KEY,
    CustomerId INT NOT NULL,
    SaleId BIGINT NULL,
    TransactionType NVARCHAR(30) NOT NULL,
    ReferenceType NVARCHAR(30) NULL,
    ReferenceId BIGINT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    BalanceBefore DECIMAL(18,2) NOT NULL CONSTRAINT DF_CustomerCreditTransaction_BalanceBefore DEFAULT(0),
    BalanceAfter DECIMAL(18,2) NOT NULL CONSTRAINT DF_CustomerCreditTransaction_BalanceAfter DEFAULT(0),
    DueDate DATE NULL,
    PaidDate DATETIME2 NULL,
    ReferenceNo NVARCHAR(100) NULL,
    Status NVARCHAR(30) NOT NULL,
    Remark NVARCHAR(1000) NULL,
    Description NVARCHAR(500) NULL,
    CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_CustomerCreditTransaction_CreatedDate DEFAULT(SYSDATETIME()),
    CreatedByUserId INT NOT NULL,
    CONSTRAINT FK_CustomerCreditTransaction_Customer FOREIGN KEY(CustomerId) REFERENCES dbo.Customer(CustomerId),
    CONSTRAINT CK_CustomerCreditTransaction_Type CHECK (TransactionType IN (N'CreditSale',N'Payment',N'AdjustmentIn',N'AdjustmentOut',N'Cancel',N'Refund',N'CreditUsed',N'CreditRepaid',N'CreditRefunded',N'CreditAdjusted',N'CreditAdded')),
    CONSTRAINT CK_CustomerCreditTransaction_ReferenceType CHECK (ReferenceType IS NULL OR ReferenceType IN (N'Sale',N'CreditRepayment',N'SalesReturn',N'ManualAdjustment')),
    CONSTRAINT CK_CustomerCreditTransaction_Status CHECK (Status IN (N'Unpaid',N'PartiallyPaid',N'Paid',N'Overdue',N'Cancelled')),
    CONSTRAINT CK_CustomerCreditTransaction_Amount CHECK (Amount > 0)
);
GO

IF OBJECT_ID(N'dbo.CustomerCreditRepayment', N'U') IS NULL
CREATE TABLE dbo.CustomerCreditRepayment
(
    CustomerCreditRepaymentId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerCreditRepayment PRIMARY KEY,
    RepaymentNo NVARCHAR(50) NOT NULL,
    CustomerId INT NOT NULL,
    RepaymentDate DATETIME2(0) NOT NULL CONSTRAINT DF_CustomerCreditRepayment_RepaymentDate DEFAULT(SYSUTCDATETIME()),
    PaymentMethodId INT NOT NULL,
    PaymentAmount DECIMAL(18,2) NOT NULL,
    ReferenceNo NVARCHAR(100) NULL,
    Remark NVARCHAR(500) NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_CustomerCreditRepayment_Status DEFAULT(N'Completed'),
    CreatedByUserId INT NOT NULL,
    UpdatedByUserId INT NULL,
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_CustomerCreditRepayment_CreatedDate DEFAULT(SYSUTCDATETIME()),
    UpdatedDate DATETIME2(0) NULL,
    CONSTRAINT UQ_CustomerCreditRepayment_RepaymentNo UNIQUE(RepaymentNo),
    CONSTRAINT FK_CustomerCreditRepayment_Customer FOREIGN KEY(CustomerId) REFERENCES dbo.Customer(CustomerId),
    CONSTRAINT CK_CustomerCreditRepayment_Status CHECK (Status IN (N'Completed',N'Cancelled',N'Voided')),
    CONSTRAINT CK_CustomerCreditRepayment_Amount CHECK (PaymentAmount > 0)
);
GO

IF OBJECT_ID(N'dbo.CustomerLevelHistory', N'U') IS NULL
CREATE TABLE dbo.CustomerLevelHistory
(
    CustomerLevelHistoryId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerLevelHistory PRIMARY KEY,
    CustomerId INT NOT NULL,
    OldMemberLevelId INT NULL,
    NewMemberLevelId INT NOT NULL,
    ChangeReason NVARCHAR(500) NULL,
    ChangedDate DATETIME2 NOT NULL CONSTRAINT DF_CustomerLevelHistory_ChangedDate DEFAULT(SYSDATETIME()),
    ChangedByUserId INT NOT NULL,
    CONSTRAINT FK_CustomerLevelHistory_Customer FOREIGN KEY(CustomerId) REFERENCES dbo.Customer(CustomerId),
    CONSTRAINT FK_CustomerLevelHistory_OldLevel FOREIGN KEY(OldMemberLevelId) REFERENCES dbo.MemberLevel(MemberLevelId),
    CONSTRAINT FK_CustomerLevelHistory_NewLevel FOREIGN KEY(NewMemberLevelId) REFERENCES dbo.MemberLevel(MemberLevelId)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Customer_Email_NotNull' AND object_id = OBJECT_ID(N'dbo.Customer'))
CREATE UNIQUE INDEX UX_Customer_Email_NotNull ON dbo.Customer(Email) WHERE Email IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CustomerNote_Customer')
ALTER TABLE dbo.CustomerNote ADD CONSTRAINT FK_CustomerNote_Customer FOREIGN KEY(CustomerId) REFERENCES dbo.Customer(CustomerId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Customer_MemberLevel_IsActive' AND object_id = OBJECT_ID(N'dbo.Customer'))
CREATE INDEX IX_Customer_MemberLevel_IsActive ON dbo.Customer(MemberLevelId, IsActive);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_MemberLevelUpgradeRule_ActiveFrom' AND object_id = OBJECT_ID(N'dbo.MemberLevelUpgradeRule'))
CREATE UNIQUE INDEX UX_MemberLevelUpgradeRule_ActiveFrom ON dbo.MemberLevelUpgradeRule(FromMemberLevelId) WHERE IsActive = 1;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CustomerPointMovement_Customer_Date' AND object_id = OBJECT_ID(N'dbo.CustomerPointMovement'))
CREATE INDEX IX_CustomerPointMovement_Customer_Date ON dbo.CustomerPointMovement(CustomerId, CreatedDate DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CustomerCreditTransaction_Customer_Date' AND object_id = OBJECT_ID(N'dbo.CustomerCreditTransaction'))
CREATE INDEX IX_CustomerCreditTransaction_Customer_Date ON dbo.CustomerCreditTransaction(CustomerId, CreatedDate DESC);
IF COL_LENGTH('dbo.CustomerCreditTransaction','ReferenceType') IS NULL ALTER TABLE dbo.CustomerCreditTransaction ADD ReferenceType NVARCHAR(30) NULL;
IF COL_LENGTH('dbo.CustomerCreditTransaction','ReferenceId') IS NULL ALTER TABLE dbo.CustomerCreditTransaction ADD ReferenceId BIGINT NULL;
IF COL_LENGTH('dbo.CustomerCreditTransaction','BalanceBefore') IS NULL ALTER TABLE dbo.CustomerCreditTransaction ADD BalanceBefore DECIMAL(18,2) NOT NULL CONSTRAINT DF_CustomerCreditTransaction_BalanceBefore DEFAULT(0);
IF COL_LENGTH('dbo.CustomerCreditTransaction','BalanceAfter') IS NULL ALTER TABLE dbo.CustomerCreditTransaction ADD BalanceAfter DECIMAL(18,2) NOT NULL CONSTRAINT DF_CustomerCreditTransaction_BalanceAfter DEFAULT(0);
IF COL_LENGTH('dbo.CustomerCreditTransaction','Description') IS NULL ALTER TABLE dbo.CustomerCreditTransaction ADD Description NVARCHAR(500) NULL;
IF OBJECT_ID(N'dbo.CK_CustomerCreditTransaction_Type', N'C') IS NOT NULL ALTER TABLE dbo.CustomerCreditTransaction DROP CONSTRAINT CK_CustomerCreditTransaction_Type;
ALTER TABLE dbo.CustomerCreditTransaction ADD CONSTRAINT CK_CustomerCreditTransaction_Type CHECK (TransactionType IN (N'CreditSale',N'Payment',N'AdjustmentIn',N'AdjustmentOut',N'Cancel',N'Refund',N'CreditUsed',N'CreditRepaid',N'CreditRefunded',N'CreditAdjusted',N'CreditAdded'));
IF OBJECT_ID(N'dbo.CK_CustomerCreditTransaction_ReferenceType', N'C') IS NULL ALTER TABLE dbo.CustomerCreditTransaction ADD CONSTRAINT CK_CustomerCreditTransaction_ReferenceType CHECK (ReferenceType IS NULL OR ReferenceType IN (N'Sale',N'CreditRepayment',N'SalesReturn',N'ManualAdjustment'));
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CustomerCreditTransaction_Overdue' AND object_id = OBJECT_ID(N'dbo.CustomerCreditTransaction'))
CREATE INDEX IX_CustomerCreditTransaction_Overdue ON dbo.CustomerCreditTransaction(CustomerId, DueDate, Status) WHERE Status IN (N'Unpaid', N'PartiallyPaid', N'Overdue');
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CustomerCreditRepayment_Customer_Date' AND object_id = OBJECT_ID(N'dbo.CustomerCreditRepayment'))
CREATE INDEX IX_CustomerCreditRepayment_Customer_Date ON dbo.CustomerCreditRepayment(CustomerId, RepaymentDate DESC);
IF OBJECT_ID(N'dbo.PaymentMethod', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_CustomerCreditRepayment_PaymentMethod')
ALTER TABLE dbo.CustomerCreditRepayment ADD CONSTRAINT FK_CustomerCreditRepayment_PaymentMethod FOREIGN KEY(PaymentMethodId) REFERENCES dbo.PaymentMethod(PaymentMethodId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CustomerNote_Customer_Important' AND object_id = OBJECT_ID(N'dbo.CustomerNote'))
CREATE INDEX IX_CustomerNote_Customer_Important ON dbo.CustomerNote(CustomerId, IsActive, IsImportant DESC, CreatedDate DESC);
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerGetPaged
    @PageNumber INT, @PageSize INT, @SearchText NVARCHAR(255)=NULL, @MemberLevelId INT=NULL, @IsActive BIT=NULL, @CreditStatus NVARCHAR(30)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH q AS
    (
        SELECT c.CustomerId,c.CustomerCode,c.CustomerName,c.PhoneNumber,c.Email,ml.LevelName AS MemberLevelName,
               ISNULL(pb.AvailablePoints,0) AvailablePoints,ISNULL(cc.CreditLimit,0) CreditLimit,
               ISNULL(cc.CurrentOutstandingAmount,0) CurrentOutstandingAmount,ISNULL(cc.AvailableCredit,0) AvailableCredit,
               ISNULL(cc.CreditStatus,N'Good') CreditStatus,c.TotalSpending,c.TotalPurchaseCount,c.LastPurchaseDate,c.IsActive,
               COUNT(1) OVER() TotalCount
        FROM dbo.Customer c
        LEFT JOIN dbo.MemberLevel ml ON ml.MemberLevelId=c.MemberLevelId
        LEFT JOIN dbo.CustomerPointBalance pb ON pb.CustomerId=c.CustomerId
        LEFT JOIN dbo.CustomerCredit cc ON cc.CustomerId=c.CustomerId
        WHERE (@SearchText IS NULL OR c.CustomerCode LIKE N'%'+@SearchText+N'%' OR c.CustomerName LIKE N'%'+@SearchText+N'%' OR c.PhoneNumber LIKE N'%'+@SearchText+N'%' OR c.Email LIKE N'%'+@SearchText+N'%')
          AND (@MemberLevelId IS NULL OR c.MemberLevelId=@MemberLevelId)
          AND (@IsActive IS NULL OR c.IsActive=@IsActive)
          AND (@CreditStatus IS NULL OR cc.CreditStatus=@CreditStatus)
    )
    SELECT * FROM q ORDER BY CustomerName OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerSearchForPOS
    @SearchText NVARCHAR(255)=NULL,
    @Top INT=20
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@Top)
        c.CustomerId,
        c.CustomerCode,
        c.CustomerName,
        c.PhoneNumber,
        c.MemberLevelId,
        ml.LevelName AS MemberLevelName,
        ISNULL(cc.CreditLimit,0) CreditLimit,
        ISNULL(cc.CurrentOutstandingAmount,0) UsedCredit,
        ISNULL(cc.AvailableCredit,0) AvailableCredit,
        CONVERT(BIT, CASE WHEN c.IsActive=1 AND ISNULL(cc.AllowCredit,0)=1 AND ISNULL(cc.CreditStatus,N'Good')=N'Good' THEN 1 ELSE 0 END) IsCreditAllowed,
        c.IsActive
    FROM dbo.Customer c
    LEFT JOIN dbo.MemberLevel ml ON ml.MemberLevelId=c.MemberLevelId
    LEFT JOIN dbo.CustomerCredit cc ON cc.CustomerId=c.CustomerId
    WHERE c.IsActive=1
      AND (@SearchText IS NULL OR c.CustomerCode LIKE N'%'+@SearchText+N'%' OR c.CustomerName LIKE N'%'+@SearchText+N'%' OR c.PhoneNumber LIKE N'%'+@SearchText+N'%')
    ORDER BY c.CustomerName;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerGetCreditInfo @CustomerId INT AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        c.CustomerId,
        c.CustomerCode,
        c.CustomerName,
        c.PhoneNumber,
        c.MemberLevelId,
        ml.LevelName AS MemberLevelName,
        ISNULL(cc.CreditLimit,0) CreditLimit,
        ISNULL(cc.CurrentOutstandingAmount,0) UsedCredit,
        ISNULL(cc.AvailableCredit,0) AvailableCredit,
        CONVERT(BIT, CASE WHEN c.IsActive=1 AND ISNULL(cc.AllowCredit,0)=1 AND ISNULL(cc.CreditStatus,N'Good')=N'Good' THEN 1 ELSE 0 END) IsCreditAllowed,
        c.IsActive
    FROM dbo.Customer c
    LEFT JOIN dbo.MemberLevel ml ON ml.MemberLevelId=c.MemberLevelId
    LEFT JOIN dbo.CustomerCredit cc ON cc.CustomerId=c.CustomerId
    WHERE c.CustomerId=@CustomerId;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerCheckAvailableCredit @CustomerId INT, @Amount DECIMAL(18,2) AS
BEGIN
    SET NOCOUNT ON;
    SELECT CONVERT(BIT, CASE WHEN c.IsActive=1 AND cc.AllowCredit=1 AND cc.CreditStatus=N'Good' AND cc.AvailableCredit>=@Amount THEN 1 ELSE 0 END) IsAllowed,
           cc.RequireManagerApproval RequiresManagerApproval, cc.CreditLimit, cc.CurrentOutstandingAmount, cc.AvailableCredit, @Amount RequestedAmount,
           CASE WHEN c.CustomerId IS NULL THEN N'Customer does not exist.'
                WHEN c.IsActive=0 THEN N'Customer is inactive.'
                WHEN ISNULL(cc.AllowCredit,0)=0 THEN N'Credit is not allowed.'
                WHEN cc.CreditStatus<>N'Good' THEN N'Credit status is not good.'
                WHEN cc.AvailableCredit<@Amount THEN N'Insufficient available credit.'
                ELSE N'Credit is allowed.' END Message
    FROM dbo.Customer c
    LEFT JOIN dbo.CustomerCredit cc ON cc.CustomerId=c.CustomerId
    WHERE c.CustomerId=@CustomerId;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerCreditTransactionGetByCustomerId @CustomerId INT AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.CustomerCreditTransaction WHERE CustomerId=@CustomerId ORDER BY CreatedDate DESC, CustomerCreditTransactionId DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerCreditTransactionGetPaged
    @CustomerId INT=NULL,@PageNumber INT=1,@PageSize INT=20,@DateFrom DATETIME2=NULL,@DateTo DATETIME2=NULL,@TransactionType NVARCHAR(30)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH q AS
    (
        SELECT t.*, c.CustomerCode, c.CustomerName, COUNT(1) OVER() TotalCount
        FROM dbo.CustomerCreditTransaction t
        JOIN dbo.Customer c ON c.CustomerId=t.CustomerId
        WHERE (@CustomerId IS NULL OR t.CustomerId=@CustomerId)
          AND (@TransactionType IS NULL OR t.TransactionType=@TransactionType)
          AND (@DateFrom IS NULL OR t.CreatedDate>=@DateFrom)
          AND (@DateTo IS NULL OR t.CreatedDate<DATEADD(DAY,1,@DateTo))
    )
    SELECT * FROM q ORDER BY CreatedDate DESC, CustomerCreditTransactionId DESC OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerCreditTransactionCreate
    @CustomerId INT,@TransactionType NVARCHAR(30),@ReferenceType NVARCHAR(30),@ReferenceId BIGINT=NULL,@ReferenceNo NVARCHAR(50)=NULL,
    @Amount DECIMAL(18,2),@BalanceBefore DECIMAL(18,2),@BalanceAfter DECIMAL(18,2),@Description NVARCHAR(500)=NULL,@CreatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.CustomerCreditTransaction(CustomerId,TransactionType,ReferenceType,ReferenceId,ReferenceNo,Amount,BalanceBefore,BalanceAfter,Status,Description,Remark,CreatedByUserId)
    VALUES(@CustomerId,@TransactionType,@ReferenceType,@ReferenceId,@ReferenceNo,@Amount,@BalanceBefore,@BalanceAfter,N'Paid',@Description,@Description,@CreatedByUserId);
    SELECT CONVERT(BIGINT,SCOPE_IDENTITY());
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerGetById @CustomerId INT AS
BEGIN
    SET NOCOUNT ON;
    SELECT c.*,ml.LevelCode AS MemberLevelCode,ml.LevelName AS MemberLevelName,ISNULL(ml.DiscountPercent,0) DiscountPercent,
           ISNULL(pb.AvailablePoints,0) AvailablePoints,ISNULL(pb.LifetimeEarnedPoints,0) LifetimeEarnedPoints,ISNULL(pb.LifetimeRedeemedPoints,0) LifetimeRedeemedPoints,
           ISNULL(cc.AllowCredit,0) AllowCredit,ISNULL(cc.CreditLimit,0) CreditLimit,ISNULL(cc.CreditTermDays,0) CreditTermDays,
           ISNULL(cc.CurrentOutstandingAmount,0) CurrentOutstandingAmount,ISNULL(cc.AvailableCredit,0) AvailableCredit,ISNULL(cc.CreditStatus,N'Good') CreditStatus,
           ISNULL(cc.RequireManagerApproval,0) RequireManagerApproval
    FROM dbo.Customer c
    LEFT JOIN dbo.MemberLevel ml ON ml.MemberLevelId=c.MemberLevelId
    LEFT JOIN dbo.CustomerPointBalance pb ON pb.CustomerId=c.CustomerId
    LEFT JOIN dbo.CustomerCredit cc ON cc.CustomerId=c.CustomerId
    WHERE c.CustomerId=@CustomerId;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerGetByPhoneNumber @PhoneNumber NVARCHAR(50) AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @CustomerId INT=(SELECT CustomerId FROM dbo.Customer WHERE PhoneNumber=@PhoneNumber);
    EXEC dbo.spCustomerGetById @CustomerId;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerCreate
    @CustomerCode NVARCHAR(50)=NULL,@CustomerName NVARCHAR(255),@PhoneNumber NVARCHAR(50),@Email NVARCHAR(255)=NULL,@MemberLevelId INT=NULL,
    @DateOfBirth DATE=NULL,@Gender NVARCHAR(20)=NULL,@Address NVARCHAR(1000)=NULL,@CreatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF EXISTS(SELECT 1 FROM dbo.Customer WHERE PhoneNumber=@PhoneNumber) THROW 51000,'Phone number already exists.',1;
    IF @Email IS NOT NULL AND EXISTS(SELECT 1 FROM dbo.Customer WHERE Email=@Email) THROW 51001,'Email already exists.',1;
    BEGIN TRAN;
    IF NULLIF(LTRIM(RTRIM(@CustomerCode)),N'') IS NULL SET @CustomerCode=CONCAT(N'C',FORMAT(SYSDATETIME(),N'yyyyMMddHHmmss'),RIGHT(CONCAT(N'0000',ABS(CHECKSUM(NEWID()))%10000),4));
    IF EXISTS(SELECT 1 FROM dbo.Customer WHERE CustomerCode=@CustomerCode) SET @CustomerCode=CONCAT(@CustomerCode,N'-',CONVERT(NVARCHAR(10),ABS(CHECKSUM(NEWID()))%10000));
    INSERT dbo.Customer(CustomerCode,CustomerName,PhoneNumber,Email,MemberLevelId,DateOfBirth,Gender,Address,CreatedByUserId)
    VALUES(@CustomerCode,@CustomerName,@PhoneNumber,@Email,@MemberLevelId,@DateOfBirth,@Gender,@Address,@CreatedByUserId);
    DECLARE @CustomerId INT=SCOPE_IDENTITY();
    INSERT dbo.CustomerPointBalance(CustomerId) VALUES(@CustomerId);
    INSERT dbo.CustomerCredit(CustomerId,AllowCredit,CreditLimit,CreditTermDays,CreditStatus,RequireManagerApproval,CreatedByUserId)
    SELECT @CustomerId,COALESCE(ml.AllowCredit,0),CASE WHEN COALESCE(ml.AllowCredit,0)=1 THEN ml.DefaultCreditLimit ELSE 0 END,CASE WHEN COALESCE(ml.AllowCredit,0)=1 THEN ml.DefaultCreditTermDays ELSE 0 END,N'Good',COALESCE(ml.RequireManagerApprovalForCredit,0),@CreatedByUserId
    FROM (SELECT 1 AS OneRow) d
    LEFT JOIN dbo.MemberLevel ml ON ml.MemberLevelId=@MemberLevelId;
    COMMIT;
    SELECT @CustomerId;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerUpdate
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
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerToggleActive @CustomerId INT,@IsActive BIT,@UpdatedByUserId INT AS
BEGIN UPDATE dbo.Customer SET IsActive=@IsActive,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@UpdatedByUserId WHERE CustomerId=@CustomerId; END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerIsPhoneNumberExists @PhoneNumber NVARCHAR(50),@ExcludeCustomerId INT=NULL AS
BEGIN SELECT CONVERT(BIT,CASE WHEN EXISTS(SELECT 1 FROM dbo.Customer WHERE PhoneNumber=@PhoneNumber AND (@ExcludeCustomerId IS NULL OR CustomerId<>@ExcludeCustomerId)) THEN 1 ELSE 0 END); END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerIsEmailExists @Email NVARCHAR(255),@ExcludeCustomerId INT=NULL AS
BEGIN SELECT CONVERT(BIT,CASE WHEN @Email IS NOT NULL AND EXISTS(SELECT 1 FROM dbo.Customer WHERE Email=@Email AND (@ExcludeCustomerId IS NULL OR CustomerId<>@ExcludeCustomerId)) THEN 1 ELSE 0 END); END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerUpdatePurchaseSummary @CustomerId INT,@SaleAmount DECIMAL(18,2),@PurchaseDate DATETIME2 AS
BEGIN UPDATE dbo.Customer SET TotalSpending=TotalSpending+@SaleAmount,TotalPurchaseCount=TotalPurchaseCount+1,LastPurchaseDate=@PurchaseDate,UpdatedDate=SYSDATETIME() WHERE CustomerId=@CustomerId; END
GO

CREATE OR ALTER PROCEDURE dbo.spMemberLevelGetAll AS BEGIN SELECT * FROM dbo.MemberLevel ORDER BY DisplayOrder,LevelName; END
GO
CREATE OR ALTER PROCEDURE dbo.spMemberLevelGetAllActive AS BEGIN SELECT * FROM dbo.MemberLevel WHERE IsActive=1 ORDER BY DisplayOrder,LevelName; END
GO
CREATE OR ALTER PROCEDURE dbo.spMemberLevelGetById @MemberLevelId INT AS BEGIN SELECT * FROM dbo.MemberLevel WHERE MemberLevelId=@MemberLevelId; END
GO
CREATE OR ALTER PROCEDURE dbo.spMemberLevelCreate
    @LevelCode NVARCHAR(50),@LevelName NVARCHAR(255),@Description NVARCHAR(500)=NULL,@MinSpendingAmount DECIMAL(18,2)=0,@DiscountPercent DECIMAL(9,2)=0,@PointEarnAmount DECIMAL(18,2)=100,@PointEarnPoint DECIMAL(18,2)=1,@PointMultiplier DECIMAL(9,2)=1,@AllowCredit BIT=0,@DefaultCreditLimit DECIMAL(18,2)=0,@DefaultCreditTermDays INT=0,@RequireManagerApprovalForCredit BIT=0,@MaxOverdueDaysAllowed INT=0,@DisplayOrder INT=0,@CreatedByUserId INT
AS
BEGIN
    IF EXISTS(SELECT 1 FROM dbo.MemberLevel WHERE LevelCode=@LevelCode) THROW 51100,'Level code already exists.',1;
    IF @AllowCredit=0 SELECT @DefaultCreditLimit=0,@DefaultCreditTermDays=0;
    INSERT dbo.MemberLevel(LevelCode,LevelName,Description,MinSpendingAmount,DiscountPercent,PointEarnAmount,PointEarnPoint,PointMultiplier,AllowCredit,DefaultCreditLimit,DefaultCreditTermDays,RequireManagerApprovalForCredit,MaxOverdueDaysAllowed,DisplayOrder,CreatedByUserId)
    VALUES(@LevelCode,@LevelName,@Description,@MinSpendingAmount,@DiscountPercent,@PointEarnAmount,@PointEarnPoint,@PointMultiplier,@AllowCredit,@DefaultCreditLimit,@DefaultCreditTermDays,@RequireManagerApprovalForCredit,@MaxOverdueDaysAllowed,@DisplayOrder,@CreatedByUserId);
    SELECT CONVERT(INT,SCOPE_IDENTITY());
END
GO
CREATE OR ALTER PROCEDURE dbo.spMemberLevelUpdate
    @MemberLevelId INT,@LevelCode NVARCHAR(50),@LevelName NVARCHAR(255),@Description NVARCHAR(500)=NULL,@MinSpendingAmount DECIMAL(18,2)=0,@DiscountPercent DECIMAL(9,2)=0,@PointEarnAmount DECIMAL(18,2)=100,@PointEarnPoint DECIMAL(18,2)=1,@PointMultiplier DECIMAL(9,2)=1,@AllowCredit BIT=0,@DefaultCreditLimit DECIMAL(18,2)=0,@DefaultCreditTermDays INT=0,@RequireManagerApprovalForCredit BIT=0,@MaxOverdueDaysAllowed INT=0,@DisplayOrder INT=0,@IsActive BIT=1,@UpdatedByUserId INT
AS
BEGIN
    IF EXISTS(SELECT 1 FROM dbo.MemberLevel WHERE LevelCode=@LevelCode AND MemberLevelId<>@MemberLevelId) THROW 51101,'Level code already exists.',1;
    IF @AllowCredit=0 SELECT @DefaultCreditLimit=0,@DefaultCreditTermDays=0;
    UPDATE dbo.MemberLevel SET LevelCode=@LevelCode,LevelName=@LevelName,Description=@Description,MinSpendingAmount=@MinSpendingAmount,DiscountPercent=@DiscountPercent,PointEarnAmount=@PointEarnAmount,PointEarnPoint=@PointEarnPoint,PointMultiplier=@PointMultiplier,AllowCredit=@AllowCredit,DefaultCreditLimit=@DefaultCreditLimit,DefaultCreditTermDays=@DefaultCreditTermDays,RequireManagerApprovalForCredit=@RequireManagerApprovalForCredit,MaxOverdueDaysAllowed=@MaxOverdueDaysAllowed,DisplayOrder=@DisplayOrder,IsActive=@IsActive,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@UpdatedByUserId WHERE MemberLevelId=@MemberLevelId;
END
GO
CREATE OR ALTER PROCEDURE dbo.spMemberLevelToggleActive @MemberLevelId INT,@IsActive BIT,@UpdatedByUserId INT AS BEGIN UPDATE dbo.MemberLevel SET IsActive=@IsActive,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@UpdatedByUserId WHERE MemberLevelId=@MemberLevelId; END
GO
CREATE OR ALTER PROCEDURE dbo.spMemberLevelIsCodeExists @LevelCode NVARCHAR(50),@ExcludeMemberLevelId INT=NULL AS BEGIN SELECT CONVERT(BIT,CASE WHEN EXISTS(SELECT 1 FROM dbo.MemberLevel WHERE LevelCode=@LevelCode AND (@ExcludeMemberLevelId IS NULL OR MemberLevelId<>@ExcludeMemberLevelId)) THEN 1 ELSE 0 END); END
GO

CREATE OR ALTER PROCEDURE dbo.spMemberLevelUpgradeRuleGetAll AS
BEGIN SELECT r.*,f.LevelName FromMemberLevelName,t.LevelName ToMemberLevelName FROM dbo.MemberLevelUpgradeRule r JOIN dbo.MemberLevel f ON f.MemberLevelId=r.FromMemberLevelId JOIN dbo.MemberLevel t ON t.MemberLevelId=r.ToMemberLevelId ORDER BY r.IsActive DESC,r.MemberLevelUpgradeRuleId; END
GO
CREATE OR ALTER PROCEDURE dbo.spMemberLevelUpgradeRuleGetById @MemberLevelUpgradeRuleId INT AS
BEGIN SELECT r.*,f.LevelName FromMemberLevelName,t.LevelName ToMemberLevelName FROM dbo.MemberLevelUpgradeRule r JOIN dbo.MemberLevel f ON f.MemberLevelId=r.FromMemberLevelId JOIN dbo.MemberLevel t ON t.MemberLevelId=r.ToMemberLevelId WHERE r.MemberLevelUpgradeRuleId=@MemberLevelUpgradeRuleId; END
GO
CREATE OR ALTER PROCEDURE dbo.spMemberLevelUpgradeRuleGetByFromLevelId @FromMemberLevelId INT AS
BEGIN SELECT r.*,f.LevelName FromMemberLevelName,t.LevelName ToMemberLevelName FROM dbo.MemberLevelUpgradeRule r JOIN dbo.MemberLevel f ON f.MemberLevelId=r.FromMemberLevelId JOIN dbo.MemberLevel t ON t.MemberLevelId=r.ToMemberLevelId WHERE r.FromMemberLevelId=@FromMemberLevelId AND r.IsActive=1; END
GO
CREATE OR ALTER PROCEDURE dbo.spMemberLevelUpgradeRuleCreate @FromMemberLevelId INT,@ToMemberLevelId INT,@RequiredTotalSpending DECIMAL(18,2)=0,@RequiredPurchaseCount INT=0,@RequiredMembershipDays INT=0,@RequireNoOverduePayment BIT=1,@RequireManagerApproval BIT=0,@CreatedByUserId INT AS
BEGIN INSERT dbo.MemberLevelUpgradeRule(FromMemberLevelId,ToMemberLevelId,RequiredTotalSpending,RequiredPurchaseCount,RequiredMembershipDays,RequireNoOverduePayment,RequireManagerApproval,CreatedByUserId) VALUES(@FromMemberLevelId,@ToMemberLevelId,@RequiredTotalSpending,@RequiredPurchaseCount,@RequiredMembershipDays,@RequireNoOverduePayment,@RequireManagerApproval,@CreatedByUserId); SELECT CONVERT(INT,SCOPE_IDENTITY()); END
GO
CREATE OR ALTER PROCEDURE dbo.spMemberLevelUpgradeRuleUpdate @MemberLevelUpgradeRuleId INT,@FromMemberLevelId INT,@ToMemberLevelId INT,@RequiredTotalSpending DECIMAL(18,2)=0,@RequiredPurchaseCount INT=0,@RequiredMembershipDays INT=0,@RequireNoOverduePayment BIT=1,@RequireManagerApproval BIT=0,@IsActive BIT=1,@UpdatedByUserId INT AS
BEGIN UPDATE dbo.MemberLevelUpgradeRule SET FromMemberLevelId=@FromMemberLevelId,ToMemberLevelId=@ToMemberLevelId,RequiredTotalSpending=@RequiredTotalSpending,RequiredPurchaseCount=@RequiredPurchaseCount,RequiredMembershipDays=@RequiredMembershipDays,RequireNoOverduePayment=@RequireNoOverduePayment,RequireManagerApproval=@RequireManagerApproval,IsActive=@IsActive,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@UpdatedByUserId WHERE MemberLevelUpgradeRuleId=@MemberLevelUpgradeRuleId; END
GO
CREATE OR ALTER PROCEDURE dbo.spMemberLevelUpgradeRuleToggleActive @MemberLevelUpgradeRuleId INT,@IsActive BIT,@UpdatedByUserId INT AS BEGIN UPDATE dbo.MemberLevelUpgradeRule SET IsActive=@IsActive,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@UpdatedByUserId WHERE MemberLevelUpgradeRuleId=@MemberLevelUpgradeRuleId; END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerCheckLevelEligibility @CustomerId INT AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @CurrentLevelId INT,@CreatedDate DATETIME2,@Spending DECIMAL(18,2),@Purchases INT;
    SELECT @CurrentLevelId=MemberLevelId,@CreatedDate=CreatedDate,@Spending=TotalSpending,@Purchases=TotalPurchaseCount FROM dbo.Customer WHERE CustomerId=@CustomerId;
    DECLARE @HasOverdue BIT=CASE WHEN EXISTS(SELECT 1 FROM dbo.CustomerCreditTransaction WHERE CustomerId=@CustomerId AND Status IN(N'Overdue') OR (CustomerId=@CustomerId AND Status IN(N'Unpaid',N'PartiallyPaid') AND DueDate<CONVERT(date,SYSDATETIME()))) THEN 1 ELSE 0 END;
    SELECT TOP(1)
        CONVERT(BIT,CASE WHEN @Spending>=r.RequiredTotalSpending AND @Purchases>=r.RequiredPurchaseCount AND DATEDIFF(DAY,@CreatedDate,SYSDATETIME())>=r.RequiredMembershipDays AND (r.RequireNoOverduePayment=0 OR @HasOverdue=0) THEN 1 ELSE 0 END) IsEligible,
        @CustomerId CustomerId,@CurrentLevelId CurrentMemberLevelId,cl.LevelName CurrentMemberLevelName,r.ToMemberLevelId NextMemberLevelId,nl.LevelName NextMemberLevelName,
        r.RequiredTotalSpending,@Spending CurrentTotalSpending,CASE WHEN r.RequiredTotalSpending>@Spending THEN r.RequiredTotalSpending-@Spending ELSE 0 END MissingSpendingAmount,
        r.RequiredPurchaseCount,@Purchases CurrentPurchaseCount,CASE WHEN r.RequiredPurchaseCount>@Purchases THEN r.RequiredPurchaseCount-@Purchases ELSE 0 END MissingPurchaseCount,
        r.RequiredMembershipDays,DATEDIFF(DAY,@CreatedDate,SYSDATETIME()) CurrentMembershipDays,@HasOverdue HasOverdueCredit,r.RequireManagerApproval,
        CASE WHEN r.MemberLevelUpgradeRuleId IS NULL THEN N'No active upgrade rule.' WHEN r.RequireManagerApproval=1 THEN N'Eligible; manager approval required.' ELSE N'Eligibility checked.' END Message
    FROM dbo.MemberLevelUpgradeRule r
    JOIN dbo.MemberLevel nl ON nl.MemberLevelId=r.ToMemberLevelId
    LEFT JOIN dbo.MemberLevel cl ON cl.MemberLevelId=@CurrentLevelId
    WHERE r.FromMemberLevelId=@CurrentLevelId AND r.IsActive=1;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerUpgradeLevel @CustomerId INT,@ChangedByUserId INT,@ApplyMemberLevelCreditDefault BIT=1,@ManagerApproved BIT=0 AS
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
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerPointGetBalance @CustomerId INT AS BEGIN SELECT * FROM dbo.CustomerPointBalance WHERE CustomerId=@CustomerId; END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerPointGetMovementsPaged @CustomerId INT,@PageNumber INT,@PageSize INT,@DateFrom DATETIME2=NULL,@DateTo DATETIME2=NULL,@MovementType NVARCHAR(30)=NULL AS
BEGIN SELECT *,COUNT(1) OVER() TotalCount FROM dbo.CustomerPointMovement WHERE CustomerId=@CustomerId AND (@MovementType IS NULL OR MovementType=@MovementType) AND (@DateFrom IS NULL OR CreatedDate>=@DateFrom) AND (@DateTo IS NULL OR CreatedDate<DATEADD(DAY,1,@DateTo)) ORDER BY CreatedDate DESC OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY; END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerPointEarn @CustomerId INT,@SaleAmount DECIMAL(18,2),@ReferenceType NVARCHAR(50)=NULL,@ReferenceId BIGINT=NULL,@ReferenceNo NVARCHAR(100)=NULL,@ExpiryDate DATE=NULL,@CreatedByUserId INT AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @Earned DECIMAL(18,2),@Balance DECIMAL(18,2);
    SELECT @Earned=FLOOR((@SaleAmount/ISNULL(NULLIF(ml.PointEarnAmount,0),100))*ISNULL(ml.PointEarnPoint,1)*ISNULL(ml.PointMultiplier,1)) FROM dbo.Customer c LEFT JOIN dbo.MemberLevel ml ON ml.MemberLevelId=c.MemberLevelId WHERE c.CustomerId=@CustomerId;
    SET @Earned=ISNULL(@Earned,0);
    BEGIN TRAN;
    UPDATE dbo.CustomerPointBalance SET AvailablePoints=AvailablePoints+@Earned,LifetimeEarnedPoints=LifetimeEarnedPoints+@Earned,LastMovementDate=SYSDATETIME(),UpdatedDate=SYSDATETIME() WHERE CustomerId=@CustomerId;
    SELECT @Balance=AvailablePoints FROM dbo.CustomerPointBalance WHERE CustomerId=@CustomerId;
    IF @Earned>0 INSERT dbo.CustomerPointMovement(CustomerId,MovementType,PointsIn,BalanceAfter,ReferenceType,ReferenceId,ReferenceNo,ExpiryDate,CreatedByUserId) VALUES(@CustomerId,N'Earn',@Earned,@Balance,@ReferenceType,@ReferenceId,@ReferenceNo,@ExpiryDate,@CreatedByUserId);
    COMMIT; SELECT @Earned;
END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerPointRedeem @CustomerId INT,@Points DECIMAL(18,2),@ReferenceType NVARCHAR(50)=NULL,@ReferenceId BIGINT=NULL,@ReferenceNo NVARCHAR(100)=NULL,@Remark NVARCHAR(1000)=NULL,@CreatedByUserId INT AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @Balance DECIMAL(18,2);
    BEGIN TRAN;
    SELECT @Balance=AvailablePoints FROM dbo.CustomerPointBalance WITH(UPDLOCK,ROWLOCK) WHERE CustomerId=@CustomerId;
    IF @Balance<@Points THROW 51301,'Redeem points exceed available balance.',1;
    UPDATE dbo.CustomerPointBalance SET AvailablePoints=AvailablePoints-@Points,LifetimeRedeemedPoints=LifetimeRedeemedPoints+@Points,LastMovementDate=SYSDATETIME(),UpdatedDate=SYSDATETIME() WHERE CustomerId=@CustomerId;
    SELECT @Balance=AvailablePoints FROM dbo.CustomerPointBalance WHERE CustomerId=@CustomerId;
    INSERT dbo.CustomerPointMovement(CustomerId,MovementType,PointsOut,BalanceAfter,ReferenceType,ReferenceId,ReferenceNo,Remark,CreatedByUserId)
    VALUES(@CustomerId,N'Redeem',@Points,@Balance,@ReferenceType,@ReferenceId,@ReferenceNo,@Remark,@CreatedByUserId);
    COMMIT;
END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerPointAdjust @CustomerId INT,@AdjustmentType NVARCHAR(30),@Points DECIMAL(18,2),@ReferenceType NVARCHAR(50)=NULL,@ReferenceId BIGINT=NULL,@ReferenceNo NVARCHAR(100)=NULL,@Remark NVARCHAR(1000)=NULL,@CreatedByUserId INT AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @Balance DECIMAL(18,2);
    BEGIN TRAN;
    SELECT @Balance=AvailablePoints FROM dbo.CustomerPointBalance WITH(UPDLOCK,ROWLOCK) WHERE CustomerId=@CustomerId;
    IF @AdjustmentType IN(N'AdjustOut',N'Redeem',N'Expire',N'Reverse') AND @Balance<@Points THROW 51300,'Point balance cannot be negative.',1;
    UPDATE dbo.CustomerPointBalance SET AvailablePoints=AvailablePoints+CASE WHEN @AdjustmentType=N'AdjustIn' THEN @Points ELSE -@Points END,LifetimeRedeemedPoints=LifetimeRedeemedPoints+CASE WHEN @AdjustmentType=N'AdjustOut' THEN @Points ELSE 0 END,LastMovementDate=SYSDATETIME(),UpdatedDate=SYSDATETIME() WHERE CustomerId=@CustomerId;
    SELECT @Balance=AvailablePoints FROM dbo.CustomerPointBalance WHERE CustomerId=@CustomerId;
    INSERT dbo.CustomerPointMovement(CustomerId,MovementType,PointsIn,PointsOut,BalanceAfter,ReferenceType,ReferenceId,ReferenceNo,Remark,CreatedByUserId)
    VALUES(@CustomerId,@AdjustmentType,CASE WHEN @AdjustmentType=N'AdjustIn' THEN @Points ELSE 0 END,CASE WHEN @AdjustmentType<>N'AdjustIn' THEN @Points ELSE 0 END,@Balance,@ReferenceType,@ReferenceId,@ReferenceNo,@Remark,@CreatedByUserId);
    COMMIT;
END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerPointReverseByReference @ReferenceType NVARCHAR(50),@ReferenceId BIGINT,@Remark NVARCHAR(1000)=NULL,@CreatedByUserId INT AS
BEGIN
    DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT CustomerId,CASE WHEN PointsIn>0 THEN PointsIn ELSE PointsOut END FROM dbo.CustomerPointMovement WHERE ReferenceType=@ReferenceType AND ReferenceId=@ReferenceId AND MovementType<>N'Reverse';
    DECLARE @CustomerId INT,@Points DECIMAL(18,2); OPEN c; FETCH NEXT FROM c INTO @CustomerId,@Points;
    WHILE @@FETCH_STATUS=0 BEGIN EXEC dbo.spCustomerPointAdjust @CustomerId,N'Reverse',@Points,@ReferenceType,@ReferenceId,NULL,@Remark,@CreatedByUserId; FETCH NEXT FROM c INTO @CustomerId,@Points; END
    CLOSE c; DEALLOCATE c;
END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerPointExpire @ExpiryDate DATE,@CreatedByUserId INT AS
BEGIN
    DECLARE @Count INT=0;
    DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT CustomerId,SUM(PointsIn-PointsOut) FROM dbo.CustomerPointMovement WHERE ExpiryDate<=@ExpiryDate GROUP BY CustomerId HAVING SUM(PointsIn-PointsOut)>0;
    DECLARE @CustomerId INT,@Points DECIMAL(18,2); OPEN c; FETCH NEXT FROM c INTO @CustomerId,@Points;
    WHILE @@FETCH_STATUS=0 BEGIN EXEC dbo.spCustomerPointAdjust @CustomerId,N'Expire',@Points,N'PointExpiry',NULL,NULL,N'Points expired.',@CreatedByUserId; SET @Count+=1; FETCH NEXT FROM c INTO @CustomerId,@Points; END
    CLOSE c; DEALLOCATE c; SELECT @Count;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerCreditGetByCustomerId @CustomerId INT AS BEGIN SELECT * FROM dbo.CustomerCredit WHERE CustomerId=@CustomerId; END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerCreditSet @CustomerId INT,@AllowCredit BIT,@CreditLimit DECIMAL(18,2),@CreditTermDays INT,@CreditStatus NVARCHAR(30),@RequireManagerApproval BIT,@ApprovedByUserId INT=NULL,@Remark NVARCHAR(1000)=NULL,@UpdatedByUserId INT AS
BEGIN UPDATE dbo.CustomerCredit SET AllowCredit=@AllowCredit,CreditLimit=CASE WHEN @AllowCredit=1 THEN @CreditLimit ELSE 0 END,CreditTermDays=CASE WHEN @AllowCredit=1 THEN @CreditTermDays ELSE 0 END,CreditStatus=@CreditStatus,RequireManagerApproval=@RequireManagerApproval,ApprovedByUserId=@ApprovedByUserId,ApprovedDate=CASE WHEN @ApprovedByUserId IS NULL THEN ApprovedDate ELSE SYSDATETIME() END,Remark=@Remark,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@UpdatedByUserId WHERE CustomerId=@CustomerId; END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerCreditCheckEligibility @CustomerId INT,@SaleAmount DECIMAL(18,2) AS
BEGIN
    SELECT CONVERT(BIT,CASE WHEN c.IsActive=1 AND cc.AllowCredit=1 AND cc.CreditStatus=N'Good' AND cc.CreditLimit>0 AND @SaleAmount<=cc.AvailableCredit AND NOT EXISTS(SELECT 1 FROM dbo.CustomerCreditTransaction t WHERE t.CustomerId=@CustomerId AND t.Status IN(N'Overdue') OR (t.CustomerId=@CustomerId AND t.Status IN(N'Unpaid',N'PartiallyPaid') AND t.DueDate<CONVERT(date,SYSDATETIME()))) THEN 1 ELSE 0 END) IsAllowed,
           cc.RequireManagerApproval RequiresManagerApproval,cc.CreditLimit,cc.CurrentOutstandingAmount,cc.AvailableCredit,@SaleAmount RequestedAmount,
           CASE WHEN c.IsActive=0 THEN N'Customer is inactive.' WHEN cc.AllowCredit=0 THEN N'Credit is not allowed.' WHEN cc.CreditStatus<>N'Good' THEN N'Credit status is not good.' WHEN @SaleAmount>cc.AvailableCredit THEN N'Insufficient available credit.' ELSE N'Credit is allowed.' END Message
    FROM dbo.Customer c JOIN dbo.CustomerCredit cc ON cc.CustomerId=c.CustomerId WHERE c.CustomerId=@CustomerId;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerUseCreditForSale
    @CustomerId INT,@SalesHeaderId BIGINT,@SaleNo NVARCHAR(50),@Amount DECIMAL(18,2),@CreatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @Before DECIMAL(18,2), @Available DECIMAL(18,2), @Term INT;
        SELECT @Before=CurrentOutstandingAmount,@Available=AvailableCredit,@Term=CreditTermDays
        FROM dbo.CustomerCredit WITH(UPDLOCK,HOLDLOCK)
        WHERE CustomerId=@CustomerId AND AllowCredit=1 AND CreditStatus=N'Good';
        IF @Before IS NULL THROW 53100, 'Customer credit is not allowed.', 1;
        IF @Amount <= 0 THROW 53101, 'Credit amount must be greater than zero.', 1;
        IF @Available < @Amount THROW 53102, 'Insufficient available credit.', 1;
        UPDATE dbo.CustomerCredit SET CurrentOutstandingAmount=CurrentOutstandingAmount+@Amount,UpdatedDate=SYSUTCDATETIME(),UpdatedByUserId=@CreatedByUserId WHERE CustomerId=@CustomerId;
        INSERT dbo.CustomerCreditTransaction(CustomerId,SaleId,TransactionType,ReferenceType,ReferenceId,ReferenceNo,Amount,BalanceBefore,BalanceAfter,DueDate,Status,Remark,Description,CreatedByUserId)
        VALUES(@CustomerId,@SalesHeaderId,N'CreditUsed',N'Sale',@SalesHeaderId,@SaleNo,@Amount,@Before,@Before+@Amount,DATEADD(DAY,ISNULL(@Term,0),CONVERT(date,SYSUTCDATETIME())),N'Unpaid',N'Customer credit used for sale.',N'Customer credit used for sale.',@CreatedByUserId);
        COMMIT;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerRefundCreditFromReturn
    @CustomerId INT,@SalesReturnHeaderId BIGINT,@ReturnNo NVARCHAR(50),@Amount DECIMAL(18,2),@CreatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @Before DECIMAL(18,2);
        SELECT @Before=CurrentOutstandingAmount FROM dbo.CustomerCredit WITH(UPDLOCK,HOLDLOCK) WHERE CustomerId=@CustomerId;
        IF @Before IS NULL THROW 53110, 'Customer credit account was not found.', 1;
        IF @Amount <= 0 THROW 53111, 'Refund credit amount must be greater than zero.', 1;
        IF @Before < @Amount THROW 53112, 'Refund credit amount exceeds used credit.', 1;
        UPDATE dbo.CustomerCredit SET CurrentOutstandingAmount=CurrentOutstandingAmount-@Amount,UpdatedDate=SYSUTCDATETIME(),UpdatedByUserId=@CreatedByUserId WHERE CustomerId=@CustomerId;
        INSERT dbo.CustomerCreditTransaction(CustomerId,TransactionType,ReferenceType,ReferenceId,ReferenceNo,Amount,BalanceBefore,BalanceAfter,Status,Remark,Description,CreatedByUserId)
        VALUES(@CustomerId,N'CreditRefunded',N'SalesReturn',@SalesReturnHeaderId,@ReturnNo,@Amount,@Before,@Before-@Amount,N'Paid',N'Customer credit restored from refund.',N'Customer credit restored from refund.',@CreatedByUserId);
        COMMIT;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerCreditGetSummary AS
BEGIN
    SET NOCOUNT ON;
    SELECT ISNULL(SUM(CreditLimit),0) TotalCreditLimit, ISNULL(SUM(CurrentOutstandingAmount),0) TotalUsedCredit, ISNULL(SUM(AvailableCredit),0) TotalAvailableCredit, COUNT(1) CustomerCount
    FROM dbo.CustomerCredit;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerCreditRepaymentGetPaged
    @PageNumber INT=1,@PageSize INT=20,@CustomerId INT=NULL,@Status NVARCHAR(30)=NULL,@DateFrom DATETIME2=NULL,@DateTo DATETIME2=NULL
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH q AS
    (
        SELECT r.*, c.CustomerName, pm.PaymentMethodName, COUNT(1) OVER() TotalCount
        FROM dbo.CustomerCreditRepayment r
        JOIN dbo.Customer c ON c.CustomerId=r.CustomerId
        JOIN dbo.PaymentMethod pm ON pm.PaymentMethodId=r.PaymentMethodId
        WHERE (@CustomerId IS NULL OR r.CustomerId=@CustomerId)
          AND (@Status IS NULL OR r.Status=@Status)
          AND (@DateFrom IS NULL OR r.RepaymentDate>=@DateFrom)
          AND (@DateTo IS NULL OR r.RepaymentDate<DATEADD(DAY,1,@DateTo))
    )
    SELECT * FROM q ORDER BY RepaymentDate DESC, CustomerCreditRepaymentId DESC OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerCreditRepaymentGetById @CustomerCreditRepaymentId BIGINT AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.*, c.CustomerName, pm.PaymentMethodName
    FROM dbo.CustomerCreditRepayment r
    JOIN dbo.Customer c ON c.CustomerId=r.CustomerId
    JOIN dbo.PaymentMethod pm ON pm.PaymentMethodId=r.PaymentMethodId
    WHERE r.CustomerCreditRepaymentId=@CustomerCreditRepaymentId;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerCreditRepaymentCreate
    @CustomerId INT,@PaymentMethodId INT,@PaymentAmount DECIMAL(18,2),@ReferenceNo NVARCHAR(100)=NULL,@Remark NVARCHAR(500)=NULL,@CreatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.Customer WHERE CustomerId=@CustomerId AND IsActive=1) THROW 53120, 'Customer does not exist or is inactive.', 1;
        IF @PaymentAmount <= 0 THROW 53121, 'Repayment amount must be greater than zero.', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.PaymentMethod WHERE PaymentMethodId=@PaymentMethodId AND IsActive=1) THROW 53122, 'Payment method does not exist or is inactive.', 1;
        DECLARE @Before DECIMAL(18,2);
        SELECT @Before=CurrentOutstandingAmount FROM dbo.CustomerCredit WITH(UPDLOCK,HOLDLOCK) WHERE CustomerId=@CustomerId;
        IF @Before IS NULL OR @Before <= 0 THROW 53123, 'Customer has no used credit to repay.', 1;
        IF @PaymentAmount > @Before THROW 53124, 'Repayment amount cannot exceed used credit.', 1;
        DECLARE @RepaymentNo NVARCHAR(50)=CONCAT(N'CRP', FORMAT(SYSUTCDATETIME(),'yyyyMMddHHmmssfff'));
        INSERT dbo.CustomerCreditRepayment(RepaymentNo,CustomerId,PaymentMethodId,PaymentAmount,ReferenceNo,Remark,Status,CreatedByUserId)
        VALUES(@RepaymentNo,@CustomerId,@PaymentMethodId,@PaymentAmount,NULLIF(LTRIM(RTRIM(@ReferenceNo)),N''),@Remark,N'Completed',@CreatedByUserId);
        DECLARE @RepaymentId BIGINT=SCOPE_IDENTITY();
        UPDATE dbo.CustomerCredit SET CurrentOutstandingAmount=CurrentOutstandingAmount-@PaymentAmount,UpdatedDate=SYSUTCDATETIME(),UpdatedByUserId=@CreatedByUserId WHERE CustomerId=@CustomerId;
        INSERT dbo.CustomerCreditTransaction(CustomerId,TransactionType,ReferenceType,ReferenceId,ReferenceNo,Amount,BalanceBefore,BalanceAfter,Status,Remark,Description,CreatedByUserId)
        VALUES(@CustomerId,N'CreditRepaid',N'CreditRepayment',@RepaymentId,@RepaymentNo,@PaymentAmount,@Before,@Before-@PaymentAmount,N'Paid',@Remark,N'Customer credit repayment.',@CreatedByUserId);
        COMMIT;
        SELECT @RepaymentId CustomerCreditRepaymentId, @RepaymentNo RepaymentNo;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerCreditRepaymentVoid
    @CustomerCreditRepaymentId BIGINT,@Reason NVARCHAR(500),@UpdatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @CustomerId INT,@Amount DECIMAL(18,2),@RepaymentNo NVARCHAR(50),@Status NVARCHAR(30),@Before DECIMAL(18,2);
        SELECT @CustomerId=CustomerId,@Amount=PaymentAmount,@RepaymentNo=RepaymentNo,@Status=Status FROM dbo.CustomerCreditRepayment WITH(UPDLOCK,HOLDLOCK) WHERE CustomerCreditRepaymentId=@CustomerCreditRepaymentId;
        IF @CustomerId IS NULL THROW 53130, 'Repayment was not found.', 1;
        IF @Status=N'Voided' THROW 53131, 'Repayment is already voided.', 1;
        SELECT @Before=CurrentOutstandingAmount FROM dbo.CustomerCredit WITH(UPDLOCK,HOLDLOCK) WHERE CustomerId=@CustomerId;
        UPDATE dbo.CustomerCredit SET CurrentOutstandingAmount=CurrentOutstandingAmount+@Amount,UpdatedDate=SYSUTCDATETIME(),UpdatedByUserId=@UpdatedByUserId WHERE CustomerId=@CustomerId;
        INSERT dbo.CustomerCreditTransaction(CustomerId,TransactionType,ReferenceType,ReferenceId,ReferenceNo,Amount,BalanceBefore,BalanceAfter,Status,Remark,Description,CreatedByUserId)
        VALUES(@CustomerId,N'CreditAdjusted',N'CreditRepayment',@CustomerCreditRepaymentId,@RepaymentNo,@Amount,@Before,@Before+@Amount,N'Paid',@Reason,N'Voided credit repayment reversal.',@UpdatedByUserId);
        UPDATE dbo.CustomerCreditRepayment SET Status=N'Voided',Remark=CONCAT(ISNULL(Remark,N''),N' | Voided: ',@Reason),UpdatedByUserId=@UpdatedByUserId,UpdatedDate=SYSUTCDATETIME() WHERE CustomerCreditRepaymentId=@CustomerCreditRepaymentId;
        COMMIT;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH
END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerCreditCreateSale @CustomerId INT,@SaleId BIGINT,@Amount DECIMAL(18,2),@ReferenceNo NVARCHAR(100)=NULL,@ManagerApproved BIT=0,@Remark NVARCHAR(1000)=NULL,@CreatedByUserId INT AS
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
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerCreditReceivePayment @CustomerId INT,@Amount DECIMAL(18,2),@PaidDate DATETIME2=NULL,@ReferenceNo NVARCHAR(100)=NULL,@Remark NVARCHAR(1000)=NULL,@CreatedByUserId INT AS
BEGIN
    SET XACT_ABORT ON; DECLARE @Outstanding DECIMAL(18,2); SELECT @Outstanding=CurrentOutstandingAmount FROM dbo.CustomerCredit WITH(UPDLOCK,ROWLOCK) WHERE CustomerId=@CustomerId;
    IF @Amount>@Outstanding THROW 51402,'Payment cannot exceed outstanding amount.',1;
    BEGIN TRAN; UPDATE dbo.CustomerCredit SET CurrentOutstandingAmount=CurrentOutstandingAmount-@Amount,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@CreatedByUserId WHERE CustomerId=@CustomerId;
    INSERT dbo.CustomerCreditTransaction(CustomerId,TransactionType,Amount,PaidDate,ReferenceNo,Status,Remark,CreatedByUserId) VALUES(@CustomerId,N'Payment',@Amount,ISNULL(@PaidDate,SYSDATETIME()),@ReferenceNo,N'Paid',@Remark,@CreatedByUserId); COMMIT;
END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerCreditAdjust @CustomerId INT,@AdjustmentType NVARCHAR(30),@Amount DECIMAL(18,2),@ReferenceNo NVARCHAR(100)=NULL,@Remark NVARCHAR(1000)=NULL,@CreatedByUserId INT AS
BEGIN
    SET XACT_ABORT ON; DECLARE @Outstanding DECIMAL(18,2); SELECT @Outstanding=CurrentOutstandingAmount FROM dbo.CustomerCredit WITH(UPDLOCK,ROWLOCK) WHERE CustomerId=@CustomerId;
    IF @AdjustmentType=N'AdjustmentOut' AND @Amount>@Outstanding THROW 51403,'Adjustment cannot make outstanding negative.',1;
    BEGIN TRAN; UPDATE dbo.CustomerCredit SET CurrentOutstandingAmount=CurrentOutstandingAmount+CASE WHEN @AdjustmentType=N'AdjustmentIn' THEN @Amount ELSE -@Amount END,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@CreatedByUserId WHERE CustomerId=@CustomerId;
    INSERT dbo.CustomerCreditTransaction(CustomerId,TransactionType,Amount,ReferenceNo,Status,Remark,CreatedByUserId) VALUES(@CustomerId,@AdjustmentType,@Amount,@ReferenceNo,N'Paid',@Remark,@CreatedByUserId); COMMIT;
END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerCreditGetTransactionsPaged @CustomerId INT,@PageNumber INT,@PageSize INT,@DateFrom DATETIME2=NULL,@DateTo DATETIME2=NULL,@TransactionType NVARCHAR(30)=NULL,@Status NVARCHAR(30)=NULL AS
BEGIN SELECT *,COUNT(1) OVER() TotalCount FROM dbo.CustomerCreditTransaction WHERE CustomerId=@CustomerId AND (@TransactionType IS NULL OR TransactionType=@TransactionType) AND (@Status IS NULL OR Status=@Status) AND (@DateFrom IS NULL OR CreatedDate>=@DateFrom) AND (@DateTo IS NULL OR CreatedDate<DATEADD(DAY,1,@DateTo)) ORDER BY CreatedDate DESC OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY; END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerCreditUpdateOverdueStatus AS
BEGIN UPDATE dbo.CustomerCreditTransaction SET Status=N'Overdue' WHERE Status IN(N'Unpaid',N'PartiallyPaid') AND DueDate<CONVERT(date,SYSDATETIME()); SELECT @@ROWCOUNT; END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerHistoryGetSummary @CustomerId INT,@DateFrom DATETIME2=NULL,@DateTo DATETIME2=NULL AS
BEGIN
    SELECT c.CustomerId,c.CustomerCode,c.CustomerName,c.PhoneNumber,ml.LevelName MemberLevelName,c.TotalSpending,c.TotalPurchaseCount,c.LastPurchaseDate,
           ISNULL(pb.AvailablePoints,0) AvailablePoints,ISNULL(pb.LifetimeEarnedPoints,0) LifetimeEarnedPoints,ISNULL(pb.LifetimeRedeemedPoints,0) LifetimeRedeemedPoints,
           ISNULL(cc.CreditLimit,0) CreditLimit,ISNULL(cc.CurrentOutstandingAmount,0) CurrentOutstandingAmount,ISNULL(cc.AvailableCredit,0) AvailableCredit,ISNULL(cc.CreditStatus,N'Good') CreditStatus,
           ISNULL(SUM(CASE WHEN ct.TransactionType=N'CreditSale' THEN ct.Amount ELSE 0 END),0) TotalCreditSales,
           ISNULL(SUM(CASE WHEN ct.TransactionType=N'Payment' THEN ct.Amount ELSE 0 END),0) TotalCreditPayments,
           ISNULL(SUM(CASE WHEN ct.Status=N'Overdue' THEN ct.Amount ELSE 0 END),0) OverdueAmount,
           SUM(CASE WHEN ct.Status=N'Overdue' THEN 1 ELSE 0 END) OverdueCount,
           (SELECT COUNT(1) FROM dbo.CustomerNote n WHERE n.CustomerId=@CustomerId AND n.IsActive=1 AND n.IsImportant=1) ImportantNoteCount
    FROM dbo.Customer c LEFT JOIN dbo.MemberLevel ml ON ml.MemberLevelId=c.MemberLevelId LEFT JOIN dbo.CustomerPointBalance pb ON pb.CustomerId=c.CustomerId LEFT JOIN dbo.CustomerCredit cc ON cc.CustomerId=c.CustomerId LEFT JOIN dbo.CustomerCreditTransaction ct ON ct.CustomerId=c.CustomerId AND (@DateFrom IS NULL OR ct.CreatedDate>=@DateFrom) AND (@DateTo IS NULL OR ct.CreatedDate<DATEADD(DAY,1,@DateTo))
    WHERE c.CustomerId=@CustomerId GROUP BY c.CustomerId,c.CustomerCode,c.CustomerName,c.PhoneNumber,ml.LevelName,c.TotalSpending,c.TotalPurchaseCount,c.LastPurchaseDate,pb.AvailablePoints,pb.LifetimeEarnedPoints,pb.LifetimeRedeemedPoints,cc.CreditLimit,cc.CurrentOutstandingAmount,cc.AvailableCredit,cc.CreditStatus;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerHistoryGetCreditHistory @CustomerId INT,@DateFrom DATETIME2=NULL,@DateTo DATETIME2=NULL,@PageNumber INT=1,@PageSize INT=20 AS
BEGIN SELECT *,COUNT(1) OVER() TotalCount FROM dbo.CustomerCreditTransaction WHERE CustomerId=@CustomerId AND (@DateFrom IS NULL OR CreatedDate>=@DateFrom) AND (@DateTo IS NULL OR CreatedDate<DATEADD(DAY,1,@DateTo)) ORDER BY CreatedDate DESC OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY; END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerHistoryGetPointHistory @CustomerId INT,@DateFrom DATETIME2=NULL,@DateTo DATETIME2=NULL,@PageNumber INT=1,@PageSize INT=20 AS
BEGIN SELECT *,COUNT(1) OVER() TotalCount FROM dbo.CustomerPointMovement WHERE CustomerId=@CustomerId AND (@DateFrom IS NULL OR CreatedDate>=@DateFrom) AND (@DateTo IS NULL OR CreatedDate<DATEADD(DAY,1,@DateTo)) ORDER BY CreatedDate DESC OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY; END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerHistoryGetLevelHistory @CustomerId INT,@DateFrom DATETIME2=NULL,@DateTo DATETIME2=NULL,@PageNumber INT=1,@PageSize INT=20 AS
BEGIN SELECT h.*,old.LevelName OldMemberLevelName,new.LevelName NewMemberLevelName,CONVERT(NVARCHAR(255),h.ChangedByUserId) ChangedByName,COUNT(1) OVER() TotalCount FROM dbo.CustomerLevelHistory h LEFT JOIN dbo.MemberLevel old ON old.MemberLevelId=h.OldMemberLevelId JOIN dbo.MemberLevel new ON new.MemberLevelId=h.NewMemberLevelId WHERE h.CustomerId=@CustomerId AND (@DateFrom IS NULL OR h.ChangedDate>=@DateFrom) AND (@DateTo IS NULL OR h.ChangedDate<DATEADD(DAY,1,@DateTo)) ORDER BY h.ChangedDate DESC OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY; END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerHistoryGetPurchaseHistory @CustomerId INT,@DateFrom DATETIME2=NULL,@DateTo DATETIME2=NULL,@PageNumber INT=1,@PageSize INT=20 AS
BEGIN
    SELECT CAST(NULL AS BIGINT) SaleId,CAST(NULL AS NVARCHAR(100)) SaleNo,CAST(NULL AS DATETIME2) SaleDate,CAST(0 AS DECIMAL(18,2)) TotalAmount,CAST(0 AS DECIMAL(18,2)) DiscountAmount,CAST(0 AS DECIMAL(18,2)) NetAmount,CAST(NULL AS NVARCHAR(30)) PaymentStatus,CAST(NULL AS NVARCHAR(30)) SaleStatus,CAST(NULL AS NVARCHAR(255)) CreatedByName,CAST(0 AS INT) TotalCount WHERE 1=0;
END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerHistoryGetPaymentHistory @CustomerId INT,@DateFrom DATETIME2=NULL,@DateTo DATETIME2=NULL,@PageNumber INT=1,@PageSize INT=20 AS
BEGIN
    SELECT CAST(NULL AS BIGINT) PaymentId,CAST(NULL AS BIGINT) SaleId,CAST(NULL AS NVARCHAR(100)) SaleNo,CAST(NULL AS DATETIME2) PaymentDate,CAST(NULL AS NVARCHAR(50)) PaymentMethod,CAST(0 AS DECIMAL(18,2)) Amount,CAST(NULL AS NVARCHAR(100)) ReferenceNo,CAST(NULL AS NVARCHAR(30)) PaymentStatus,CAST(NULL AS NVARCHAR(255)) CreatedByName,CAST(0 AS INT) TotalCount WHERE 1=0;
END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerHistoryGetRefundHistory @CustomerId INT,@DateFrom DATETIME2=NULL,@DateTo DATETIME2=NULL,@PageNumber INT=1,@PageSize INT=20 AS
BEGIN
    SELECT CAST(NULL AS BIGINT) RefundId,CAST(NULL AS BIGINT) SaleId,CAST(NULL AS NVARCHAR(100)) SaleNo,CAST(NULL AS DATETIME2) RefundDate,CAST(0 AS DECIMAL(18,2)) RefundAmount,CAST(NULL AS NVARCHAR(500)) RefundReason,CAST(NULL AS NVARCHAR(30)) RefundStatus,CAST(NULL AS NVARCHAR(255)) CreatedByName,CAST(0 AS INT) TotalCount WHERE 1=0;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerHistoryGetTimeline @CustomerId INT,@DateFrom DATETIME2=NULL,@DateTo DATETIME2=NULL,@HistoryType NVARCHAR(30)=NULL,@PageNumber INT=1,@PageSize INT=20 AS
BEGIN
    ;WITH t AS
    (
        SELECT CreatedDate ActivityDate,N'Credit' HistoryType,CustomerCreditTransactionId ReferenceId,ReferenceNo,TransactionType Title,Remark Description,Amount,CAST(NULL AS DECIMAL(18,2)) Points,Status,CONVERT(NVARCHAR(255),CreatedByUserId) CreatedByName FROM dbo.CustomerCreditTransaction WHERE CustomerId=@CustomerId
        UNION ALL SELECT CreatedDate,N'Point',CustomerPointMovementId,ReferenceNo,MovementType,Remark,NULL,PointsIn-PointsOut,NULL,CONVERT(NVARCHAR(255),CreatedByUserId) FROM dbo.CustomerPointMovement WHERE CustomerId=@CustomerId
        UNION ALL SELECT ChangedDate,N'MemberLevel',CustomerLevelHistoryId,NULL,N'Member level changed',ChangeReason,NULL,NULL,NULL,CONVERT(NVARCHAR(255),ChangedByUserId) FROM dbo.CustomerLevelHistory WHERE CustomerId=@CustomerId
        UNION ALL SELECT CreatedDate,N'Note',CustomerNoteId,NULL,NoteType,NoteText,NULL,NULL,CASE WHEN IsActive=1 THEN N'Active' ELSE N'Inactive' END,CONVERT(NVARCHAR(255),CreatedByUserId) FROM dbo.CustomerNote WHERE CustomerId=@CustomerId
    )
    SELECT *,COUNT(1) OVER() TotalCount FROM t WHERE (@HistoryType IS NULL OR HistoryType=@HistoryType) AND (@DateFrom IS NULL OR ActivityDate>=@DateFrom) AND (@DateTo IS NULL OR ActivityDate<DATEADD(DAY,1,@DateTo)) ORDER BY ActivityDate DESC OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerNoteGetPaged @CustomerId INT,@PageNumber INT,@PageSize INT,@NoteType NVARCHAR(30)=NULL,@IsActive BIT=NULL AS
BEGIN SELECT *,COUNT(1) OVER() TotalCount FROM dbo.CustomerNote WHERE CustomerId=@CustomerId AND (@NoteType IS NULL OR NoteType=@NoteType) AND (@IsActive IS NULL OR IsActive=@IsActive) ORDER BY IsImportant DESC,CreatedDate DESC OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY; END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerNoteGetById @CustomerNoteId BIGINT AS BEGIN SELECT * FROM dbo.CustomerNote WHERE CustomerNoteId=@CustomerNoteId; END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerNoteCreate @CustomerId INT,@NoteType NVARCHAR(30),@NoteText NVARCHAR(2000),@IsImportant BIT=0,@CreatedByUserId INT AS
BEGIN INSERT dbo.CustomerNote(CustomerId,NoteType,NoteText,IsImportant,CreatedByUserId) VALUES(@CustomerId,@NoteType,@NoteText,@IsImportant,@CreatedByUserId); SELECT CONVERT(BIGINT,SCOPE_IDENTITY()); END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerNoteUpdate @CustomerNoteId BIGINT,@NoteType NVARCHAR(30),@NoteText NVARCHAR(2000),@IsImportant BIT=0,@IsActive BIT=1,@UpdatedByUserId INT AS
BEGIN UPDATE dbo.CustomerNote SET NoteType=@NoteType,NoteText=@NoteText,IsImportant=@IsImportant,IsActive=@IsActive,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@UpdatedByUserId WHERE CustomerNoteId=@CustomerNoteId; END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerNoteToggleActive @CustomerNoteId BIGINT,@IsActive BIT,@UpdatedByUserId INT AS
BEGIN UPDATE dbo.CustomerNote SET IsActive=@IsActive,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@UpdatedByUserId WHERE CustomerNoteId=@CustomerNoteId; END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerReportGetSummary @DateFrom DATETIME2=NULL,@DateTo DATETIME2=NULL,@MemberLevelId INT=NULL,@IsActive BIT=NULL,@Top INT=20,@NoPurchaseAfterDate DATETIME2=NULL AS
BEGIN
    SELECT COUNT(1) TotalCustomers,SUM(CASE WHEN c.IsActive=1 THEN 1 ELSE 0 END) ActiveCustomers,SUM(CASE WHEN (@DateFrom IS NOT NULL AND c.CreatedDate>=@DateFrom AND (@DateTo IS NULL OR c.CreatedDate<DATEADD(DAY,1,@DateTo))) THEN 1 ELSE 0 END) NewCustomers,
           SUM(c.TotalSpending) TotalCustomerSpending,ISNULL(SUM(cc.CurrentOutstandingAmount),0) TotalOutstandingCredit,ISNULL(SUM(pb.AvailablePoints),0) TotalAvailablePoints,
           SUM(CASE WHEN cc.AllowCredit=1 THEN 1 ELSE 0 END) TotalCreditCustomers,
           SUM(CASE WHEN overdue.CustomerId IS NULL THEN 0 ELSE 1 END) TotalOverdueCustomers
    FROM dbo.Customer c LEFT JOIN dbo.CustomerCredit cc ON cc.CustomerId=c.CustomerId LEFT JOIN dbo.CustomerPointBalance pb ON pb.CustomerId=c.CustomerId
    LEFT JOIN (SELECT DISTINCT CustomerId FROM dbo.CustomerCreditTransaction WHERE Status=N'Overdue') overdue ON overdue.CustomerId=c.CustomerId
    WHERE (@MemberLevelId IS NULL OR c.MemberLevelId=@MemberLevelId) AND (@IsActive IS NULL OR c.IsActive=@IsActive);
END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerReportTopCustomersBySpending @DateFrom DATETIME2=NULL,@DateTo DATETIME2=NULL,@MemberLevelId INT=NULL,@IsActive BIT=NULL,@Top INT=20,@NoPurchaseAfterDate DATETIME2=NULL AS
BEGIN SELECT TOP(@Top) c.CustomerId,c.CustomerCode,c.CustomerName,ml.LevelName MemberLevelName,c.TotalSpending,c.TotalPurchaseCount,c.LastPurchaseDate FROM dbo.Customer c LEFT JOIN dbo.MemberLevel ml ON ml.MemberLevelId=c.MemberLevelId WHERE (@MemberLevelId IS NULL OR c.MemberLevelId=@MemberLevelId) AND (@IsActive IS NULL OR c.IsActive=@IsActive) ORDER BY c.TotalSpending DESC; END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerReportTopCustomersByVisit @DateFrom DATETIME2=NULL,@DateTo DATETIME2=NULL,@MemberLevelId INT=NULL,@IsActive BIT=NULL,@Top INT=20,@NoPurchaseAfterDate DATETIME2=NULL AS
BEGIN SELECT TOP(@Top) c.CustomerId,c.CustomerCode,c.CustomerName,ml.LevelName MemberLevelName,c.TotalSpending,c.TotalPurchaseCount,c.LastPurchaseDate FROM dbo.Customer c LEFT JOIN dbo.MemberLevel ml ON ml.MemberLevelId=c.MemberLevelId WHERE (@MemberLevelId IS NULL OR c.MemberLevelId=@MemberLevelId) AND (@IsActive IS NULL OR c.IsActive=@IsActive) ORDER BY c.TotalPurchaseCount DESC; END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerReportMemberLevelSummary @DateFrom DATETIME2=NULL,@DateTo DATETIME2=NULL,@MemberLevelId INT=NULL,@IsActive BIT=NULL,@Top INT=20,@NoPurchaseAfterDate DATETIME2=NULL AS
BEGIN SELECT c.MemberLevelId,ISNULL(ml.LevelName,N'No Level') MemberLevelName,COUNT(1) CustomerCount,SUM(c.TotalSpending) TotalSpending,ISNULL(SUM(cc.CurrentOutstandingAmount),0) TotalOutstandingCredit FROM dbo.Customer c LEFT JOIN dbo.MemberLevel ml ON ml.MemberLevelId=c.MemberLevelId LEFT JOIN dbo.CustomerCredit cc ON cc.CustomerId=c.CustomerId WHERE (@MemberLevelId IS NULL OR c.MemberLevelId=@MemberLevelId) AND (@IsActive IS NULL OR c.IsActive=@IsActive) GROUP BY c.MemberLevelId,ml.LevelName; END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerReportLoyaltyPointSummary @DateFrom DATETIME2=NULL,@DateTo DATETIME2=NULL,@MemberLevelId INT=NULL,@IsActive BIT=NULL,@Top INT=20,@NoPurchaseAfterDate DATETIME2=NULL AS
BEGIN SELECT ISNULL(SUM(LifetimeEarnedPoints),0) TotalEarnedPoints,ISNULL(SUM(LifetimeRedeemedPoints),0) TotalRedeemedPoints,ISNULL(SUM(AvailablePoints),0) TotalAvailablePoints,ISNULL((SELECT SUM(PointsOut) FROM dbo.CustomerPointMovement WHERE MovementType=N'Expire'),0) TotalExpiredPoints FROM dbo.CustomerPointBalance; END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerReportCreditSummary @DateFrom DATETIME2=NULL,@DateTo DATETIME2=NULL,@MemberLevelId INT=NULL,@IsActive BIT=NULL,@Top INT=20,@NoPurchaseAfterDate DATETIME2=NULL AS
BEGIN SELECT ISNULL(SUM(CreditLimit),0) TotalCreditLimit,ISNULL(SUM(CurrentOutstandingAmount),0) TotalOutstandingAmount,ISNULL(SUM(AvailableCredit),0) TotalAvailableCredit,ISNULL((SELECT SUM(Amount) FROM dbo.CustomerCreditTransaction WHERE Status=N'Overdue'),0) OverdueAmount,ISNULL((SELECT COUNT(1) FROM dbo.CustomerCreditTransaction WHERE Status=N'Overdue'),0) OverdueCount,SUM(CASE WHEN CreditStatus=N'Blocked' THEN 1 ELSE 0 END) BlockedCustomerCount FROM dbo.CustomerCredit; END
GO
CREATE OR ALTER PROCEDURE dbo.spCustomerReportInactiveCustomers @DateFrom DATETIME2=NULL,@DateTo DATETIME2=NULL,@MemberLevelId INT=NULL,@IsActive BIT=NULL,@Top INT=20,@NoPurchaseAfterDate DATETIME2=NULL AS
BEGIN SELECT c.CustomerId,c.CustomerCode,c.CustomerName,c.PhoneNumber,ml.LevelName MemberLevelName,c.TotalSpending,c.TotalPurchaseCount,c.LastPurchaseDate FROM dbo.Customer c LEFT JOIN dbo.MemberLevel ml ON ml.MemberLevelId=c.MemberLevelId WHERE (@MemberLevelId IS NULL OR c.MemberLevelId=@MemberLevelId) AND (@IsActive IS NULL OR c.IsActive=@IsActive) AND (@NoPurchaseAfterDate IS NULL OR c.LastPurchaseDate IS NULL OR c.LastPurchaseDate<@NoPurchaseAfterDate) ORDER BY c.LastPurchaseDate; END
GO
