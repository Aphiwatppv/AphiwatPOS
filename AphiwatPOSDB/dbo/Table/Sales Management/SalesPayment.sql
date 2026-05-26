CREATE TABLE [dbo].[SalesPayment]
(
    [SalesPaymentId] BIGINT IDENTITY(1,1) NOT NULL,
    [SalesHeaderId] BIGINT NOT NULL,
    [PaymentMethodId] INT NOT NULL,
    [PaymentAmount] DECIMAL(18,4) NOT NULL,
    [ReferenceNo] NVARCHAR(100) NULL,
    [PaymentDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_SalesPayment_PaymentDate] DEFAULT (SYSUTCDATETIME()),
    [CreatedByUserId] INT NOT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_SalesPayment_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_SalesPayment] PRIMARY KEY CLUSTERED ([SalesPaymentId]),
    CONSTRAINT [FK_SalesPayment_Header] FOREIGN KEY ([SalesHeaderId]) REFERENCES [dbo].[SalesHeader] ([SalesHeaderId]),
    CONSTRAINT [FK_SalesPayment_Method] FOREIGN KEY ([PaymentMethodId]) REFERENCES [dbo].[PaymentMethod] ([PaymentMethodId]),
    CONSTRAINT [FK_SalesPayment_CreatedBy] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[AccessUser] ([UserId]),
    CONSTRAINT [CK_SalesPayment_Amount] CHECK ([PaymentAmount] > 0)
);
