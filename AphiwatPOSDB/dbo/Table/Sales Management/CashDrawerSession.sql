CREATE TABLE [dbo].[CashDrawerSession]
(
    [SessionId] BIGINT IDENTITY(1,1) NOT NULL,
    [CashierUserId] INT NOT NULL,
    [StartingCash] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_CashDrawerSession_StartingCash] DEFAULT (0),
    [CashSales] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_CashDrawerSession_CashSales] DEFAULT (0),
    [CashIn] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_CashDrawerSession_CashIn] DEFAULT (0),
    [CashOut] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_CashDrawerSession_CashOut] DEFAULT (0),
    [CashRefund] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_CashDrawerSession_CashRefund] DEFAULT (0),
    [ExpectedCash] AS ([StartingCash] + [CashSales] + [CashIn] - [CashOut] - [CashRefund]) PERSISTED,
    [ActualCash] DECIMAL(18,2) NULL,
    [Difference] DECIMAL(18,2) NULL,
    [OpenedByUserId] INT NOT NULL,
    [ClosedByUserId] INT NULL,
    [ApprovedByUserId] INT NULL,
    [OpenedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_CashDrawerSession_OpenedDate] DEFAULT (SYSUTCDATETIME()),
    [ClosedDate] DATETIME2(0) NULL,
    [Status] NVARCHAR(20) NOT NULL CONSTRAINT [DF_CashDrawerSession_Status] DEFAULT (N'Open'),
    [CloseNote] NVARCHAR(1000) NOT NULL CONSTRAINT [DF_CashDrawerSession_CloseNote] DEFAULT (N''),
    CONSTRAINT [PK_CashDrawerSession] PRIMARY KEY CLUSTERED ([SessionId]),
    CONSTRAINT [CK_CashDrawerSession_Status] CHECK ([Status] IN (N'Open',N'Closed',N'Approved')),
    CONSTRAINT [FK_CashDrawerSession_Cashier] FOREIGN KEY ([CashierUserId]) REFERENCES [dbo].[AccessUser] ([UserId]),
    CONSTRAINT [FK_CashDrawerSession_OpenedBy] FOREIGN KEY ([OpenedByUserId]) REFERENCES [dbo].[AccessUser] ([UserId]),
    CONSTRAINT [FK_CashDrawerSession_ClosedBy] FOREIGN KEY ([ClosedByUserId]) REFERENCES [dbo].[AccessUser] ([UserId]),
    CONSTRAINT [FK_CashDrawerSession_ApprovedBy] FOREIGN KEY ([ApprovedByUserId]) REFERENCES [dbo].[AccessUser] ([UserId])
);
GO
CREATE UNIQUE INDEX [UX_CashDrawerSession_OneOpenPerCashier]
ON [dbo].[CashDrawerSession] ([CashierUserId])
WHERE [Status] = N'Open';
