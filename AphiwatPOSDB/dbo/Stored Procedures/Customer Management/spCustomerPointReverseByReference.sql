CREATE PROCEDURE [dbo].[spCustomerPointReverseByReference] @ReferenceType NVARCHAR(50),@ReferenceId BIGINT,@Remark NVARCHAR(1000)=NULL,@CreatedByUserId INT AS
BEGIN
    DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT CustomerId,CASE WHEN PointsIn>0 THEN PointsIn ELSE PointsOut END FROM dbo.CustomerPointMovement WHERE ReferenceType=@ReferenceType AND ReferenceId=@ReferenceId AND MovementType<>N'Reverse';
    DECLARE @CustomerId INT,@Points DECIMAL(18,2); OPEN c; FETCH NEXT FROM c INTO @CustomerId,@Points;
    WHILE @@FETCH_STATUS=0 BEGIN EXEC dbo.spCustomerPointAdjust @CustomerId,N'Reverse',@Points,@ReferenceType,@ReferenceId,NULL,@Remark,@CreatedByUserId; FETCH NEXT FROM c INTO @CustomerId,@Points; END
    CLOSE c; DEALLOCATE c;
END
