CREATE PROCEDURE [dbo].[spCustomerPointExpire] @ExpiryDate DATE,@CreatedByUserId INT AS
BEGIN
    DECLARE @Count INT=0;
    DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT CustomerId,SUM(PointsIn-PointsOut) FROM dbo.CustomerPointMovement WHERE ExpiryDate<=@ExpiryDate GROUP BY CustomerId HAVING SUM(PointsIn-PointsOut)>0;
    DECLARE @CustomerId INT,@Points DECIMAL(18,2); OPEN c; FETCH NEXT FROM c INTO @CustomerId,@Points;
    WHILE @@FETCH_STATUS=0 BEGIN EXEC dbo.spCustomerPointAdjust @CustomerId,N'Expire',@Points,N'PointExpiry',NULL,NULL,N'Points expired.',@CreatedByUserId; SET @Count+=1; FETCH NEXT FROM c INTO @CustomerId,@Points; END
    CLOSE c; DEALLOCATE c; SELECT @Count;
END
