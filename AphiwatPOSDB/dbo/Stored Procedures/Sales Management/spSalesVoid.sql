CREATE PROCEDURE [dbo].[spSalesVoid] @SalesHeaderId BIGINT, @Reason NVARCHAR(500), @UpdatedByUserId INT, @ReverseInventory BIT=1 AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF NULLIF(LTRIM(RTRIM(@Reason)),N'') IS NULL THROW 52120, 'Void reason is required.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.SalesHeader WHERE SalesHeaderId=@SalesHeaderId AND Status=N'Completed') THROW 52121, 'Only completed sales can be voided.', 1;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @SaleNo NVARCHAR(50) = (SELECT SaleNo FROM dbo.SalesHeader WHERE SalesHeaderId=@SalesHeaderId);
        UPDATE dbo.SalesHeader SET Status=N'Voided', Remark=CONCAT(Remark, CASE WHEN Remark=N'' THEN N'' ELSE N' | ' END, N'Voided: ', @Reason), UpdatedByUserId=@UpdatedByUserId, UpdatedDate=SYSUTCDATETIME() WHERE SalesHeaderId=@SalesHeaderId;
        IF @ReverseInventory=1
        BEGIN
            DECLARE @ProductId INT, @LocationId INT, @Qty DECIMAL(18,4), @Cost DECIMAL(18,4);
            DECLARE v CURSOR LOCAL FAST_FORWARD FOR SELECT m.ProductId, m.LocationId, m.Quantity, m.UnitCost FROM dbo.InventoryMovement m WHERE m.ReferenceType=N'Sale' AND m.ReferenceId=@SalesHeaderId AND m.MovementType=N'Sale';
            OPEN v; FETCH NEXT FROM v INTO @ProductId,@LocationId,@Qty,@Cost;
            WHILE @@FETCH_STATUS=0 BEGIN EXEC dbo.spInventoryMovementCreate @ProductId=@ProductId,@LocationId=@LocationId,@MovementType=N'Return',@Quantity=@Qty,@UnitCost=@Cost,@ReferenceType=N'SaleVoid',@ReferenceId=@SalesHeaderId,@ReferenceNo=@SaleNo,@Reason=@Reason,@AllowNegativeStock=1,@CreatedByUserId=@UpdatedByUserId; FETCH NEXT FROM v INTO @ProductId,@LocationId,@Qty,@Cost; END
            CLOSE v; DEALLOCATE v;
        END
        COMMIT TRANSACTION;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH
END;

