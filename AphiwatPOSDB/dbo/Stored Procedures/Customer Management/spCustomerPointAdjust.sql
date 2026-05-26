CREATE PROCEDURE [dbo].[spCustomerPointAdjust] @CustomerId INT,@AdjustmentType NVARCHAR(30),@Points DECIMAL(18,2),@ReferenceType NVARCHAR(50)=NULL,@ReferenceId BIGINT=NULL,@ReferenceNo NVARCHAR(100)=NULL,@Remark NVARCHAR(1000)=NULL,@CreatedByUserId INT AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @Balance DECIMAL(18,2);
    BEGIN TRAN;
    SELECT @Balance=AvailablePoints FROM dbo.CustomerPointBalance WITH(UPDLOCK,ROWLOCK) WHERE CustomerId=@CustomerId;
    IF @AdjustmentType IN(N'AdjustOut',N'Redeem',N'Expire',N'Reverse') AND @Balance<@Points THROW 51300,'Point balance cannot be negative.',1;
    UPDATE dbo.CustomerPointBalance SET AvailablePoints=AvailablePoints+CASE WHEN @AdjustmentType=N'AdjustIn' THEN @Points ELSE -@Points END,LifetimeRedeemedPoints=LifetimeRedeemedPoints+CASE WHEN @AdjustmentType=N'AdjustOut' THEN @Points ELSE 0 END,LastMovementDate=SYSDATETIME(),UpdatedDate=SYSDATETIME() WHERE CustomerId=@CustomerId;
    SELECT @Balance=AvailablePoints FROM dbo.CustomerPointBalance WHERE CustomerId=@CustomerId;
    INSERT dbo.CustomerPointMovement(CustomerId,MovementType,PointsIn,PointsOut,BalanceAfter,ReferenceType,ReferenceId,ReferenceNo,Remark,CreatedByUserId)
    VALUES(@CustomerId,@AdjustmentType,CASE WHEN @AdjustmentType=N'AdjustIn' THEN @Points ELSE 0 END,CASE WHEN @AdjustmentType<>N'AdjustIn' THEN @Points ELSE 0 END,@Balance,@ReferenceType,@ReferenceId,@ReferenceNo,@Remark,@CreatedByUserId);
    COMMIT;
END
