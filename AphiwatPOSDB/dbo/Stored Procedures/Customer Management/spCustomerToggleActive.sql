CREATE PROCEDURE [dbo].[spCustomerToggleActive]
    @CustomerId INT,
    @IsActive BIT,
    @UpdatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OldActive BIT=(SELECT IsActive FROM dbo.Customer WHERE CustomerId=@CustomerId);

    UPDATE dbo.Customer
    SET IsActive=@IsActive,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@UpdatedByUserId
    WHERE CustomerId=@CustomerId;

    IF ISNULL(@OldActive,0)<>@IsActive
        INSERT dbo.CustomerAuditLog(CustomerId,ActionType,EntityName,EntityId,OldValue,NewValue,Remark,CreatedByUserId)
        VALUES(@CustomerId,CASE WHEN @IsActive=1 THEN N'CustomerActivated' ELSE N'CustomerDeactivated' END,N'Customer',@CustomerId,CONVERT(NVARCHAR(10),@OldActive),CONVERT(NVARCHAR(10),@IsActive),N'Customer active status changed.',@UpdatedByUserId);
END
