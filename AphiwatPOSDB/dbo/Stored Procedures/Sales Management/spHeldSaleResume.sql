CREATE PROCEDURE [dbo].[spHeldSaleResume] @HeldSaleHeaderId BIGINT,@UpdatedByUserId INT AS BEGIN SET NOCOUNT ON; UPDATE dbo.HeldSaleHeader SET Status=N'Resumed', UpdatedByUserId=@UpdatedByUserId, UpdatedDate=SYSUTCDATETIME() WHERE HeldSaleHeaderId=@HeldSaleHeaderId AND Status=N'Held'; EXEC dbo.spHeldSaleGetById @HeldSaleHeaderId; END;

