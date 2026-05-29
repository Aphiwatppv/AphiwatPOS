CREATE TABLE [dbo].[CashDrawerTransaction]
(
    [TransactionId] BIGINT IDENTITY(1,1) NOT NULL,
    [SessionId] BIGINT NOT NULL,
    [TransactionType] NVARCHAR(30) NOT NULL,
    [Amount] DECIMAL(18,2) NOT NULL,
    [Reason] NVARCHAR(500) NOT NULL CONSTRAINT [DF_CashDrawerTransaction_Reason] DEFAULT (N''),
    [ReferenceNo] NVARCHAR(100) NOT NULL CONSTRAINT [DF_CashDrawerTransaction_ReferenceNo] DEFAULT (N''),
    [SaleId] BIGINT NULL,
    [CreatedByUserId] INT NOT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_CashDrawerTransaction_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_CashDrawerTransaction] PRIMARY KEY CLUSTERED ([TransactionId]),
    CONSTRAINT [CK_CashDrawerTransaction_Type] CHECK ([TransactionType] IN (N'StartShift',N'CashSale',N'CashIn',N'CashOut',N'CashRefund',N'CloseShift')),
    CONSTRAINT [CK_CashDrawerTransaction_Amount] CHECK ([Amount] >= 0),
    CONSTRAINT [FK_CashDrawerTransaction_Session] FOREIGN KEY ([SessionId]) REFERENCES [dbo].[CashDrawerSession] ([SessionId]),
    CONSTRAINT [FK_CashDrawerTransaction_Sale] FOREIGN KEY ([SaleId]) REFERENCES [dbo].[SalesHeader] ([SalesHeaderId]),
    CONSTRAINT [FK_CashDrawerTransaction_CreatedBy] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[AccessUser] ([UserId])
);
