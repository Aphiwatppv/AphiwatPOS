CREATE PROCEDURE [dbo].[spCashDrawerOpenLogCreate]
    @SessionId BIGINT = NULL,
    @SaleId BIGINT = NULL,
    @OpenType NVARCHAR(30),
    @Reason NVARCHAR(500) = N'',
    @IsSuccess BIT,
    @ErrorMessage NVARCHAR(1000) = N'',
    @OpenedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [dbo].[CashDrawerOpenLog] ([SessionId], [SaleId], [OpenType], [Reason], [IsSuccess], [ErrorMessage], [OpenedByUserId])
    VALUES (@SessionId, @SaleId, @OpenType, ISNULL(@Reason,N''), @IsSuccess, ISNULL(@ErrorMessage,N''), @OpenedByUserId);

    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
END;
