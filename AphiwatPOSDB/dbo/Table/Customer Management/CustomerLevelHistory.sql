CREATE TABLE [dbo].[CustomerLevelHistory]
(
    CustomerLevelHistoryId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerLevelHistory PRIMARY KEY,
    CustomerId INT NOT NULL,
    OldMemberLevelId INT NULL,
    NewMemberLevelId INT NOT NULL,
    ChangeReason NVARCHAR(500) NULL,
    ChangedDate DATETIME2 NOT NULL CONSTRAINT DF_CustomerLevelHistory_ChangedDate DEFAULT(SYSDATETIME()),
    ChangedByUserId INT NOT NULL,
    CONSTRAINT FK_CustomerLevelHistory_Customer FOREIGN KEY(CustomerId) REFERENCES dbo.Customer(CustomerId),
    CONSTRAINT FK_CustomerLevelHistory_OldLevel FOREIGN KEY(OldMemberLevelId) REFERENCES dbo.MemberLevel(MemberLevelId),
    CONSTRAINT FK_CustomerLevelHistory_NewLevel FOREIGN KEY(NewMemberLevelId) REFERENCES dbo.MemberLevel(MemberLevelId)
);
