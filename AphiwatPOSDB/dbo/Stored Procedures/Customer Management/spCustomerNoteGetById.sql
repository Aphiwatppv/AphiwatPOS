CREATE PROCEDURE [dbo].[spCustomerNoteGetById] @CustomerNoteId BIGINT AS BEGIN SELECT * FROM dbo.CustomerNote WHERE CustomerNoteId=@CustomerNoteId; END
