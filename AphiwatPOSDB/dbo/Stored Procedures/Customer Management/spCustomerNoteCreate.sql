CREATE PROCEDURE [dbo].[spCustomerNoteCreate] @CustomerId INT,@NoteType NVARCHAR(30),@NoteText NVARCHAR(2000),@IsImportant BIT=0,@CreatedByUserId INT AS
BEGIN INSERT dbo.CustomerNote(CustomerId,NoteType,NoteText,IsImportant,CreatedByUserId) VALUES(@CustomerId,@NoteType,@NoteText,@IsImportant,@CreatedByUserId); SELECT CONVERT(BIGINT,SCOPE_IDENTITY()); END
