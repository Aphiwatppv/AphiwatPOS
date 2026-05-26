IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Customer_Email_NotNull' AND object_id = OBJECT_ID(N'dbo.Customer'))
CREATE UNIQUE INDEX UX_Customer_Email_NotNull ON dbo.Customer(Email) WHERE Email IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CustomerNote_Customer')
ALTER TABLE dbo.CustomerNote ADD CONSTRAINT FK_CustomerNote_Customer FOREIGN KEY(CustomerId) REFERENCES dbo.Customer(CustomerId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Customer_MemberLevel_IsActive' AND object_id = OBJECT_ID(N'dbo.Customer'))
CREATE INDEX IX_Customer_MemberLevel_IsActive ON dbo.Customer(MemberLevelId, IsActive);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_MemberLevelUpgradeRule_ActiveFrom' AND object_id = OBJECT_ID(N'dbo.MemberLevelUpgradeRule'))
CREATE UNIQUE INDEX UX_MemberLevelUpgradeRule_ActiveFrom ON dbo.MemberLevelUpgradeRule(FromMemberLevelId) WHERE IsActive = 1;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CustomerPointMovement_Customer_Date' AND object_id = OBJECT_ID(N'dbo.CustomerPointMovement'))
CREATE INDEX IX_CustomerPointMovement_Customer_Date ON dbo.CustomerPointMovement(CustomerId, CreatedDate DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CustomerCreditTransaction_Customer_Date' AND object_id = OBJECT_ID(N'dbo.CustomerCreditTransaction'))
CREATE INDEX IX_CustomerCreditTransaction_Customer_Date ON dbo.CustomerCreditTransaction(CustomerId, CreatedDate DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CustomerCreditTransaction_Overdue' AND object_id = OBJECT_ID(N'dbo.CustomerCreditTransaction'))
CREATE INDEX IX_CustomerCreditTransaction_Overdue ON dbo.CustomerCreditTransaction(CustomerId, DueDate, Status) WHERE Status IN (N'Unpaid', N'PartiallyPaid', N'Overdue');
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CustomerNote_Customer_Important' AND object_id = OBJECT_ID(N'dbo.CustomerNote'))
CREATE INDEX IX_CustomerNote_Customer_Important ON dbo.CustomerNote(CustomerId, IsActive, IsImportant DESC, CreatedDate DESC);
GO
