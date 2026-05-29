CREATE PROCEDURE [dbo].[spCashDrawerSessionGetActive]
    @CashierUserId INT
AS
BEGIN
    SET NOCOUNT ON;
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
    WHERE s.[CashierUserId] = @CashierUserId
      AND s.[Status] = N'Open'
    ORDER BY s.[OpenedDate] DESC;
END;
