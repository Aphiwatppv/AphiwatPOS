CREATE PROCEDURE [dbo].[spHeldSaleExpireOld] @ExpireBeforeDate DATETIME2(0),@UpdatedByUserId INT AS BEGIN SET NOCOUNT ON; UPDATE dbo.HeldSaleHeader SET Status=N'Expired', UpdatedByUserId=@UpdatedByUserId, UpdatedDate=SYSUTCDATETIME() WHERE Status IN (N'Held',N'Resumed') AND HeldDate<@ExpireBeforeDate; SELECT @@ROWCOUNT; END;

