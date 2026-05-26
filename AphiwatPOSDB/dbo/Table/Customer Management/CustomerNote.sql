CREATE TABLE [dbo].[CustomerNote]
(
    CustomerNoteId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerNote PRIMARY KEY,
    CustomerId INT NOT NULL,
    NoteType NVARCHAR(30) NOT NULL,
    NoteText NVARCHAR(2000) NOT NULL,
    IsImportant BIT NOT NULL CONSTRAINT DF_CustomerNote_IsImportant DEFAULT(0),
    IsActive BIT NOT NULL CONSTRAINT DF_CustomerNote_IsActive DEFAULT(1),
    CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_CustomerNote_CreatedDate DEFAULT(SYSDATETIME()),
    CreatedByUserId INT NOT NULL,
    UpdatedDate DATETIME2 NULL,
    UpdatedByUserId INT NULL,
    CONSTRAINT FK_CustomerNote_Customer FOREIGN KEY(CustomerId) REFERENCES dbo.Customer(CustomerId),
    CONSTRAINT CK_CustomerNote_NoteType CHECK (NoteType IN (N'General',N'Warning',N'Credit',N'Service',N'Complaint',N'FollowUp',N'Other')),
    CONSTRAINT CK_CustomerNote_NoteText CHECK (LEN(LTRIM(RTRIM(NoteText))) > 0)
);
