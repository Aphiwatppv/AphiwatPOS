CREATE PROCEDURE [dbo].[spSalesReturnCreate] @SalesHeaderId BIGINT,@CashierUserId INT,@Reason NVARCHAR(500),@CreatedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.SalesHeader WHERE SalesHeaderId=@SalesHeaderId AND Status IN (N'Completed',N'PartiallyRefunded')) THROW 52300, 'Refund must reference an existing completed sale.', 1;
    DECLARE @ReturnNo NVARCHAR(50)=CONCAT(N'RET',FORMAT(SYSUTCDATETIME(),'yyyyMMddHHmmssfff'));
    INSERT dbo.SalesReturnHeader (ReturnNo, SalesHeaderId, CustomerId, CashierUserId, Reason, Status, CreatedByUserId) SELECT @ReturnNo, SalesHeaderId, CustomerId, @CashierUserId, @Reason, N'Draft', @CreatedByUserId FROM dbo.SalesHeader WHERE SalesHeaderId=@SalesHeaderId;
    SELECT CONVERT(BIGINT,SCOPE_IDENTITY());
END;

