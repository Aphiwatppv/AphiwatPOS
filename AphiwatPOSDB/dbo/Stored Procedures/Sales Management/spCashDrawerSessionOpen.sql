CREATE PROCEDURE [dbo].[spCashDrawerSessionOpen]
    @CashierUserId INT,
    @StartingCash DECIMAL(18,2),
    @OpenedByUserId INT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @StartingCash < 0 THROW 53100, 'Starting cash cannot be negative.', 1;
    IF EXISTS (SELECT 1 FROM [dbo].[CashDrawerSession] WHERE [CashierUserId] = @CashierUserId AND [Status] = N'Open')
        THROW 53101, 'Cashier already has an active cash drawer shift.', 1;

    BEGIN TRANSACTION;
    INSERT INTO [dbo].[CashDrawerSession] ([CashierUserId], [StartingCash], [OpenedByUserId])
    VALUES (@CashierUserId, @StartingCash, @OpenedByUserId);

    DECLARE @SessionId BIGINT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[CashDrawerTransaction] ([SessionId], [TransactionType], [Amount], [Reason], [CreatedByUserId])
    VALUES (@SessionId, N'StartShift', @StartingCash, N'Open shift starting cash', @OpenedByUserId);
    COMMIT TRANSACTION;

    EXEC [dbo].[spCashDrawerSessionGetActive] @CashierUserId = @CashierUserId;
END;
