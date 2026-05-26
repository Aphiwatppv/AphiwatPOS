CREATE PROCEDURE [dbo].[spSalesReturnApprove] @SalesReturnHeaderId BIGINT,@ApprovedByUserId INT AS BEGIN SET NOCOUNT ON; UPDATE dbo.SalesReturnHeader SET Status=N'Approved', ApprovedByUserId=@ApprovedByUserId, ApprovedDate=SYSUTCDATETIME(), UpdatedByUserId=@ApprovedByUserId, UpdatedDate=SYSUTCDATETIME() WHERE SalesReturnHeaderId=@SalesReturnHeaderId AND Status=N'Draft'; END;

