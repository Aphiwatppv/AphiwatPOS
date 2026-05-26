CREATE TABLE [dbo].[ReceiptPrintHistory]
(
    [ReceiptPrintHistoryId] BIGINT IDENTITY(1,1) NOT NULL,
    [SalesHeaderId] BIGINT NULL,
    [SalesReturnHeaderId] BIGINT NULL,
    [ReceiptNo] NVARCHAR(50) NOT NULL,
    [ReceiptType] NVARCHAR(20) NOT NULL,
    [PrintedByUserId] INT NOT NULL,
    [PrintedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_ReceiptPrintHistory_PrintedDate] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_ReceiptPrintHistory] PRIMARY KEY CLUSTERED ([ReceiptPrintHistoryId]),
    CONSTRAINT [FK_ReceiptPrintHistory_SalesHeader] FOREIGN KEY ([SalesHeaderId]) REFERENCES [dbo].[SalesHeader] ([SalesHeaderId]),
    CONSTRAINT [FK_ReceiptPrintHistory_SalesReturnHeader] FOREIGN KEY ([SalesReturnHeaderId]) REFERENCES [dbo].[SalesReturnHeader] ([SalesReturnHeaderId]),
    CONSTRAINT [FK_ReceiptPrintHistory_PrintedBy] FOREIGN KEY ([PrintedByUserId]) REFERENCES [dbo].[AccessUser] ([UserId]),
    CONSTRAINT [CK_ReceiptPrintHistory_Type] CHECK ([ReceiptType] IN (N'Sale',N'Return')),
    CONSTRAINT [CK_ReceiptPrintHistory_Parent] CHECK (([SalesHeaderId] IS NOT NULL AND [SalesReturnHeaderId] IS NULL) OR ([SalesHeaderId] IS NULL AND [SalesReturnHeaderId] IS NOT NULL))
);
