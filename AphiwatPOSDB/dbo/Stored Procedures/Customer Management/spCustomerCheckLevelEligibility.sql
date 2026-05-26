CREATE PROCEDURE [dbo].[spCustomerCheckLevelEligibility] @CustomerId INT AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @CurrentLevelId INT,@CreatedDate DATETIME2,@Spending DECIMAL(18,2),@Purchases INT;
    SELECT @CurrentLevelId=MemberLevelId,@CreatedDate=CreatedDate,@Spending=TotalSpending,@Purchases=TotalPurchaseCount FROM dbo.Customer WHERE CustomerId=@CustomerId;
    DECLARE @HasOverdue BIT=CASE WHEN EXISTS(SELECT 1 FROM dbo.CustomerCreditTransaction WHERE CustomerId=@CustomerId AND Status IN(N'Overdue') OR (CustomerId=@CustomerId AND Status IN(N'Unpaid',N'PartiallyPaid') AND DueDate<CONVERT(date,SYSDATETIME()))) THEN 1 ELSE 0 END;
    SELECT TOP(1)
        CONVERT(BIT,CASE WHEN @Spending>=r.RequiredTotalSpending AND @Purchases>=r.RequiredPurchaseCount AND DATEDIFF(DAY,@CreatedDate,SYSDATETIME())>=r.RequiredMembershipDays AND (r.RequireNoOverduePayment=0 OR @HasOverdue=0) THEN 1 ELSE 0 END) IsEligible,
        @CustomerId CustomerId,@CurrentLevelId CurrentMemberLevelId,cl.LevelName CurrentMemberLevelName,r.ToMemberLevelId NextMemberLevelId,nl.LevelName NextMemberLevelName,
        r.RequiredTotalSpending,@Spending CurrentTotalSpending,CASE WHEN r.RequiredTotalSpending>@Spending THEN r.RequiredTotalSpending-@Spending ELSE 0 END MissingSpendingAmount,
        r.RequiredPurchaseCount,@Purchases CurrentPurchaseCount,CASE WHEN r.RequiredPurchaseCount>@Purchases THEN r.RequiredPurchaseCount-@Purchases ELSE 0 END MissingPurchaseCount,
        r.RequiredMembershipDays,DATEDIFF(DAY,@CreatedDate,SYSDATETIME()) CurrentMembershipDays,@HasOverdue HasOverdueCredit,r.RequireManagerApproval,
        CASE WHEN r.MemberLevelUpgradeRuleId IS NULL THEN N'No active upgrade rule.' WHEN r.RequireManagerApproval=1 THEN N'Eligible; manager approval required.' ELSE N'Eligibility checked.' END Message
    FROM dbo.MemberLevelUpgradeRule r
    JOIN dbo.MemberLevel nl ON nl.MemberLevelId=r.ToMemberLevelId
    LEFT JOIN dbo.MemberLevel cl ON cl.MemberLevelId=@CurrentLevelId
    WHERE r.FromMemberLevelId=@CurrentLevelId AND r.IsActive=1;
END
