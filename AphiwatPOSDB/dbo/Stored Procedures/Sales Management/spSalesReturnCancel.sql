CREATE PROCEDURE [dbo].[spSalesReturnCancel] @SalesReturnHeaderId BIGINT,@Reason NVARCHAR(500),@UpdatedByUserId INT AS BEGIN SET NOCOUNT ON; UPDATE dbo.SalesReturnHeader SET Status=N'Cancelled', Reason=CONCAT(Reason,N' | Cancelled: ',@Reason), UpdatedByUserId=@UpdatedByUserId, UpdatedDate=SYSUTCDATETIME() WHERE SalesReturnHeaderId=@SalesReturnHeaderId AND Status IN (N'Draft',N'Approved'); END;

