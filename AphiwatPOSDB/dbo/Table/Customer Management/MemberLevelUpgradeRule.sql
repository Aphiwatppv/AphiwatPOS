CREATE TABLE [dbo].[MemberLevelUpgradeRule]
(
    MemberLevelUpgradeRuleId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MemberLevelUpgradeRule PRIMARY KEY,
    FromMemberLevelId INT NOT NULL,
    ToMemberLevelId INT NOT NULL,
    RequiredTotalSpending DECIMAL(18,2) NOT NULL CONSTRAINT DF_MemberLevelUpgradeRule_RequiredTotalSpending DEFAULT(0),
    RequiredPurchaseCount INT NOT NULL CONSTRAINT DF_MemberLevelUpgradeRule_RequiredPurchaseCount DEFAULT(0),
    RequiredMembershipDays INT NOT NULL CONSTRAINT DF_MemberLevelUpgradeRule_RequiredMembershipDays DEFAULT(0),
    RequireNoOverduePayment BIT NOT NULL CONSTRAINT DF_MemberLevelUpgradeRule_RequireNoOverduePayment DEFAULT(1),
    RequireManagerApproval BIT NOT NULL CONSTRAINT DF_MemberLevelUpgradeRule_RequireManagerApproval DEFAULT(0),
    IsActive BIT NOT NULL CONSTRAINT DF_MemberLevelUpgradeRule_IsActive DEFAULT(1),
    CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_MemberLevelUpgradeRule_CreatedDate DEFAULT(SYSDATETIME()),
    CreatedByUserId INT NOT NULL,
    UpdatedDate DATETIME2 NULL,
    UpdatedByUserId INT NULL,
    CONSTRAINT FK_MemberLevelUpgradeRule_From FOREIGN KEY(FromMemberLevelId) REFERENCES dbo.MemberLevel(MemberLevelId),
    CONSTRAINT FK_MemberLevelUpgradeRule_To FOREIGN KEY(ToMemberLevelId) REFERENCES dbo.MemberLevel(MemberLevelId),
    CONSTRAINT CK_MemberLevelUpgradeRule_DifferentLevels CHECK (FromMemberLevelId <> ToMemberLevelId),
    CONSTRAINT CK_MemberLevelUpgradeRule_NonNegative CHECK (RequiredTotalSpending >= 0 AND RequiredPurchaseCount >= 0 AND RequiredMembershipDays >= 0)
);
