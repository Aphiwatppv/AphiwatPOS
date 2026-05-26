CREATE PROCEDURE [dbo].[spCustomerPointRedeem] @CustomerId INT,@Points DECIMAL(18,2),@ReferenceType NVARCHAR(50)=NULL,@ReferenceId BIGINT=NULL,@ReferenceNo NVARCHAR(100)=NULL,@Remark NVARCHAR(1000)=NULL,@CreatedByUserId INT AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @Balance DECIMAL(18,2);
    BEGIN TRAN;
    SELECT @Balance=AvailablePoints FROM dbo.CustomerPointBalance WITH(UPDLOCK,ROWLOCK) WHERE CustomerId=@CustomerId;
    IF @Balance<@Points THROW 51301,'Redeem points exceed available balance.',1;
    UPDATE dbo.CustomerPointBalance SET AvailablePoints=AvailablePoints-@Points,LifetimeRedeemedPoints=LifetimeRedeemedPoints+@Points,LastMovementDate=SYSDATETIME(),UpdatedDate=SYSDATETIME() WHERE CustomerId=@CustomerId;
    SELECT @Balance=AvailablePoints FROM dbo.CustomerPointBalance WHERE CustomerId=@CustomerId;
    INSERT dbo.CustomerPointMovement(CustomerId,MovementType,PointsOut,BalanceAfter,ReferenceType,ReferenceId,ReferenceNo,Remark,CreatedByUserId)
    VALUES(@CustomerId,N'Redeem',@Points,@Balance,@ReferenceType,@ReferenceId,@ReferenceNo,@Remark,@CreatedByUserId);
    COMMIT;
END
