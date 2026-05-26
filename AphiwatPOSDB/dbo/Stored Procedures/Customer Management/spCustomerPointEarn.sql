CREATE PROCEDURE [dbo].[spCustomerPointEarn] @CustomerId INT,@SaleAmount DECIMAL(18,2),@ReferenceType NVARCHAR(50)=NULL,@ReferenceId BIGINT=NULL,@ReferenceNo NVARCHAR(100)=NULL,@ExpiryDate DATE=NULL,@CreatedByUserId INT AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @Earned DECIMAL(18,2),@Balance DECIMAL(18,2);
    SELECT @Earned=FLOOR((@SaleAmount/ISNULL(NULLIF(ml.PointEarnAmount,0),100))*ISNULL(ml.PointEarnPoint,1)*ISNULL(ml.PointMultiplier,1)) FROM dbo.Customer c LEFT JOIN dbo.MemberLevel ml ON ml.MemberLevelId=c.MemberLevelId WHERE c.CustomerId=@CustomerId;
    SET @Earned=ISNULL(@Earned,0);
    BEGIN TRAN;
    UPDATE dbo.CustomerPointBalance SET AvailablePoints=AvailablePoints+@Earned,LifetimeEarnedPoints=LifetimeEarnedPoints+@Earned,LastMovementDate=SYSDATETIME(),UpdatedDate=SYSDATETIME() WHERE CustomerId=@CustomerId;
    SELECT @Balance=AvailablePoints FROM dbo.CustomerPointBalance WHERE CustomerId=@CustomerId;
    IF @Earned>0 INSERT dbo.CustomerPointMovement(CustomerId,MovementType,PointsIn,BalanceAfter,ReferenceType,ReferenceId,ReferenceNo,ExpiryDate,CreatedByUserId) VALUES(@CustomerId,N'Earn',@Earned,@Balance,@ReferenceType,@ReferenceId,@ReferenceNo,@ExpiryDate,@CreatedByUserId);
    COMMIT; SELECT @Earned;
END
