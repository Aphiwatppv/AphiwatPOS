CREATE TABLE [dbo].[CustomerPointBalance]
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
