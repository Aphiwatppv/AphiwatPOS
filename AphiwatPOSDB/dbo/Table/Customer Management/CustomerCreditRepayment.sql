CREATE TABLE [dbo].[CustomerCreditRepayment]
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
    CONSTRAINT FK_CustomerCreditRepayment_PaymentMethod FOREIGN KEY(PaymentMethodId) REFERENCES dbo.PaymentMethod(PaymentMethodId),
    CONSTRAINT CK_CustomerCreditRepayment_Status CHECK (Status IN (N'Completed',N'Cancelled',N'Voided')),
    CONSTRAINT CK_CustomerCreditRepayment_Amount CHECK (PaymentAmount > 0)
);
