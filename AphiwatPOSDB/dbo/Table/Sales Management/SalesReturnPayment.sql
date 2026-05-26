CREATE TABLE [dbo].[SalesReturnPayment]
(
    [SalesReturnPaymentId] BIGINT IDENTITY(1,1) NOT NULL,
    [SalesReturnHeaderId] BIGINT NOT NULL,
    [PaymentMethodId] INT NOT NULL,
    [RefundAmount] DECIMAL(18,4) NOT NULL,
    [ReferenceNo] NVARCHAR(100) NULL,
    [PaymentDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_SalesReturnPayment_PaymentDate] DEFAULT (SYSUTCDATETIME()),
    [CreatedByUserId] INT NOT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_SalesReturnPayment_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_SalesReturnPayment] PRIMARY KEY CLUSTERED ([SalesReturnPaymentId]),
    CONSTRAINT [FK_SalesReturnPayment_Header] FOREIGN KEY ([SalesReturnHeaderId]) REFERENCES [dbo].[SalesReturnHeader] ([SalesReturnHeaderId]),
    CONSTRAINT [FK_SalesReturnPayment_Method] FOREIGN KEY ([PaymentMethodId]) REFERENCES [dbo].[PaymentMethod] ([PaymentMethodId]),
    CONSTRAINT [FK_SalesReturnPayment_CreatedBy] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[AccessUser] ([UserId]),
    CONSTRAINT [CK_SalesReturnPayment_Amount] CHECK ([RefundAmount] > 0)
);
