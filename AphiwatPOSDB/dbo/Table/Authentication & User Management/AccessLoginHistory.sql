CREATE TABLE [dbo].[AccessLoginHistory]
(
    [LoginHistoryId] BIGINT IDENTITY(1,1) NOT NULL,
    [UserId] INT NULL,
    [Username] NVARCHAR(50) NOT NULL,
    [Succeeded] BIT NOT NULL,
    [FailureReason] NVARCHAR(200) NOT NULL CONSTRAINT [DF_AccessLoginHistory_FailureReason] DEFAULT (N''),
    [AttemptedAtUtc] DATETIME2(0) NOT NULL CONSTRAINT [DF_AccessLoginHistory_AttemptedAtUtc] DEFAULT (SYSUTCDATETIME()),
    [LogoutAtUtc] DATETIME2(0) NULL,
    CONSTRAINT [PK_AccessLoginHistory] PRIMARY KEY CLUSTERED ([LoginHistoryId] ASC),
    CONSTRAINT [FK_AccessLoginHistory_AccessUser] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AccessUser] ([UserId])
);
