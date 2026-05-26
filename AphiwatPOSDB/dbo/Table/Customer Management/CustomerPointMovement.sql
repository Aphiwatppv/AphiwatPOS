CREATE TABLE [dbo].[CustomerPointMovement]
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
