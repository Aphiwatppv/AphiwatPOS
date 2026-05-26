CREATE TABLE [dbo].[PaymentMethod]
(
    [PaymentMethodId] INT IDENTITY(1,1) NOT NULL,
    [PaymentMethodCode] NVARCHAR(50) NOT NULL,
    [PaymentMethodName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NOT NULL CONSTRAINT [DF_PaymentMethod_Description] DEFAULT (N''),
    [RequireReferenceNo] BIT NOT NULL CONSTRAINT [DF_PaymentMethod_RequireReferenceNo] DEFAULT (0),
    [IsCash] BIT NOT NULL CONSTRAINT [DF_PaymentMethod_IsCash] DEFAULT (0),
    [IsActive] BIT NOT NULL CONSTRAINT [DF_PaymentMethod_IsActive] DEFAULT (1),
    [DisplayOrder] INT NOT NULL CONSTRAINT [DF_PaymentMethod_DisplayOrder] DEFAULT (0),
    [CreatedByUserId] INT NULL,
    [UpdatedByUserId] INT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_PaymentMethod_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    [UpdatedDate] DATETIME2(0) NULL,
    CONSTRAINT [PK_PaymentMethod] PRIMARY KEY CLUSTERED ([PaymentMethodId]),
    CONSTRAINT [UQ_PaymentMethod_Code] UNIQUE ([PaymentMethodCode]),
    CONSTRAINT [FK_PaymentMethod_CreatedBy] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[AccessUser] ([UserId]),
    CONSTRAINT [FK_PaymentMethod_UpdatedBy] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[AccessUser] ([UserId])
);
