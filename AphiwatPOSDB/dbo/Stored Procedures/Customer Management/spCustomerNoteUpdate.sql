CREATE PROCEDURE [dbo].[spCustomerNoteUpdate] @CustomerNoteId BIGINT,@NoteType NVARCHAR(30),@NoteText NVARCHAR(2000),@IsImportant BIT=0,@IsActive BIT=1,@UpdatedByUserId INT AS
BEGIN UPDATE dbo.CustomerNote SET NoteType=@NoteType,NoteText=@NoteText,IsImportant=@IsImportant,IsActive=@IsActive,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@UpdatedByUserId WHERE CustomerNoteId=@CustomerNoteId; END
