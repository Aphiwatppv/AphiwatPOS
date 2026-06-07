CREATE TABLE [dbo].[CustomerAuditLog]
(
    CustomerAuditLogId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerAuditLog PRIMARY KEY,
    CustomerId INT NULL,
    ActionType NVARCHAR(50) NOT NULL,
    EntityName NVARCHAR(100) NOT NULL,
    EntityId BIGINT NULL,
    OldValue NVARCHAR(MAX) NULL,
    NewValue NVARCHAR(MAX) NULL,
    Remark NVARCHAR(1000) NULL,
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_CustomerAuditLog_CreatedDate DEFAULT(SYSUTCDATETIME()),
    CreatedByUserId INT NOT NULL,
    CONSTRAINT FK_CustomerAuditLog_Customer FOREIGN KEY(CustomerId) REFERENCES dbo.Customer(CustomerId)
);
