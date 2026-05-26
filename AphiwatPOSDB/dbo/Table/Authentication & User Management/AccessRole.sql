CREATE TABLE [dbo].[AccessRole]
(
    [RoleId] INT IDENTITY(1,1) NOT NULL,
    [RoleCode] NVARCHAR(50) NULL,
    [RoleName] NVARCHAR(50) NOT NULL,
    [Description] NVARCHAR(200) NOT NULL CONSTRAINT [DF_AccessRole_Description] DEFAULT (N''),
    [IsActive] BIT NOT NULL CONSTRAINT [DF_AccessRole_IsActive] DEFAULT (1),
    [CreatedAtUtc] DATETIME2(0) NOT NULL CONSTRAINT [DF_AccessRole_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
    [CreatedByUserId] INT NULL,
    [UpdatedAtUtc] DATETIME2(0) NULL,
    [UpdatedByUserId] INT NULL,
    CONSTRAINT [PK_AccessRole] PRIMARY KEY CLUSTERED ([RoleId] ASC),
    CONSTRAINT [UQ_AccessRole_RoleName] UNIQUE ([RoleName])
);
