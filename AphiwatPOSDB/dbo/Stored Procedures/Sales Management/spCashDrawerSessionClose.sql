CREATE PROCEDURE [dbo].[spCashDrawerSessionClose]
    @CashierUserId INT,
    @ActualCash DECIMAL(18,2),
    @Note NVARCHAR(1000) = N'',
    @ClosedByUserId INT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @ActualCash < 0 THROW 53102, 'Actual cash cannot be negative.', 1;

    DECLARE @SessionId BIGINT = (SELECT TOP 1 [SessionId] FROM [dbo].[CashDrawerSession] WHERE [CashierUserId] = @CashierUserId AND [Status] = N'Open' ORDER BY [OpenedDate] DESC);
    IF @SessionId IS NULL THROW 53103, 'No active cash drawer session.', 1;

    UPDATE [dbo].[CashDrawerSession]
    SET [ActualCash] = @ActualCash,
        [Difference] = @ActualCash - [ExpectedCash],
        [ClosedByUserId] = @ClosedByUserId,
        [ClosedDate] = SYSUTCDATETIME(),
        [Status] = N'Closed',
        [CloseNote] = ISNULL(@Note, N'')
    WHERE [SessionId] = @SessionId;

    INSERT INTO [dbo].[CashDrawerTransaction] ([SessionId], [TransactionType], [Amount], [Reason], [CreatedByUserId])
    VALUES (@SessionId, N'CloseShift', @ActualCash, N'Close shift actual cash', @ClosedByUserId);

    SELECT TOP 1
        s.[SessionId],
        s.[CashierUserId],
        ISNULL(u.[DisplayName], u.[Username]) AS [CashierName],
        s.[StartingCash],
        s.[CashSales],
        s.[CashIn],
        s.[CashOut],
        s.[CashRefund],
        s.[ExpectedCash],
        s.[ActualCash],
        s.[Difference],
        s.[OpenedByUserId],
        s.[ClosedByUserId],
        s.[ApprovedByUserId],
        s.[OpenedDate],
        s.[ClosedDate],
        s.[Status]
    FROM [dbo].[CashDrawerSession] s
    INNER JOIN [dbo].[AccessUser] u ON u.[UserId] = s.[CashierUserId]
    WHERE s.[SessionId] = @SessionId;
END;
