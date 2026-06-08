/*
    Customer multi-membership deployment
    Rollback guidance:
    - This script is additive for membership/profile/loyalty objects and preserves dbo.Customer.CustomerId.
    - To roll back UI-facing procedure behavior, restore previous definitions of spCustomerGetPaged,
      spCustomerGetById, and spCustomerReportGetSummary from source control.
    - Do not drop ledger tables after production use unless their history has been archived.
*/

SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
GO

IF OBJECT_ID(N'dbo.MemberType', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MemberType
    (
        MemberTypeId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MemberType PRIMARY KEY,
        MemberTypeCode NVARCHAR(50) NOT NULL,
        MemberTypeName NVARCHAR(100) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_MemberType_IsActive DEFAULT(1),
        CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_MemberType_CreatedDate DEFAULT(SYSDATETIME()),
        UpdatedDate DATETIME2(0) NULL,
        CONSTRAINT UQ_MemberType_Code UNIQUE(MemberTypeCode)
    );
END
GO

MERGE dbo.MemberType AS target
USING (VALUES
    (N'RETAIL', N'Retail Member'),
    (N'WHOLESALE', N'Wholesale Member'),
    (N'RUBBER_SUPPLIER', N'Rubber Supplier Member')
) AS source(MemberTypeCode, MemberTypeName)
ON target.MemberTypeCode = source.MemberTypeCode
WHEN MATCHED THEN UPDATE SET MemberTypeName = source.MemberTypeName, IsActive = 1, UpdatedDate = SYSDATETIME()
WHEN NOT MATCHED THEN INSERT(MemberTypeCode, MemberTypeName) VALUES(source.MemberTypeCode, source.MemberTypeName);
GO

IF OBJECT_ID(N'dbo.CustomerMemberType', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CustomerMemberType
    (
        CustomerMemberTypeId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerMemberType PRIMARY KEY,
        CustomerId INT NOT NULL,
        MemberTypeId INT NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_CustomerMemberType_IsActive DEFAULT(1),
        StartDate DATE NOT NULL CONSTRAINT DF_CustomerMemberType_StartDate DEFAULT(CONVERT(date, SYSDATETIME())),
        EndDate DATE NULL,
        CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_CustomerMemberType_CreatedDate DEFAULT(SYSDATETIME()),
        UpdatedDate DATETIME2(0) NULL,
        CreatedByUserId INT NULL,
        UpdatedByUserId INT NULL,
        CONSTRAINT FK_CustomerMemberType_Customer FOREIGN KEY(CustomerId) REFERENCES dbo.Customer(CustomerId),
        CONSTRAINT FK_CustomerMemberType_MemberType FOREIGN KEY(MemberTypeId) REFERENCES dbo.MemberType(MemberTypeId),
        CONSTRAINT UQ_CustomerMemberType UNIQUE(CustomerId, MemberTypeId),
        CONSTRAINT CK_CustomerMemberType_Date CHECK(EndDate IS NULL OR EndDate >= StartDate)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CustomerMemberType_ActiveLookup' AND object_id = OBJECT_ID(N'dbo.CustomerMemberType'))
    CREATE INDEX IX_CustomerMemberType_ActiveLookup ON dbo.CustomerMemberType(CustomerId, IsActive) INCLUDE(MemberTypeId);
GO

INSERT dbo.CustomerMemberType(CustomerId, MemberTypeId, IsActive, StartDate, CreatedByUserId)
SELECT c.CustomerId, mt.MemberTypeId, 1, CONVERT(date, ISNULL(c.CreatedDate, SYSDATETIME())), c.CreatedByUserId
FROM dbo.Customer c
JOIN dbo.MemberType mt ON mt.MemberTypeCode = CASE WHEN c.MemberType = N'Wholesale' THEN N'WHOLESALE' ELSE N'RETAIL' END
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.CustomerMemberType cmt
    WHERE cmt.CustomerId = c.CustomerId AND cmt.MemberTypeId = mt.MemberTypeId
);
GO

IF OBJECT_ID(N'dbo.WholesaleMemberProfile', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WholesaleMemberProfile
    (
        CustomerId INT NOT NULL CONSTRAINT PK_WholesaleMemberProfile PRIMARY KEY,
        BusinessName NVARCHAR(255) NULL,
        WholesaleLevelId INT NULL,
        IsApproved BIT NOT NULL CONSTRAINT DF_WholesaleMemberProfile_IsApproved DEFAULT(0),
        PaymentTermDays INT NOT NULL CONSTRAINT DF_WholesaleMemberProfile_PaymentTermDays DEFAULT(0),
        ApprovedByUserId INT NULL,
        ApprovedDate DATETIME2(0) NULL,
        CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_WholesaleMemberProfile_CreatedDate DEFAULT(SYSDATETIME()),
        UpdatedDate DATETIME2(0) NULL,
        CONSTRAINT FK_WholesaleMemberProfile_Customer FOREIGN KEY(CustomerId) REFERENCES dbo.Customer(CustomerId),
        CONSTRAINT CK_WholesaleMemberProfile_Term CHECK(PaymentTermDays >= 0)
    );
END
GO

IF OBJECT_ID(N'dbo.RubberSupplierMemberProfile', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RubberSupplierMemberProfile
    (
        CustomerId INT NOT NULL CONSTRAINT PK_RubberSupplierMemberProfile PRIMARY KEY,
        SupplierCode NVARCHAR(50) NOT NULL,
        DefaultBusinessLocationId INT NULL,
        Remark NVARCHAR(1000) NULL,
        CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_RubberSupplierMemberProfile_CreatedDate DEFAULT(SYSDATETIME()),
        UpdatedDate DATETIME2(0) NULL,
        CONSTRAINT FK_RubberSupplierMemberProfile_Customer FOREIGN KEY(CustomerId) REFERENCES dbo.Customer(CustomerId),
        CONSTRAINT UQ_RubberSupplierMemberProfile_SupplierCode UNIQUE(SupplierCode)
    );
END
GO

IF COL_LENGTH(N'dbo.RubberSupplierMemberProfile', N'BankName') IS NOT NULL
    ALTER TABLE dbo.RubberSupplierMemberProfile DROP COLUMN BankName;
IF COL_LENGTH(N'dbo.RubberSupplierMemberProfile', N'BankAccountName') IS NOT NULL
    ALTER TABLE dbo.RubberSupplierMemberProfile DROP COLUMN BankAccountName;
IF COL_LENGTH(N'dbo.RubberSupplierMemberProfile', N'BankAccountNumber') IS NOT NULL
    ALTER TABLE dbo.RubberSupplierMemberProfile DROP COLUMN BankAccountNumber;
GO

IF OBJECT_ID(N'dbo.LoyaltyRule', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LoyaltyRule
    (
        LoyaltyRuleId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoyaltyRule PRIMARY KEY,
        RuleCode NVARCHAR(50) NOT NULL,
        RuleName NVARCHAR(100) NOT NULL,
        WeightKgPerPoint DECIMAL(18,4) NOT NULL,
        IsCarryForwardEnabled BIT NOT NULL CONSTRAINT DF_LoyaltyRule_Carry DEFAULT(1),
        IsActive BIT NOT NULL CONSTRAINT DF_LoyaltyRule_IsActive DEFAULT(1),
        EffectiveFrom DATETIME2(0) NOT NULL CONSTRAINT DF_LoyaltyRule_EffectiveFrom DEFAULT(SYSDATETIME()),
        EffectiveTo DATETIME2(0) NULL,
        CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_LoyaltyRule_CreatedDate DEFAULT(SYSDATETIME()),
        CONSTRAINT UQ_LoyaltyRule_Code UNIQUE(RuleCode, EffectiveFrom),
        CONSTRAINT CK_LoyaltyRule_Weight CHECK(WeightKgPerPoint > 0),
        CONSTRAINT CK_LoyaltyRule_Effective CHECK(EffectiveTo IS NULL OR EffectiveTo > EffectiveFrom)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.LoyaltyRule WHERE RuleCode = N'RUBBER_WEIGHT_REWARD')
BEGIN
    INSERT dbo.LoyaltyRule(RuleCode, RuleName, WeightKgPerPoint, IsCarryForwardEnabled, IsActive)
    VALUES(N'RUBBER_WEIGHT_REWARD', N'Rubber weight reward', 100, 1, 1);
END
GO

IF OBJECT_ID(N'dbo.MemberLoyaltyAccount', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MemberLoyaltyAccount
    (
        MemberLoyaltyAccountId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MemberLoyaltyAccount PRIMARY KEY,
        CustomerId INT NOT NULL,
        PointBalance DECIMAL(18,2) NOT NULL CONSTRAINT DF_MemberLoyaltyAccount_Point DEFAULT(0),
        RubberWeightCarryForwardKg DECIMAL(18,4) NOT NULL CONSTRAINT DF_MemberLoyaltyAccount_Carry DEFAULT(0),
        UpdatedDate DATETIME2(0) NULL,
        CONSTRAINT UQ_MemberLoyaltyAccount_Customer UNIQUE(CustomerId),
        CONSTRAINT FK_MemberLoyaltyAccount_Customer FOREIGN KEY(CustomerId) REFERENCES dbo.Customer(CustomerId),
        CONSTRAINT CK_MemberLoyaltyAccount_Balances CHECK(PointBalance >= 0 AND RubberWeightCarryForwardKg >= 0)
    );
END
GO

IF OBJECT_ID(N'dbo.MemberLoyaltyTransaction', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MemberLoyaltyTransaction
    (
        MemberLoyaltyTransactionId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MemberLoyaltyTransaction PRIMARY KEY,
        MemberLoyaltyAccountId BIGINT NOT NULL,
        TransactionType NVARCHAR(50) NOT NULL,
        SourceType NVARCHAR(50) NOT NULL,
        ReferenceId BIGINT NULL,
        RubberWeightKg DECIMAL(18,4) NOT NULL CONSTRAINT DF_MemberLoyaltyTransaction_Weight DEFAULT(0),
        WeightKgPerPointSnapshot DECIMAL(18,4) NOT NULL CONSTRAINT DF_MemberLoyaltyTransaction_Snapshot DEFAULT(0),
        PreviousCarryForwardWeightKg DECIMAL(18,4) NOT NULL CONSTRAINT DF_MemberLoyaltyTransaction_PreviousCarry DEFAULT(0),
        CarryForwardWeightAfterKg DECIMAL(18,4) NOT NULL CONSTRAINT DF_MemberLoyaltyTransaction_AfterCarry DEFAULT(0),
        Points INT NOT NULL,
        PointBalanceAfterTransaction DECIMAL(18,2) NOT NULL,
        Remark NVARCHAR(1000) NULL,
        CreatedByUserId INT NOT NULL,
        CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_MemberLoyaltyTransaction_CreatedDate DEFAULT(SYSDATETIME()),
        CONSTRAINT FK_MemberLoyaltyTransaction_Account FOREIGN KEY(MemberLoyaltyAccountId) REFERENCES dbo.MemberLoyaltyAccount(MemberLoyaltyAccountId)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_MemberLoyaltyTransaction_RubberPurchaseEarn' AND object_id = OBJECT_ID(N'dbo.MemberLoyaltyTransaction'))
    CREATE UNIQUE INDEX UX_MemberLoyaltyTransaction_RubberPurchaseEarn ON dbo.MemberLoyaltyTransaction(SourceType, ReferenceId, TransactionType)
    WHERE SourceType = N'RUBBER_PURCHASE' AND ReferenceId IS NOT NULL AND TransactionType IN (N'EARN_FROM_RUBBER_PURCHASE', N'CANCELLATION_REVERSAL');
GO

IF OBJECT_ID(N'dbo.RubberPrice', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RubberPrice
    (
        RubberPriceId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RubberPrice PRIMARY KEY,
        PricePerKg DECIMAL(18,2) NOT NULL,
        PercentageOfService DECIMAL(5,2) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_RubberPrice_IsActive DEFAULT(1),
        CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_RubberPrice_CreatedDate DEFAULT(SYSDATETIME()),
        UpdatedDate DATETIME2(0) NULL,
        CONSTRAINT CK_RubberPrice_PricePerKg CHECK(PricePerKg >= 0),
        CONSTRAINT CK_RubberPrice_PercentageOfService CHECK(PercentageOfService >= 0 AND PercentageOfService <= 100)
    );
END
GO

IF OBJECT_ID(N'dbo.RubberAuctionLocation', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RubberAuctionLocation
    (
        RubberAuctionLocationId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RubberAuctionLocation PRIMARY KEY,
        LocationName NVARCHAR(150) NOT NULL,
        Address NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_RubberAuctionLocation_IsActive DEFAULT(1),
        CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_RubberAuctionLocation_CreatedDate DEFAULT(SYSDATETIME()),
        UpdatedDate DATETIME2(0) NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_RubberAuctionLocation_LocationName' AND object_id = OBJECT_ID(N'dbo.RubberAuctionLocation'))
    CREATE UNIQUE INDEX UX_RubberAuctionLocation_LocationName ON dbo.RubberAuctionLocation(LocationName);
GO

IF DB_ID(N'eFinanceAphiwatGroupDB') IS NOT NULL
BEGIN
    EXEC(N'
        SET IDENTITY_INSERT dbo.RubberPrice ON;
        INSERT dbo.RubberPrice(RubberPriceId, PricePerKg, PercentageOfService, IsActive, CreatedDate, UpdatedDate)
        SELECT src.RubberPriceId, src.PricePerKg, src.PercentageOfService, src.IsActive, src.CreatedDate, src.UpdatedDate
        FROM eFinanceAphiwatGroupDB.dbo.RubberPrice src
        WHERE NOT EXISTS (SELECT 1 FROM dbo.RubberPrice target WHERE target.RubberPriceId = src.RubberPriceId);
        SET IDENTITY_INSERT dbo.RubberPrice OFF;

        SET IDENTITY_INSERT dbo.RubberAuctionLocation ON;
        INSERT dbo.RubberAuctionLocation(RubberAuctionLocationId, LocationName, Address, IsActive, CreatedDate, UpdatedDate)
        SELECT src.RubberAuctionLocationId, src.LocationName, src.Address, src.IsActive, src.CreatedDate, src.UpdatedDate
        FROM eFinanceAphiwatGroupDB.dbo.RubberAuctionLocation src
        WHERE NOT EXISTS (SELECT 1 FROM dbo.RubberAuctionLocation target WHERE target.RubberAuctionLocationId = src.RubberAuctionLocationId)
          AND NOT EXISTS (SELECT 1 FROM dbo.RubberAuctionLocation target WHERE target.LocationName = src.LocationName);
        SET IDENTITY_INSERT dbo.RubberAuctionLocation OFF;
    ');
END
GO

IF OBJECT_ID(N'dbo.RubberPurchaseHeader', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RubberPurchaseHeader
    (
        RubberPurchaseHeaderId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RubberPurchaseHeader PRIMARY KEY,
        CustomerId INT NULL,
        NonMemberFarmerName NVARCHAR(255) NULL,
        NonMemberFarmerPhone NVARCHAR(50) NULL,
        BusinessLocationId INT NOT NULL,
        RubberAuctionLocationId INT NULL,
        TransactionDate DATETIME2(0) NOT NULL,
        WeightKg DECIMAL(18,4) NOT NULL,
        RubberPriceId INT NULL,
        MarketingPriceId BIGINT NULL,
        PricePerKgSnapshot DECIMAL(18,4) NULL,
        PercentageSnapshot DECIMAL(18,4) NULL,
        TotalAmount DECIMAL(18,2) NULL,
        PaymentStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_RubberPurchaseHeader_PaymentStatus DEFAULT(N'Pending'),
        ReceiptNo NVARCHAR(30) NULL,
        PaidAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_RubberPurchaseHeader_PaidAmount DEFAULT(0),
        CreditDeductedAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_RubberPurchaseHeader_CreditDeducted DEFAULT(0),
        PaidDate DATETIME2(0) NULL,
        PaymentMethod NVARCHAR(30) NULL,
        PaymentRemark NVARCHAR(500) NULL,
        CreatedByUserId INT NOT NULL,
        CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_RubberPurchaseHeader_CreatedDate DEFAULT(SYSDATETIME()),
        CONSTRAINT FK_RubberPurchaseHeader_Customer FOREIGN KEY(CustomerId) REFERENCES dbo.Customer(CustomerId),
        CONSTRAINT FK_RubberPurchaseHeader_Location FOREIGN KEY(BusinessLocationId) REFERENCES dbo.InventoryLocation(LocationId),
        CONSTRAINT FK_RubberPurchaseHeader_AuctionLocation FOREIGN KEY(RubberAuctionLocationId) REFERENCES dbo.RubberAuctionLocation(RubberAuctionLocationId),
        CONSTRAINT FK_RubberPurchaseHeader_RubberPrice FOREIGN KEY(RubberPriceId) REFERENCES dbo.RubberPrice(RubberPriceId),
        CONSTRAINT CK_RubberPurchaseHeader_Weight CHECK(WeightKg > 0),
        CONSTRAINT CK_RubberPurchaseHeader_Total CHECK(TotalAmount IS NULL OR TotalAmount >= 0),
        CONSTRAINT CK_RubberPurchaseHeader_PaymentStatus CHECK(PaymentStatus IN (N'Pending', N'Paid', N'Cancelled'))
    );
END
GO

IF COL_LENGTH(N'dbo.RubberPurchaseHeader', N'RubberAuctionLocationId') IS NULL
    ALTER TABLE dbo.RubberPurchaseHeader ADD RubberAuctionLocationId INT NULL;
IF COL_LENGTH(N'dbo.RubberPurchaseHeader', N'NonMemberFarmerName') IS NULL
    ALTER TABLE dbo.RubberPurchaseHeader ADD NonMemberFarmerName NVARCHAR(255) NULL;
IF COL_LENGTH(N'dbo.RubberPurchaseHeader', N'NonMemberFarmerPhone') IS NULL
    ALTER TABLE dbo.RubberPurchaseHeader ADD NonMemberFarmerPhone NVARCHAR(50) NULL;
IF COL_LENGTH(N'dbo.RubberPurchaseHeader', N'CustomerId') IS NOT NULL
    ALTER TABLE dbo.RubberPurchaseHeader ALTER COLUMN CustomerId INT NULL;
IF COL_LENGTH(N'dbo.RubberPurchaseHeader', N'RubberPriceId') IS NULL
    ALTER TABLE dbo.RubberPurchaseHeader ADD RubberPriceId INT NULL;
IF COL_LENGTH(N'dbo.RubberPurchaseHeader', N'ReceiptNo') IS NULL
    ALTER TABLE dbo.RubberPurchaseHeader ADD ReceiptNo NVARCHAR(30) NULL;
IF COL_LENGTH(N'dbo.RubberPurchaseHeader', N'PaidAmount') IS NULL
    ALTER TABLE dbo.RubberPurchaseHeader ADD PaidAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_RubberPurchaseHeader_PaidAmount_Live DEFAULT(0);
IF COL_LENGTH(N'dbo.RubberPurchaseHeader', N'CreditDeductedAmount') IS NULL
    ALTER TABLE dbo.RubberPurchaseHeader ADD CreditDeductedAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_RubberPurchaseHeader_CreditDeducted_Live DEFAULT(0);
IF COL_LENGTH(N'dbo.RubberPurchaseHeader', N'PaidDate') IS NULL
    ALTER TABLE dbo.RubberPurchaseHeader ADD PaidDate DATETIME2(0) NULL;
IF COL_LENGTH(N'dbo.RubberPurchaseHeader', N'PaymentMethod') IS NULL
    ALTER TABLE dbo.RubberPurchaseHeader ADD PaymentMethod NVARCHAR(30) NULL;
IF COL_LENGTH(N'dbo.RubberPurchaseHeader', N'PaymentRemark') IS NULL
    ALTER TABLE dbo.RubberPurchaseHeader ADD PaymentRemark NVARCHAR(500) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RubberPurchaseHeader_AuctionLocation')
    ALTER TABLE dbo.RubberPurchaseHeader ADD CONSTRAINT FK_RubberPurchaseHeader_AuctionLocation FOREIGN KEY(RubberAuctionLocationId) REFERENCES dbo.RubberAuctionLocation(RubberAuctionLocationId);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RubberPurchaseHeader_RubberPrice')
    ALTER TABLE dbo.RubberPurchaseHeader ADD CONSTRAINT FK_RubberPurchaseHeader_RubberPrice FOREIGN KEY(RubberPriceId) REFERENCES dbo.RubberPrice(RubberPriceId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RubberPurchaseHeader_Customer_Date' AND object_id = OBJECT_ID(N'dbo.RubberPurchaseHeader'))
    CREATE INDEX IX_RubberPurchaseHeader_Customer_Date ON dbo.RubberPurchaseHeader(CustomerId, TransactionDate DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RubberPurchaseHeader_Location_Date' AND object_id = OBJECT_ID(N'dbo.RubberPurchaseHeader'))
    CREATE INDEX IX_RubberPurchaseHeader_Location_Date ON dbo.RubberPurchaseHeader(BusinessLocationId, TransactionDate DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RubberPurchaseHeader_AuctionLocation_Date' AND object_id = OBJECT_ID(N'dbo.RubberPurchaseHeader'))
    CREATE INDEX IX_RubberPurchaseHeader_AuctionLocation_Date ON dbo.RubberPurchaseHeader(RubberAuctionLocationId, TransactionDate DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RubberPurchaseHeader_ReceiptNo' AND object_id = OBJECT_ID(N'dbo.RubberPurchaseHeader'))
    CREATE UNIQUE INDEX IX_RubberPurchaseHeader_ReceiptNo ON dbo.RubberPurchaseHeader(ReceiptNo) WHERE ReceiptNo IS NOT NULL;
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerMemberTypeGetAllActive
AS
BEGIN
    SET NOCOUNT ON;
    SELECT MemberTypeId, MemberTypeCode, MemberTypeName, IsActive
    FROM dbo.MemberType
    WHERE IsActive = 1
    ORDER BY CASE MemberTypeCode WHEN N'RETAIL' THEN 1 WHEN N'WHOLESALE' THEN 2 WHEN N'RUBBER_SUPPLIER' THEN 3 ELSE 99 END;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerMemberTypeGetByCustomerId @CustomerId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT cmt.CustomerMemberTypeId, cmt.CustomerId, cmt.MemberTypeId, mt.MemberTypeCode, mt.MemberTypeName, cmt.IsActive, cmt.StartDate, cmt.EndDate
    FROM dbo.CustomerMemberType cmt
    JOIN dbo.MemberType mt ON mt.MemberTypeId = cmt.MemberTypeId
    WHERE cmt.CustomerId = @CustomerId AND cmt.IsActive = 1
    ORDER BY mt.MemberTypeCode;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerMemberTypeCheckActiveMembership @CustomerId INT, @MemberTypeCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CONVERT(BIT, CASE WHEN EXISTS
    (
        SELECT 1
        FROM dbo.CustomerMemberType cmt
        JOIN dbo.MemberType mt ON mt.MemberTypeId = cmt.MemberTypeId
        WHERE cmt.CustomerId = @CustomerId AND mt.MemberTypeCode = @MemberTypeCode AND cmt.IsActive = 1
    ) THEN 1 ELSE 0 END);
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerMemberTypeAssign
    @CustomerId INT,
    @MemberTypeCode NVARCHAR(50),
    @CreatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @MemberTypeId INT = (SELECT MemberTypeId FROM dbo.MemberType WHERE MemberTypeCode = @MemberTypeCode AND IsActive = 1);
    IF @MemberTypeId IS NULL THROW 52000, 'Member type does not exist or is inactive.', 1;

    BEGIN TRAN;
    IF EXISTS (SELECT 1 FROM dbo.CustomerMemberType WHERE CustomerId = @CustomerId AND MemberTypeId = @MemberTypeId)
    BEGIN
        UPDATE dbo.CustomerMemberType
        SET IsActive = 1, EndDate = NULL, UpdatedDate = SYSDATETIME(), UpdatedByUserId = @CreatedByUserId
        WHERE CustomerId = @CustomerId AND MemberTypeId = @MemberTypeId;
    END
    ELSE
    BEGIN
        INSERT dbo.CustomerMemberType(CustomerId, MemberTypeId, IsActive, StartDate, CreatedByUserId)
        VALUES(@CustomerId, @MemberTypeId, 1, CONVERT(date, SYSDATETIME()), @CreatedByUserId);
    END

    IF @MemberTypeCode = N'RUBBER_SUPPLIER'
        EXEC dbo.spMemberLoyaltyAccountCreateIfNotExists @CustomerId;

    INSERT dbo.CustomerAuditLog(CustomerId, ActionType, EntityName, EntityId, NewValue, Remark, CreatedByUserId)
    VALUES(@CustomerId, N'MemberTypeAssigned', N'CustomerMemberType', @MemberTypeId, @MemberTypeCode, N'Membership type assigned or reactivated.', @CreatedByUserId);
    COMMIT;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerMemberTypeDeactivate
    @CustomerId INT,
    @MemberTypeCode NVARCHAR(50),
    @UpdatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @MemberTypeId INT = (SELECT MemberTypeId FROM dbo.MemberType WHERE MemberTypeCode = @MemberTypeCode);
    IF @MemberTypeId IS NULL THROW 52000, 'Member type does not exist.', 1;

    UPDATE dbo.CustomerMemberType
    SET IsActive = 0, EndDate = CONVERT(date, SYSDATETIME()), UpdatedDate = SYSDATETIME(), UpdatedByUserId = @UpdatedByUserId
    WHERE CustomerId = @CustomerId AND MemberTypeId = @MemberTypeId AND IsActive = 1;

    INSERT dbo.CustomerAuditLog(CustomerId, ActionType, EntityName, EntityId, OldValue, Remark, CreatedByUserId)
    VALUES(@CustomerId, N'MemberTypeDeactivated', N'CustomerMemberType', @MemberTypeId, @MemberTypeCode, N'Membership type deactivated.', @UpdatedByUserId);
END
GO

CREATE OR ALTER PROCEDURE dbo.spMemberLoyaltyAccountCreateIfNotExists @CustomerId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.MemberLoyaltyAccount WITH (UPDLOCK, HOLDLOCK) WHERE CustomerId = @CustomerId)
        INSERT dbo.MemberLoyaltyAccount(CustomerId) VALUES(@CustomerId);
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerMemberTypeSync
    @CustomerId INT,
    @MemberTypeCodesCsv NVARCHAR(500),
    @WholesaleBusinessName NVARCHAR(255)=NULL,
    @WholesaleLevelId INT=NULL,
    @WholesaleIsApproved BIT=0,
    @WholesalePaymentTermDays INT=0,
    @WholesaleApprovedByUserId INT=NULL,
    @SupplierCode NVARCHAR(50)=NULL,
    @DefaultBusinessLocationId INT=NULL,
    @SupplierRemark NVARCHAR(1000)=NULL,
    @UpdatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF NULLIF(LTRIM(RTRIM(@MemberTypeCodesCsv)), N'') IS NULL SET @MemberTypeCodesCsv = N'RETAIL';

    BEGIN TRAN;

    DECLARE @Desired TABLE(MemberTypeCode NVARCHAR(50) NOT NULL PRIMARY KEY);
    INSERT @Desired(MemberTypeCode)
    SELECT DISTINCT UPPER(LTRIM(RTRIM(value)))
    FROM STRING_SPLIT(@MemberTypeCodesCsv, N',')
    WHERE NULLIF(LTRIM(RTRIM(value)), N'') IS NOT NULL;

    IF EXISTS (SELECT 1 FROM @Desired d WHERE NOT EXISTS (SELECT 1 FROM dbo.MemberType mt WHERE mt.MemberTypeCode = d.MemberTypeCode AND mt.IsActive = 1))
        THROW 52001, 'One or more selected member types are invalid.', 1;

    DECLARE @Code NVARCHAR(50);
    DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT MemberTypeCode FROM @Desired;
    OPEN cur;
    FETCH NEXT FROM cur INTO @Code;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC dbo.spCustomerMemberTypeAssign @CustomerId, @Code, @UpdatedByUserId;
        FETCH NEXT FROM cur INTO @Code;
    END
    CLOSE cur;
    DEALLOCATE cur;

    UPDATE cmt
    SET IsActive = 0, EndDate = CONVERT(date, SYSDATETIME()), UpdatedDate = SYSDATETIME(), UpdatedByUserId = @UpdatedByUserId
    FROM dbo.CustomerMemberType cmt
    JOIN dbo.MemberType mt ON mt.MemberTypeId = cmt.MemberTypeId
    WHERE cmt.CustomerId = @CustomerId AND cmt.IsActive = 1 AND NOT EXISTS (SELECT 1 FROM @Desired d WHERE d.MemberTypeCode = mt.MemberTypeCode);

    UPDATE dbo.Customer
    SET MemberType = CASE WHEN EXISTS(SELECT 1 FROM @Desired WHERE MemberTypeCode = N'WHOLESALE') THEN N'Wholesale' ELSE N'Retail' END,
        UpdatedDate = SYSDATETIME(),
        UpdatedByUserId = @UpdatedByUserId
    WHERE CustomerId = @CustomerId;

    IF EXISTS (SELECT 1 FROM @Desired WHERE MemberTypeCode = N'WHOLESALE')
    BEGIN
        MERGE dbo.WholesaleMemberProfile AS target
        USING (SELECT @CustomerId CustomerId) AS source
        ON target.CustomerId = source.CustomerId
        WHEN MATCHED THEN UPDATE SET BusinessName = @WholesaleBusinessName, WholesaleLevelId = @WholesaleLevelId, IsApproved = @WholesaleIsApproved, PaymentTermDays = @WholesalePaymentTermDays, ApprovedByUserId = CASE WHEN @WholesaleIsApproved = 1 THEN @WholesaleApprovedByUserId ELSE NULL END, ApprovedDate = CASE WHEN @WholesaleIsApproved = 1 THEN COALESCE(target.ApprovedDate, SYSDATETIME()) ELSE NULL END, UpdatedDate = SYSDATETIME()
        WHEN NOT MATCHED THEN INSERT(CustomerId, BusinessName, WholesaleLevelId, IsApproved, PaymentTermDays, ApprovedByUserId, ApprovedDate) VALUES(@CustomerId, @WholesaleBusinessName, @WholesaleLevelId, @WholesaleIsApproved, @WholesalePaymentTermDays, CASE WHEN @WholesaleIsApproved = 1 THEN @WholesaleApprovedByUserId ELSE NULL END, CASE WHEN @WholesaleIsApproved = 1 THEN SYSDATETIME() ELSE NULL END);
    END

    IF EXISTS (SELECT 1 FROM @Desired WHERE MemberTypeCode = N'RUBBER_SUPPLIER')
    BEGIN
        IF NULLIF(LTRIM(RTRIM(@SupplierCode)), N'') IS NULL
            SET @SupplierCode = CONCAT(N'RS', FORMAT(SYSDATETIME(), N'yyyyMMddHHmmss'), RIGHT(CONCAT(N'0000', @CustomerId), 4));

        MERGE dbo.RubberSupplierMemberProfile AS target
        USING (SELECT @CustomerId CustomerId) AS source
        ON target.CustomerId = source.CustomerId
        WHEN MATCHED THEN UPDATE SET SupplierCode = @SupplierCode, DefaultBusinessLocationId = @DefaultBusinessLocationId, Remark = @SupplierRemark, UpdatedDate = SYSDATETIME()
        WHEN NOT MATCHED THEN INSERT(CustomerId, SupplierCode, DefaultBusinessLocationId, Remark) VALUES(@CustomerId, @SupplierCode, @DefaultBusinessLocationId, @SupplierRemark);

        EXEC dbo.spMemberLoyaltyAccountCreateIfNotExists @CustomerId;
    END

    INSERT dbo.CustomerAuditLog(CustomerId, ActionType, EntityName, EntityId, NewValue, Remark, CreatedByUserId)
    VALUES(@CustomerId, N'MemberTypesSynced', N'Customer', @CustomerId, @MemberTypeCodesCsv, N'Active membership types synchronized.', @UpdatedByUserId);

    COMMIT;
END
GO

CREATE OR ALTER PROCEDURE dbo.spLoyaltyRuleGetActive
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (1) *
    FROM dbo.LoyaltyRule
    WHERE RuleCode = N'RUBBER_WEIGHT_REWARD' AND IsActive = 1 AND EffectiveFrom <= SYSDATETIME() AND (EffectiveTo IS NULL OR EffectiveTo > SYSDATETIME())
    ORDER BY EffectiveFrom DESC, LoyaltyRuleId DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.spMemberLoyaltyAccountGetByCustomerId @CustomerId INT
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.spMemberLoyaltyAccountCreateIfNotExists @CustomerId;
    SELECT MemberLoyaltyAccountId, CustomerId, PointBalance, RubberWeightCarryForwardKg
    FROM dbo.MemberLoyaltyAccount
    WHERE CustomerId = @CustomerId;
END
GO

IF OBJECT_ID(N'dbo.fnCustomerHasActiveMembership', N'FN') IS NULL
    EXEC(N'CREATE FUNCTION dbo.fnCustomerHasActiveMembership(@CustomerId INT, @MemberTypeCode NVARCHAR(50)) RETURNS BIT AS BEGIN RETURN 0 END');
GO

CREATE OR ALTER PROCEDURE dbo.spMemberLoyaltyCalculateFromRubberWeight @CustomerId INT, @RubberWeightKg DECIMAL(18,4)
AS
BEGIN
    SET NOCOUNT ON;
    IF @RubberWeightKg <= 0 THROW 53000, 'Rubber weight must be greater than zero.', 1;
    IF dbo.fnCustomerHasActiveMembership(@CustomerId, N'RUBBER_SUPPLIER') = 0 THROW 53001, 'Customer is not an active rubber supplier member.', 1;

    DECLARE @Carry DECIMAL(18,4), @WeightKgPerPoint DECIMAL(18,4), @Total DECIMAL(18,4), @Points INT;
    SELECT @Carry = RubberWeightCarryForwardKg FROM dbo.MemberLoyaltyAccount WHERE CustomerId = @CustomerId;
    IF @Carry IS NULL SET @Carry = 0;
    SELECT TOP (1) @WeightKgPerPoint = WeightKgPerPoint FROM dbo.LoyaltyRule WHERE RuleCode = N'RUBBER_WEIGHT_REWARD' AND IsActive = 1 ORDER BY EffectiveFrom DESC;
    IF @WeightKgPerPoint IS NULL THROW 53002, 'Active rubber loyalty rule was not found.', 1;

    SET @Total = @Carry + @RubberWeightKg;
    SET @Points = FLOOR(@Total / @WeightKgPerPoint);
    SELECT @Carry PreviousCarryForwardWeightKg, @RubberWeightKg RubberWeightKg, @WeightKgPerPoint WeightKgPerPoint, @Points PointsEarned, @Total - (@Points * @WeightKgPerPoint) CarryForwardWeightAfterKg;
END
GO

ALTER FUNCTION dbo.fnCustomerHasActiveMembership(@CustomerId INT, @MemberTypeCode NVARCHAR(50))
RETURNS BIT
AS
BEGIN
    RETURN
    (
        SELECT CONVERT(BIT, CASE WHEN EXISTS
        (
            SELECT 1
            FROM dbo.CustomerMemberType cmt
            JOIN dbo.MemberType mt ON mt.MemberTypeId = cmt.MemberTypeId
            WHERE cmt.CustomerId = @CustomerId AND mt.MemberTypeCode = @MemberTypeCode AND cmt.IsActive = 1
        ) THEN 1 ELSE 0 END)
    );
END
GO

CREATE OR ALTER PROCEDURE dbo.spMemberLoyaltyAddFromRubberPurchase
    @CustomerId INT,
    @RubberPurchaseHeaderId BIGINT,
    @ConfirmedWeightKg DECIMAL(18,4),
    @CreatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @ConfirmedWeightKg <= 0 THROW 53000, 'Rubber weight must be greater than zero.', 1;
    IF dbo.fnCustomerHasActiveMembership(@CustomerId, N'RUBBER_SUPPLIER') = 0 THROW 53001, 'Customer is not an active rubber supplier member.', 1;

    BEGIN TRAN;
    IF EXISTS (SELECT 1 FROM dbo.MemberLoyaltyTransaction WHERE SourceType = N'RUBBER_PURCHASE' AND ReferenceId = @RubberPurchaseHeaderId AND TransactionType = N'EARN_FROM_RUBBER_PURCHASE')
        THROW 53003, 'Rubber purchase reward has already been processed.', 1;

    EXEC dbo.spMemberLoyaltyAccountCreateIfNotExists @CustomerId;

    DECLARE @AccountId BIGINT, @PreviousCarry DECIMAL(18,4), @CurrentBalance DECIMAL(18,2), @WeightKgPerPoint DECIMAL(18,4), @Total DECIMAL(18,4), @Points INT, @NewCarry DECIMAL(18,4), @NewBalance DECIMAL(18,2);
    SELECT @AccountId = MemberLoyaltyAccountId, @PreviousCarry = RubberWeightCarryForwardKg, @CurrentBalance = PointBalance
    FROM dbo.MemberLoyaltyAccount WITH (UPDLOCK, HOLDLOCK)
    WHERE CustomerId = @CustomerId;
    SELECT TOP (1) @WeightKgPerPoint = WeightKgPerPoint FROM dbo.LoyaltyRule WHERE RuleCode = N'RUBBER_WEIGHT_REWARD' AND IsActive = 1 ORDER BY EffectiveFrom DESC;
    IF @WeightKgPerPoint IS NULL THROW 53002, 'Active rubber loyalty rule was not found.', 1;

    SET @Total = @PreviousCarry + @ConfirmedWeightKg;
    SET @Points = FLOOR(@Total / @WeightKgPerPoint);
    SET @NewCarry = @Total - (@Points * @WeightKgPerPoint);
    SET @NewBalance = @CurrentBalance + @Points;

    UPDATE dbo.MemberLoyaltyAccount
    SET PointBalance = @NewBalance, RubberWeightCarryForwardKg = @NewCarry, UpdatedDate = SYSDATETIME()
    WHERE MemberLoyaltyAccountId = @AccountId;

    INSERT dbo.MemberLoyaltyTransaction(MemberLoyaltyAccountId, TransactionType, SourceType, ReferenceId, RubberWeightKg, WeightKgPerPointSnapshot, PreviousCarryForwardWeightKg, CarryForwardWeightAfterKg, Points, PointBalanceAfterTransaction, Remark, CreatedByUserId)
    VALUES(@AccountId, N'EARN_FROM_RUBBER_PURCHASE', N'RUBBER_PURCHASE', @RubberPurchaseHeaderId, @ConfirmedWeightKg, @WeightKgPerPoint, @PreviousCarry, @NewCarry, @Points, @NewBalance, N'Points earned from confirmed rubber purchase.', @CreatedByUserId);

    SELECT TOP (1) *
    FROM dbo.MemberLoyaltyTransaction
    WHERE MemberLoyaltyTransactionId = SCOPE_IDENTITY();
    COMMIT;
END
GO

CREATE OR ALTER PROCEDURE dbo.spMemberLoyaltyReverseRubberPurchase @RubberPurchaseHeaderId BIGINT, @CreatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRAN;
    IF EXISTS (SELECT 1 FROM dbo.MemberLoyaltyTransaction WHERE SourceType = N'RUBBER_PURCHASE' AND ReferenceId = @RubberPurchaseHeaderId AND TransactionType = N'CANCELLATION_REVERSAL')
        THROW 53004, 'Rubber purchase loyalty reversal has already been processed.', 1;

    DECLARE @EarnId BIGINT, @AccountId BIGINT, @Points INT, @PreviousCarry DECIMAL(18,4), @CurrentBalance DECIMAL(18,2);
    SELECT TOP (1) @EarnId = MemberLoyaltyTransactionId, @AccountId = MemberLoyaltyAccountId, @Points = Points, @PreviousCarry = PreviousCarryForwardWeightKg
    FROM dbo.MemberLoyaltyTransaction
    WHERE SourceType = N'RUBBER_PURCHASE' AND ReferenceId = @RubberPurchaseHeaderId AND TransactionType = N'EARN_FROM_RUBBER_PURCHASE';
    IF @EarnId IS NULL THROW 53005, 'Original rubber purchase loyalty transaction was not found.', 1;

    SELECT @CurrentBalance = PointBalance FROM dbo.MemberLoyaltyAccount WITH (UPDLOCK, HOLDLOCK) WHERE MemberLoyaltyAccountId = @AccountId;
    IF @CurrentBalance - @Points < 0 THROW 53006, 'Reversal would make point balance negative.', 1;

    UPDATE dbo.MemberLoyaltyAccount
    SET PointBalance = @CurrentBalance - @Points, RubberWeightCarryForwardKg = @PreviousCarry, UpdatedDate = SYSDATETIME()
    WHERE MemberLoyaltyAccountId = @AccountId;

    INSERT dbo.MemberLoyaltyTransaction(MemberLoyaltyAccountId, TransactionType, SourceType, ReferenceId, Points, PointBalanceAfterTransaction, CarryForwardWeightAfterKg, Remark, CreatedByUserId)
    VALUES(@AccountId, N'CANCELLATION_REVERSAL', N'RUBBER_PURCHASE', @RubberPurchaseHeaderId, -@Points, @CurrentBalance - @Points, @PreviousCarry, N'Reversed points from cancelled rubber purchase.', @CreatedByUserId);
    COMMIT;
END
GO

CREATE OR ALTER PROCEDURE dbo.spMemberLoyaltyRedeem @CustomerId INT, @Points INT, @Remark NVARCHAR(1000)=NULL, @CreatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @Points <= 0 THROW 53007, 'Redeem points must be greater than zero.', 1;

    BEGIN TRAN;
    EXEC dbo.spMemberLoyaltyAccountCreateIfNotExists @CustomerId;
    DECLARE @AccountId BIGINT, @Balance DECIMAL(18,2), @Carry DECIMAL(18,4);
    SELECT @AccountId = MemberLoyaltyAccountId, @Balance = PointBalance, @Carry = RubberWeightCarryForwardKg FROM dbo.MemberLoyaltyAccount WITH (UPDLOCK, HOLDLOCK) WHERE CustomerId = @CustomerId;
    IF @Balance < @Points THROW 53008, 'Insufficient point balance.', 1;
    UPDATE dbo.MemberLoyaltyAccount SET PointBalance = @Balance - @Points, UpdatedDate = SYSDATETIME() WHERE MemberLoyaltyAccountId = @AccountId;
    INSERT dbo.MemberLoyaltyTransaction(MemberLoyaltyAccountId, TransactionType, SourceType, Points, PointBalanceAfterTransaction, CarryForwardWeightAfterKg, Remark, CreatedByUserId)
    VALUES(@AccountId, N'REDEEM', N'POINT_REDEMPTION', -@Points, @Balance - @Points, @Carry, @Remark, @CreatedByUserId);
    COMMIT;
END
GO

CREATE OR ALTER PROCEDURE dbo.spMemberLoyaltyTransactionGetByCustomerId @CustomerId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.*
    FROM dbo.MemberLoyaltyTransaction t
    JOIN dbo.MemberLoyaltyAccount a ON a.MemberLoyaltyAccountId = t.MemberLoyaltyAccountId
    WHERE a.CustomerId = @CustomerId
    ORDER BY t.CreatedDate DESC, t.MemberLoyaltyTransactionId DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.spRubberPriceGetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RubberPriceId, PricePerKg, PercentageOfService, IsActive, CreatedDate, UpdatedDate
    FROM dbo.RubberPrice
    ORDER BY CreatedDate DESC, RubberPriceId DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.spRubberPriceGetActive
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RubberPriceId, PricePerKg, PercentageOfService, IsActive, CreatedDate, UpdatedDate
    FROM dbo.RubberPrice
    WHERE IsActive = 1
    ORDER BY CreatedDate DESC, RubberPriceId DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.spRubberPriceGetById @RubberPriceId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RubberPriceId, PricePerKg, PercentageOfService, IsActive, CreatedDate, UpdatedDate
    FROM dbo.RubberPrice
    WHERE RubberPriceId = @RubberPriceId;
END
GO

CREATE OR ALTER PROCEDURE dbo.spRubberPriceCreate
    @PricePerKg DECIMAL(18,2),
    @PercentageOfService DECIMAL(5,2),
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    IF @PricePerKg < 0 THROW 54030, 'Rubber price must not be negative.', 1;
    IF @PercentageOfService < 0 OR @PercentageOfService > 100 THROW 54031, 'Service percentage must be between 0 and 100.', 1;

    INSERT dbo.RubberPrice(PricePerKg, PercentageOfService, IsActive)
    VALUES(@PricePerKg, @PercentageOfService, @IsActive);

    SELECT CONVERT(INT, SCOPE_IDENTITY()) AS RubberPriceId;
END
GO

CREATE OR ALTER PROCEDURE dbo.spRubberPriceUpdate
    @RubberPriceId INT,
    @PricePerKg DECIMAL(18,2),
    @PercentageOfService DECIMAL(5,2),
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    IF @RubberPriceId <= 0 THROW 54032, 'Rubber price id is required.', 1;
    IF @PricePerKg < 0 THROW 54030, 'Rubber price must not be negative.', 1;
    IF @PercentageOfService < 0 OR @PercentageOfService > 100 THROW 54031, 'Service percentage must be between 0 and 100.', 1;

    UPDATE dbo.RubberPrice
    SET PricePerKg = @PricePerKg,
        PercentageOfService = @PercentageOfService,
        IsActive = @IsActive,
        UpdatedDate = SYSDATETIME()
    WHERE RubberPriceId = @RubberPriceId;

    IF @@ROWCOUNT = 0 THROW 54033, 'Rubber price was not found.', 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.spRubberPriceToggleActive
    @RubberPriceId INT,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    IF @RubberPriceId <= 0 THROW 54032, 'Rubber price id is required.', 1;

    UPDATE dbo.RubberPrice
    SET IsActive = @IsActive,
        UpdatedDate = SYSDATETIME()
    WHERE RubberPriceId = @RubberPriceId;

    IF @@ROWCOUNT = 0 THROW 54033, 'Rubber price was not found.', 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.spRubberAuctionLocationGetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RubberAuctionLocationId, LocationName, Address, IsActive, CreatedDate, UpdatedDate
    FROM dbo.RubberAuctionLocation
    ORDER BY LocationName;
END
GO

CREATE OR ALTER PROCEDURE dbo.spRubberAuctionLocationGetActive
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RubberAuctionLocationId, LocationName, Address, IsActive, CreatedDate, UpdatedDate
    FROM dbo.RubberAuctionLocation
    WHERE IsActive = 1
    ORDER BY LocationName;
END
GO

CREATE OR ALTER PROCEDURE dbo.spRubberPurchaseHeaderCreate
    @CustomerId INT=NULL,
    @NonMemberFarmerName NVARCHAR(255)=NULL,
    @NonMemberFarmerPhone NVARCHAR(50)=NULL,
    @BusinessLocationId INT,
    @RubberAuctionLocationId INT=NULL,
    @TransactionDate DATETIME2(0),
    @WeightKg DECIMAL(18,4),
    @RubberPriceId INT=NULL,
    @MarketingPriceId BIGINT=NULL,
    @PricePerKgSnapshot DECIMAL(18,4)=NULL,
    @PercentageSnapshot DECIMAL(18,4)=NULL,
    @TotalAmount DECIMAL(18,2)=NULL,
    @PaymentStatus NVARCHAR(30)=N'Pending',
    @CreatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @WeightKg <= 0 THROW 54000, 'Rubber weight must be greater than zero.', 1;
    IF @TotalAmount IS NOT NULL AND @TotalAmount < 0 THROW 54001, 'Total amount cannot be negative.', 1;
    IF @PaymentStatus NOT IN (N'Pending', N'Paid', N'Cancelled') THROW 54002, 'Invalid rubber purchase payment status.', 1;
    SET @NonMemberFarmerName = NULLIF(LTRIM(RTRIM(ISNULL(@NonMemberFarmerName, N''))), N'');
    SET @NonMemberFarmerPhone = NULLIF(LTRIM(RTRIM(ISNULL(@NonMemberFarmerPhone, N''))), N'');
    IF @CustomerId IS NULL AND @NonMemberFarmerName IS NULL THROW 54003, 'Member supplier or non-member farmer name is required.', 1;
    IF @CustomerId IS NOT NULL AND @NonMemberFarmerName IS NOT NULL THROW 54006, 'Use either member supplier or non-member farmer details, not both.', 1;
    IF @CustomerId IS NOT NULL AND dbo.fnCustomerHasActiveMembership(@CustomerId, N'RUBBER_SUPPLIER') = 0 THROW 54007, 'Customer is not an active rubber supplier member.', 1;
    IF @RubberAuctionLocationId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.RubberAuctionLocation WHERE RubberAuctionLocationId = @RubberAuctionLocationId AND IsActive = 1) THROW 54004, 'Rubber auction location does not exist or is inactive.', 1;
    IF @RubberPriceId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.RubberPrice WHERE RubberPriceId = @RubberPriceId AND IsActive = 1) THROW 54005, 'Rubber price does not exist or is inactive.', 1;

    BEGIN TRAN;

    INSERT dbo.RubberPurchaseHeader(CustomerId, NonMemberFarmerName, NonMemberFarmerPhone, BusinessLocationId, RubberAuctionLocationId, TransactionDate, WeightKg, RubberPriceId, MarketingPriceId, PricePerKgSnapshot, PercentageSnapshot, TotalAmount, PaymentStatus, CreatedByUserId)
    VALUES(@CustomerId, @NonMemberFarmerName, @NonMemberFarmerPhone, @BusinessLocationId, @RubberAuctionLocationId, @TransactionDate, @WeightKg, @RubberPriceId, @MarketingPriceId, @PricePerKgSnapshot, @PercentageSnapshot, @TotalAmount, @PaymentStatus, @CreatedByUserId);

    DECLARE @RubberPurchaseHeaderId BIGINT = CONVERT(BIGINT, SCOPE_IDENTITY());

    IF @CustomerId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.MemberLoyaltyTransaction WHERE SourceType = N'RUBBER_PURCHASE' AND ReferenceId = @RubberPurchaseHeaderId AND TransactionType = N'EARN_FROM_RUBBER_PURCHASE')
    BEGIN
        EXEC dbo.spMemberLoyaltyAccountCreateIfNotExists @CustomerId;

        DECLARE @AccountId BIGINT, @PreviousCarry DECIMAL(18,4), @CurrentBalance DECIMAL(18,2), @WeightKgPerPoint DECIMAL(18,4), @TotalWeight DECIMAL(18,4), @Points INT, @NewCarry DECIMAL(18,4), @NewBalance DECIMAL(18,2);
        SELECT @AccountId = MemberLoyaltyAccountId, @PreviousCarry = RubberWeightCarryForwardKg, @CurrentBalance = PointBalance
        FROM dbo.MemberLoyaltyAccount WITH (UPDLOCK, HOLDLOCK)
        WHERE CustomerId = @CustomerId;

        SELECT TOP (1) @WeightKgPerPoint = WeightKgPerPoint
        FROM dbo.LoyaltyRule
        WHERE RuleCode = N'RUBBER_WEIGHT_REWARD' AND IsActive = 1 AND EffectiveFrom <= SYSDATETIME() AND (EffectiveTo IS NULL OR EffectiveTo > SYSDATETIME())
        ORDER BY EffectiveFrom DESC, LoyaltyRuleId DESC;

        IF @WeightKgPerPoint IS NULL THROW 53002, 'Active rubber loyalty rule was not found.', 1;

        SET @TotalWeight = @PreviousCarry + @WeightKg;
        SET @Points = FLOOR(@TotalWeight / @WeightKgPerPoint);
        SET @NewCarry = @TotalWeight - (@Points * @WeightKgPerPoint);
        SET @NewBalance = @CurrentBalance + @Points;

        UPDATE dbo.MemberLoyaltyAccount
        SET PointBalance = @NewBalance, RubberWeightCarryForwardKg = @NewCarry, UpdatedDate = SYSDATETIME()
        WHERE MemberLoyaltyAccountId = @AccountId;

        INSERT dbo.MemberLoyaltyTransaction(MemberLoyaltyAccountId, TransactionType, SourceType, ReferenceId, RubberWeightKg, WeightKgPerPointSnapshot, PreviousCarryForwardWeightKg, CarryForwardWeightAfterKg, Points, PointBalanceAfterTransaction, Remark, CreatedByUserId)
        VALUES(@AccountId, N'EARN_FROM_RUBBER_PURCHASE', N'RUBBER_PURCHASE', @RubberPurchaseHeaderId, @WeightKg, @WeightKgPerPoint, @PreviousCarry, @NewCarry, @Points, @NewBalance, N'Points earned from confirmed rubber purchase.', @CreatedByUserId);
    END

    COMMIT;

    SELECT @RubberPurchaseHeaderId;
END
GO

CREATE OR ALTER PROCEDURE dbo.spRubberPurchaseHeaderGetByCustomerId @CustomerId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT h.RubberPurchaseHeaderId, h.CustomerId,
           ISNULL(c.CustomerCode, N'NON-MEMBER') CustomerCode,
           COALESCE(c.CustomerName, h.NonMemberFarmerName, N'Non-member farmer') CustomerName,
           COALESCE(c.PhoneNumber, h.NonMemberFarmerPhone, N'') PhoneNumber,
           h.NonMemberFarmerName, h.NonMemberFarmerPhone,
           h.BusinessLocationId, l.LocationName,
           h.RubberAuctionLocationId, al.LocationName RubberAuctionLocationName,
           h.TransactionDate, h.WeightKg, h.RubberPriceId, h.MarketingPriceId, h.PricePerKgSnapshot, h.PercentageSnapshot, h.TotalAmount, h.PaymentStatus,
           h.ReceiptNo, h.PaidAmount, ISNULL(h.CreditDeductedAmount,0) CreditDeductedAmount, h.PaidDate, h.PaymentMethod, h.PaymentRemark,
           ISNULL(t.Points,0) PointsEarned, ISNULL(t.CarryForwardWeightAfterKg,0) CarryForwardWeightAfterKg,
           h.CreatedByUserId, h.CreatedDate
    FROM dbo.RubberPurchaseHeader h
    LEFT JOIN dbo.Customer c ON c.CustomerId = h.CustomerId
    JOIN dbo.InventoryLocation l ON l.LocationId = h.BusinessLocationId
    LEFT JOIN dbo.RubberAuctionLocation al ON al.RubberAuctionLocationId = h.RubberAuctionLocationId
    LEFT JOIN dbo.MemberLoyaltyAccount a ON a.CustomerId = h.CustomerId
    LEFT JOIN dbo.MemberLoyaltyTransaction t ON t.MemberLoyaltyAccountId = a.MemberLoyaltyAccountId AND t.SourceType = N'RUBBER_PURCHASE' AND t.ReferenceId = h.RubberPurchaseHeaderId AND t.TransactionType = N'EARN_FROM_RUBBER_PURCHASE'
    WHERE h.CustomerId = @CustomerId
    ORDER BY TransactionDate DESC, RubberPurchaseHeaderId DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.spRubberPurchaseHeaderGetById @RubberPurchaseHeaderId BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT h.RubberPurchaseHeaderId, h.CustomerId,
           ISNULL(c.CustomerCode, N'NON-MEMBER') CustomerCode,
           COALESCE(c.CustomerName, h.NonMemberFarmerName, N'Non-member farmer') CustomerName,
           COALESCE(c.PhoneNumber, h.NonMemberFarmerPhone, N'') PhoneNumber,
           h.NonMemberFarmerName, h.NonMemberFarmerPhone,
           h.BusinessLocationId, l.LocationName,
           h.RubberAuctionLocationId, al.LocationName RubberAuctionLocationName,
           h.TransactionDate, h.WeightKg, h.RubberPriceId, h.MarketingPriceId, h.PricePerKgSnapshot, h.PercentageSnapshot, h.TotalAmount, h.PaymentStatus,
           h.ReceiptNo, h.PaidAmount, ISNULL(h.CreditDeductedAmount,0) CreditDeductedAmount, h.PaidDate, h.PaymentMethod, h.PaymentRemark,
           ISNULL(t.Points,0) PointsEarned, ISNULL(t.CarryForwardWeightAfterKg,0) CarryForwardWeightAfterKg,
           h.CreatedByUserId, h.CreatedDate
    FROM dbo.RubberPurchaseHeader h
    LEFT JOIN dbo.Customer c ON c.CustomerId = h.CustomerId
    JOIN dbo.InventoryLocation l ON l.LocationId = h.BusinessLocationId
    LEFT JOIN dbo.RubberAuctionLocation al ON al.RubberAuctionLocationId = h.RubberAuctionLocationId
    LEFT JOIN dbo.MemberLoyaltyAccount a ON a.CustomerId = h.CustomerId
    LEFT JOIN dbo.MemberLoyaltyTransaction t ON t.MemberLoyaltyAccountId = a.MemberLoyaltyAccountId AND t.SourceType = N'RUBBER_PURCHASE' AND t.ReferenceId = h.RubberPurchaseHeaderId AND t.TransactionType = N'EARN_FROM_RUBBER_PURCHASE'
    WHERE h.RubberPurchaseHeaderId = @RubberPurchaseHeaderId;
END
GO

CREATE OR ALTER PROCEDURE dbo.spRubberPurchaseHeaderPayBill
    @RubberPurchaseHeaderId BIGINT,
    @PaidAmount DECIMAL(18,2),
    @CreditDeductedAmount DECIMAL(18,2)=0,
    @PaymentMethod NVARCHAR(30),
    @PaymentRemark NVARCHAR(500)=NULL,
    @UpdatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @CreditDeductedAmount = ISNULL(@CreditDeductedAmount, 0);
    IF @PaidAmount < 0 THROW 54010, 'Paid amount cannot be negative.', 1;
    IF @CreditDeductedAmount < 0 THROW 54016, 'Credit deducted amount cannot be negative.', 1;
    IF @PaymentMethod NOT IN (N'Cash', N'Transfer') THROW 54011, 'Invalid rubber purchase payment method.', 1;

    DECLARE @TotalAmount DECIMAL(18,2);
    DECLARE @PaymentStatus NVARCHAR(30);
    DECLARE @CustomerId INT;
    DECLARE @ReceiptNo NVARCHAR(30);
    SELECT @TotalAmount = ISNULL(TotalAmount, 0),
           @PaymentStatus = PaymentStatus,
           @CustomerId = CustomerId,
           @ReceiptNo = ReceiptNo
    FROM dbo.RubberPurchaseHeader WITH (UPDLOCK, HOLDLOCK)
    WHERE RubberPurchaseHeaderId = @RubberPurchaseHeaderId;

    IF @TotalAmount IS NULL THROW 54012, 'Rubber purchase was not found.', 1;
    IF @TotalAmount <= 0 THROW 54013, 'Rubber purchase total amount must be greater than zero before payment.', 1;
    IF @PaymentStatus IN (N'Paid', N'Cancelled') THROW 54014, 'Rubber purchase is not available for payment.', 1;
    IF @PaidAmount + @CreditDeductedAmount <> @TotalAmount THROW 54015, 'Rubber bill must be settled in full by payment and credit deduction.', 1;
    IF @CreditDeductedAmount > 0 AND @CustomerId IS NULL THROW 54017, 'Credit deduction requires a member customer.', 1;

    SET @ReceiptNo = COALESCE(@ReceiptNo, CONCAT(N'RB', FORMAT(SYSDATETIME(), N'yyyyMMdd'), RIGHT(CONCAT(N'000000', @RubberPurchaseHeaderId), 6)));

    BEGIN TRAN;

    IF @CreditDeductedAmount > 0
    BEGIN
        DECLARE @Outstanding DECIMAL(18,2);
        DECLARE @BalanceAfter DECIMAL(18,2);

        SELECT @Outstanding = CurrentOutstandingAmount
        FROM dbo.CustomerCredit WITH(UPDLOCK, HOLDLOCK)
        WHERE CustomerId = @CustomerId;

        IF @Outstanding IS NULL THROW 54018, 'Customer credit account was not found.', 1;
        IF @CreditDeductedAmount > @Outstanding THROW 54019, 'Credit deduction cannot exceed outstanding customer credit.', 1;

        SET @BalanceAfter = @Outstanding - @CreditDeductedAmount;

        UPDATE dbo.CustomerCredit
        SET CurrentOutstandingAmount = @BalanceAfter,
            UpdatedDate = SYSDATETIME(),
            UpdatedByUserId = @UpdatedByUserId
        WHERE CustomerId = @CustomerId;

        INSERT dbo.CustomerCreditTransaction(CustomerId,TransactionType,Amount,BalanceBefore,BalanceAfter,PaidDate,ReferenceType,ReferenceId,ReferenceNo,Status,Remark,Description,CreatedByUserId)
        VALUES(@CustomerId,N'Payment',@CreditDeductedAmount,@Outstanding,@BalanceAfter,SYSDATETIME(),N'CreditRepayment',@RubberPurchaseHeaderId,@ReceiptNo,N'Paid',N'Deducted from rubber purchase payment.',N'Customer credit deducted from rubber purchase receipt.',@UpdatedByUserId);

        INSERT dbo.CustomerAuditLog(CustomerId,ActionType,EntityName,EntityId,OldValue,NewValue,Remark,CreatedByUserId)
        VALUES(@CustomerId,N'CreditPaymentReceived',N'CustomerCredit',@CustomerId,CONCAT(N'Outstanding=',@Outstanding),CONCAT(N'Outstanding=',@BalanceAfter,N'; Payment=',@CreditDeductedAmount),N'Deducted from rubber purchase payment.',@UpdatedByUserId);
    END

    UPDATE dbo.RubberPurchaseHeader
    SET PaidAmount = @PaidAmount,
        CreditDeductedAmount = @CreditDeductedAmount,
        PaidDate = SYSDATETIME(),
        PaymentMethod = @PaymentMethod,
        PaymentRemark = @PaymentRemark,
        PaymentStatus = N'Paid',
        ReceiptNo = @ReceiptNo
    WHERE RubberPurchaseHeaderId = @RubberPurchaseHeaderId;

    COMMIT;

    EXEC dbo.spRubberPurchaseHeaderGetById @RubberPurchaseHeaderId;
END
GO

CREATE OR ALTER PROCEDURE dbo.spRubberPurchaseHeaderGetPaged
    @PageNumber INT=1,
    @PageSize INT=20,
    @CustomerId INT=NULL,
    @BusinessLocationId INT=NULL,
    @RubberAuctionLocationId INT=NULL,
    @PaymentStatus NVARCHAR(30)=NULL,
    @DateFrom DATETIME2(0)=NULL,
    @DateTo DATETIME2(0)=NULL,
    @SearchText NVARCHAR(255)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH q AS
    (
        SELECT h.RubberPurchaseHeaderId, h.CustomerId,
               ISNULL(c.CustomerCode, N'NON-MEMBER') CustomerCode,
               COALESCE(c.CustomerName, h.NonMemberFarmerName, N'Non-member farmer') CustomerName,
               COALESCE(c.PhoneNumber, h.NonMemberFarmerPhone, N'') PhoneNumber,
               h.NonMemberFarmerName, h.NonMemberFarmerPhone,
               h.BusinessLocationId, l.LocationName,
               h.RubberAuctionLocationId, al.LocationName RubberAuctionLocationName,
               h.TransactionDate, h.WeightKg, h.RubberPriceId, h.MarketingPriceId, h.PricePerKgSnapshot, h.PercentageSnapshot, h.TotalAmount, h.PaymentStatus,
               h.ReceiptNo, h.PaidAmount, ISNULL(h.CreditDeductedAmount,0) CreditDeductedAmount, h.PaidDate, h.PaymentMethod, h.PaymentRemark,
               ISNULL(t.Points,0) PointsEarned, ISNULL(t.CarryForwardWeightAfterKg,0) CarryForwardWeightAfterKg,
               h.CreatedByUserId, h.CreatedDate, COUNT(1) OVER() TotalCount
        FROM dbo.RubberPurchaseHeader h
        LEFT JOIN dbo.Customer c ON c.CustomerId = h.CustomerId
        JOIN dbo.InventoryLocation l ON l.LocationId = h.BusinessLocationId
        LEFT JOIN dbo.RubberAuctionLocation al ON al.RubberAuctionLocationId = h.RubberAuctionLocationId
        LEFT JOIN dbo.MemberLoyaltyAccount a ON a.CustomerId = h.CustomerId
        LEFT JOIN dbo.MemberLoyaltyTransaction t ON t.MemberLoyaltyAccountId = a.MemberLoyaltyAccountId AND t.SourceType = N'RUBBER_PURCHASE' AND t.ReferenceId = h.RubberPurchaseHeaderId AND t.TransactionType = N'EARN_FROM_RUBBER_PURCHASE'
        WHERE (@CustomerId IS NULL OR h.CustomerId = @CustomerId)
          AND (@BusinessLocationId IS NULL OR h.BusinessLocationId = @BusinessLocationId)
          AND (@RubberAuctionLocationId IS NULL OR h.RubberAuctionLocationId = @RubberAuctionLocationId)
          AND (@PaymentStatus IS NULL OR h.PaymentStatus = @PaymentStatus)
          AND (@DateFrom IS NULL OR h.TransactionDate >= @DateFrom)
          AND (@DateTo IS NULL OR h.TransactionDate < DATEADD(DAY, 1, @DateTo))
          AND (@SearchText IS NULL OR c.CustomerCode LIKE N'%'+@SearchText+N'%' OR c.CustomerName LIKE N'%'+@SearchText+N'%' OR c.PhoneNumber LIKE N'%'+@SearchText+N'%' OR h.NonMemberFarmerName LIKE N'%'+@SearchText+N'%' OR h.NonMemberFarmerPhone LIKE N'%'+@SearchText+N'%')
    )
    SELECT *
    FROM q
    ORDER BY TransactionDate DESC, RubberPurchaseHeaderId DESC
    OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerGetPaged
    @PageNumber INT,
    @PageSize INT,
    @SearchText NVARCHAR(255)=NULL,
    @MemberType NVARCHAR(30)=NULL,
    @MemberLevelId INT=NULL,
    @IsActive BIT=NULL,
    @CreditStatus NVARCHAR(30)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH type_agg AS
    (
        SELECT cmt.CustomerId, STRING_AGG(mt.MemberTypeCode, N',') WITHIN GROUP (ORDER BY mt.MemberTypeCode) ActiveMemberTypeCodes
        FROM dbo.CustomerMemberType cmt
        JOIN dbo.MemberType mt ON mt.MemberTypeId = cmt.MemberTypeId
        WHERE cmt.IsActive = 1
        GROUP BY cmt.CustomerId
    ),
    q AS
    (
        SELECT c.CustomerId,c.CustomerCode,c.CustomerName,c.PhoneNumber,c.Email,c.MemberType,ISNULL(ta.ActiveMemberTypeCodes, CASE WHEN c.MemberType=N'Wholesale' THEN N'WHOLESALE' ELSE N'RETAIL' END) ActiveMemberTypeCodes,
            ml.LevelName AS MemberLevelName,ISNULL(pb.AvailablePoints,0) AvailablePoints,ISNULL(mla.RubberWeightCarryForwardKg,0) RubberWeightCarryForwardKg,
            ISNULL(cc.CreditLimit,0) CreditLimit,ISNULL(cc.CurrentOutstandingAmount,0) CurrentOutstandingAmount,ISNULL(cc.AvailableCredit,0) AvailableCredit,ISNULL(cc.CreditStatus,N'Good') CreditStatus,
            c.TotalSpending,c.TotalPurchaseCount,c.LastPurchaseDate,c.IsActive,COUNT(1) OVER() TotalCount
        FROM dbo.Customer c
        LEFT JOIN type_agg ta ON ta.CustomerId = c.CustomerId
        LEFT JOIN dbo.MemberLevel ml ON ml.MemberLevelId=c.MemberLevelId
        LEFT JOIN dbo.CustomerPointBalance pb ON pb.CustomerId=c.CustomerId
        LEFT JOIN dbo.MemberLoyaltyAccount mla ON mla.CustomerId=c.CustomerId
        LEFT JOIN dbo.CustomerCredit cc ON cc.CustomerId=c.CustomerId
        WHERE (@SearchText IS NULL OR c.CustomerCode LIKE N'%'+@SearchText+N'%' OR c.CustomerName LIKE N'%'+@SearchText+N'%' OR c.PhoneNumber LIKE N'%'+@SearchText+N'%' OR c.Email LIKE N'%'+@SearchText+N'%')
          AND (@MemberType IS NULL OR c.MemberType=@MemberType OR EXISTS(SELECT 1 FROM dbo.CustomerMemberType fx JOIN dbo.MemberType fmt ON fmt.MemberTypeId=fx.MemberTypeId WHERE fx.CustomerId=c.CustomerId AND fx.IsActive=1 AND fmt.MemberTypeCode=UPPER(@MemberType)))
          AND (@MemberLevelId IS NULL OR c.MemberLevelId=@MemberLevelId)
          AND (@IsActive IS NULL OR c.IsActive=@IsActive)
          AND (@CreditStatus IS NULL OR cc.CreditStatus=@CreditStatus)
    )
    SELECT * FROM q ORDER BY CustomerName OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerGetById @CustomerId INT AS
BEGIN
    SET NOCOUNT ON;
    ;WITH type_agg AS
    (
        SELECT cmt.CustomerId, STRING_AGG(mt.MemberTypeCode, N',') WITHIN GROUP (ORDER BY mt.MemberTypeCode) ActiveMemberTypeCodes
        FROM dbo.CustomerMemberType cmt JOIN dbo.MemberType mt ON mt.MemberTypeId = cmt.MemberTypeId
        WHERE cmt.IsActive = 1 AND cmt.CustomerId = @CustomerId
        GROUP BY cmt.CustomerId
    )
    SELECT c.*,ISNULL(ta.ActiveMemberTypeCodes, CASE WHEN c.MemberType=N'Wholesale' THEN N'WHOLESALE' ELSE N'RETAIL' END) ActiveMemberTypeCodes,
           ml.LevelCode AS MemberLevelCode,ml.LevelName AS MemberLevelName,ISNULL(ml.DiscountPercent,0) DiscountPercent,
           ISNULL(pb.AvailablePoints,0) AvailablePoints,ISNULL(pb.LifetimeEarnedPoints,0) LifetimeEarnedPoints,ISNULL(pb.LifetimeRedeemedPoints,0) LifetimeRedeemedPoints,
           ISNULL(cc.AllowCredit,0) AllowCredit,ISNULL(cc.CreditLimit,0) CreditLimit,ISNULL(cc.CreditTermDays,0) CreditTermDays,
           ISNULL(cc.CurrentOutstandingAmount,0) CurrentOutstandingAmount,ISNULL(cc.AvailableCredit,0) AvailableCredit,ISNULL(cc.CreditStatus,N'Good') CreditStatus,
           ISNULL(cc.RequireManagerApproval,0) RequireManagerApproval,
           wp.BusinessName WholesaleBusinessName,ISNULL(wp.IsApproved,0) WholesaleApproved,ISNULL(wp.PaymentTermDays,0) WholesalePaymentTermDays,
           rp.SupplierCode RubberSupplierCode,
           ISNULL(mla.RubberWeightCarryForwardKg,0) RubberWeightCarryForwardKg,ISNULL(mla.PointBalance,0) RubberLoyaltyPointBalance
    FROM dbo.Customer c
    LEFT JOIN type_agg ta ON ta.CustomerId=c.CustomerId
    LEFT JOIN dbo.MemberLevel ml ON ml.MemberLevelId=c.MemberLevelId
    LEFT JOIN dbo.CustomerPointBalance pb ON pb.CustomerId=c.CustomerId
    LEFT JOIN dbo.CustomerCredit cc ON cc.CustomerId=c.CustomerId
    LEFT JOIN dbo.WholesaleMemberProfile wp ON wp.CustomerId=c.CustomerId
    LEFT JOIN dbo.RubberSupplierMemberProfile rp ON rp.CustomerId=c.CustomerId
    LEFT JOIN dbo.MemberLoyaltyAccount mla ON mla.CustomerId=c.CustomerId
    WHERE c.CustomerId=@CustomerId;
END
GO

CREATE OR ALTER PROCEDURE dbo.spCustomerReportGetSummary
    @DateFrom DATETIME2=NULL,
    @DateTo DATETIME2=NULL,
    @MemberType NVARCHAR(30)=NULL,
    @MemberLevelId INT=NULL,
    @IsActive BIT=NULL,
    @Top INT=20,
    @NoPurchaseAfterDate DATETIME2=NULL
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH flags AS
    (
        SELECT
            c.CustomerId,
            c.IsActive,
            c.MemberLevelId,
            c.MemberType,
            c.CreatedDate,
            c.TotalSpending,
            ISNULL(cc.CurrentOutstandingAmount,0) CurrentOutstandingAmount,
            ISNULL(cc.AllowCredit,0) AllowCredit,
            ISNULL(cc.CreditStatus,N'Good') CreditStatus,
            ISNULL(pb.AvailablePoints,0) AvailablePoints,
            CASE WHEN EXISTS(SELECT 1 FROM dbo.CustomerMemberType cmt JOIN dbo.MemberType mt ON mt.MemberTypeId=cmt.MemberTypeId WHERE cmt.CustomerId=c.CustomerId AND cmt.IsActive=1 AND mt.MemberTypeCode=N'RETAIL') THEN 1 ELSE 0 END HasRetail,
            CASE WHEN EXISTS(SELECT 1 FROM dbo.CustomerMemberType cmt JOIN dbo.MemberType mt ON mt.MemberTypeId=cmt.MemberTypeId WHERE cmt.CustomerId=c.CustomerId AND cmt.IsActive=1 AND mt.MemberTypeCode=N'WHOLESALE') THEN 1 ELSE 0 END HasWholesale,
            CASE WHEN EXISTS(SELECT 1 FROM dbo.CustomerMemberType cmt JOIN dbo.MemberType mt ON mt.MemberTypeId=cmt.MemberTypeId WHERE cmt.CustomerId=c.CustomerId AND cmt.IsActive=1 AND mt.MemberTypeCode=N'RUBBER_SUPPLIER') THEN 1 ELSE 0 END HasRubberSupplier,
            CASE WHEN EXISTS(SELECT 1 FROM dbo.CustomerCreditTransaction t WHERE t.CustomerId=c.CustomerId AND t.Status IN(N'Overdue')) THEN 1 ELSE 0 END HasOverdueTransaction
        FROM dbo.Customer c
        LEFT JOIN dbo.CustomerCredit cc ON cc.CustomerId=c.CustomerId
        LEFT JOIN dbo.CustomerPointBalance pb ON pb.CustomerId=c.CustomerId
        WHERE (@MemberType IS NULL OR c.MemberType=@MemberType OR EXISTS(SELECT 1 FROM dbo.CustomerMemberType fx JOIN dbo.MemberType fmt ON fmt.MemberTypeId=fx.MemberTypeId WHERE fx.CustomerId=c.CustomerId AND fx.IsActive=1 AND fmt.MemberTypeCode=UPPER(@MemberType)))
          AND (@MemberLevelId IS NULL OR c.MemberLevelId=@MemberLevelId)
          AND (@IsActive IS NULL OR c.IsActive=@IsActive)
    )
    SELECT
        COUNT(1) TotalCustomers,
        COALESCE(SUM(CASE WHEN IsActive=1 THEN 1 ELSE 0 END),0) ActiveCustomers,
        COALESCE(SUM(HasRetail),0) RetailMemberCount,
        COALESCE(SUM(HasWholesale),0) WholesaleMemberCount,
        COALESCE(SUM(HasRubberSupplier),0) RubberSupplierMemberCount,
        COALESCE(SUM(CASE WHEN (@DateFrom IS NOT NULL AND CreatedDate>=@DateFrom AND (@DateTo IS NULL OR CreatedDate<DATEADD(DAY,1,@DateTo))) THEN 1 ELSE 0 END),0) NewCustomers,
        COALESCE(SUM(TotalSpending),0) TotalCustomerSpending,
        COALESCE(SUM(CurrentOutstandingAmount),0) TotalOutstandingCredit,
        COALESCE(SUM(AvailablePoints),0) TotalAvailablePoints,
        COALESCE(SUM(CASE WHEN AllowCredit=1 THEN 1 ELSE 0 END),0) TotalCreditCustomers,
        COALESCE(SUM(CASE WHEN CreditStatus=N'Overdue' OR HasOverdueTransaction=1 THEN 1 ELSE 0 END),0) TotalOverdueCustomers
    FROM flags;
END
GO
