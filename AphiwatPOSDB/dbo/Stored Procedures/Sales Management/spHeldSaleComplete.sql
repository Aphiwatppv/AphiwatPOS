CREATE PROCEDURE [dbo].[spHeldSaleComplete] @HeldSaleHeaderId BIGINT AS BEGIN SET NOCOUNT ON; UPDATE dbo.HeldSaleHeader SET Status=N'Completed', UpdatedDate=SYSUTCDATETIME() WHERE HeldSaleHeaderId=@HeldSaleHeaderId; END;

