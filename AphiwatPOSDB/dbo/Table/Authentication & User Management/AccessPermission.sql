CREATE TABLE [dbo].[AccessPermission]
(
    [PermissionId] INT IDENTITY(1,1) NOT NULL,
    [PermissionCode] NVARCHAR(100) NOT NULL,
    [PermissionName] NVARCHAR(100) NOT NULL,
    [ModuleName] NVARCHAR(100) NOT NULL CONSTRAINT [DF_AccessPermission_ModuleName] DEFAULT (N''),
    [Description] NVARCHAR(250) NOT NULL CONSTRAINT [DF_AccessPermission_Description] DEFAULT (N''),
    [IsActive] BIT NOT NULL CONSTRAINT [DF_AccessPermission_IsActive] DEFAULT (1),
    [CreatedAtUtc] DATETIME2(0) NOT NULL CONSTRAINT [DF_AccessPermission_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
    [CreatedByUserId] INT NULL,
    [UpdatedAtUtc] DATETIME2(0) NULL,
    [UpdatedByUserId] INT NULL,
    CONSTRAINT [PK_AccessPermission] PRIMARY KEY CLUSTERED ([PermissionId] ASC),
    CONSTRAINT [UQ_AccessPermission_PermissionCode] UNIQUE ([PermissionCode])
);
