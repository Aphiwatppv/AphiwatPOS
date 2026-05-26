CREATE PROCEDURE [dbo].[spCustomerNoteToggleActive] @CustomerNoteId BIGINT,@IsActive BIT,@UpdatedByUserId INT AS
BEGIN UPDATE dbo.CustomerNote SET IsActive=@IsActive,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@UpdatedByUserId WHERE CustomerNoteId=@CustomerNoteId; END
