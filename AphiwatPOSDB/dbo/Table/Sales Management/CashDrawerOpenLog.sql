CREATE TABLE [dbo].[CashDrawerOpenLog]
(
    [OpenLogId] BIGINT IDENTITY(1,1) NOT NULL,
    [SessionId] BIGINT NULL,
    [SaleId] BIGINT NULL,
    [OpenType] NVARCHAR(30) NOT NULL,
    [Reason] NVARCHAR(500) NOT NULL CONSTRAINT [DF_CashDrawerOpenLog_Reason] DEFAULT (N''),
    [IsSuccess] BIT NOT NULL,
    [ErrorMessage] NVARCHAR(1000) NOT NULL CONSTRAINT [DF_CashDrawerOpenLog_ErrorMessage] DEFAULT (N''),
    [OpenedByUserId] INT NOT NULL,
    [OpenedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_CashDrawerOpenLog_OpenedDate] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_CashDrawerOpenLog] PRIMARY KEY CLUSTERED ([OpenLogId]),
    CONSTRAINT [CK_CashDrawerOpenLog_OpenType] CHECK ([OpenType] IN (N'CashSale',N'Manual',N'Test',N'PrinterError')),
    CONSTRAINT [FK_CashDrawerOpenLog_Session] FOREIGN KEY ([SessionId]) REFERENCES [dbo].[CashDrawerSession] ([SessionId]),
    CONSTRAINT [FK_CashDrawerOpenLog_Sale] FOREIGN KEY ([SaleId]) REFERENCES [dbo].[SalesHeader] ([SalesHeaderId]),
    CONSTRAINT [FK_CashDrawerOpenLog_OpenedBy] FOREIGN KEY ([OpenedByUserId]) REFERENCES [dbo].[AccessUser] ([UserId])
);
