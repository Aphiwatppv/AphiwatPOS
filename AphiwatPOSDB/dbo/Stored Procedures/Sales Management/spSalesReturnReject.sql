CREATE PROCEDURE [dbo].[spSalesReturnReject] @SalesReturnHeaderId BIGINT,@Reason NVARCHAR(500),@UpdatedByUserId INT AS BEGIN SET NOCOUNT ON; UPDATE dbo.SalesReturnHeader SET Status=N'Rejected', Reason=CONCAT(Reason,N' | Rejected: ',@Reason), UpdatedByUserId=@UpdatedByUserId, UpdatedDate=SYSUTCDATETIME() WHERE SalesReturnHeaderId=@SalesReturnHeaderId AND Status IN (N'Draft',N'Approved'); END;

