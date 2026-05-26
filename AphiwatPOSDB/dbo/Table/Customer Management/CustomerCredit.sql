CREATE TABLE [dbo].[CustomerCredit]
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
