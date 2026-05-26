CREATE TABLE [dbo].[AccessUser]
(
    [UserId] INT IDENTITY(1,1) NOT NULL,
    [Username] NVARCHAR(50) NOT NULL,
    [DisplayName] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(254) NOT NULL CONSTRAINT [DF_AccessUser_Email] DEFAULT (N''),
    [ProfileImageUrl] NVARCHAR(500) NOT NULL CONSTRAINT [DF_AccessUser_ProfileImageUrl] DEFAULT (N''),
    [PasswordHash] NVARCHAR(500) NOT NULL,
    [IsActive] BIT NOT NULL CONSTRAINT [DF_AccessUser_IsActive] DEFAULT (1),
    [IsLocked] BIT NOT NULL CONSTRAINT [DF_AccessUser_IsLocked] DEFAULT (0),
    [LockoutEndAtUtc] DATETIME2(0) NULL,
    [AccessFailedCount] INT NOT NULL CONSTRAINT [DF_AccessUser_AccessFailedCount] DEFAULT (0),
    [CreatedAtUtc] DATETIME2(0) NOT NULL CONSTRAINT [DF_AccessUser_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
    [CreatedByUserId] INT NULL,
    [UpdatedAtUtc] DATETIME2(0) NULL,
    [UpdatedByUserId] INT NULL,
    [LastLoginAtUtc] DATETIME2(0) NULL,
    CONSTRAINT [PK_AccessUser] PRIMARY KEY CLUSTERED ([UserId] ASC),
    CONSTRAINT [UQ_AccessUser_Username] UNIQUE ([Username])
);
