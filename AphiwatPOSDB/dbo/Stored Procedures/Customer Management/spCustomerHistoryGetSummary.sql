CREATE PROCEDURE [dbo].[spCustomerHistoryGetSummary] @CustomerId INT,@DateFrom DATETIME2=NULL,@DateTo DATETIME2=NULL AS
BEGIN
    SELECT c.CustomerId,c.CustomerCode,c.CustomerName,c.PhoneNumber,ml.LevelName MemberLevelName,c.TotalSpending,c.TotalPurchaseCount,c.LastPurchaseDate,
           ISNULL(pb.AvailablePoints,0) AvailablePoints,ISNULL(pb.LifetimeEarnedPoints,0) LifetimeEarnedPoints,ISNULL(pb.LifetimeRedeemedPoints,0) LifetimeRedeemedPoints,
           ISNULL(cc.CreditLimit,0) CreditLimit,ISNULL(cc.CurrentOutstandingAmount,0) CurrentOutstandingAmount,ISNULL(cc.AvailableCredit,0) AvailableCredit,ISNULL(cc.CreditStatus,N'Good') CreditStatus,
           ISNULL(SUM(CASE WHEN ct.TransactionType=N'CreditSale' THEN ct.Amount ELSE 0 END),0) TotalCreditSales,
           ISNULL(SUM(CASE WHEN ct.TransactionType=N'Payment' THEN ct.Amount ELSE 0 END),0) TotalCreditPayments,
           ISNULL(SUM(CASE WHEN ct.Status=N'Overdue' THEN ct.Amount ELSE 0 END),0) OverdueAmount,
           SUM(CASE WHEN ct.Status=N'Overdue' THEN 1 ELSE 0 END) OverdueCount,
           (SELECT COUNT(1) FROM dbo.CustomerNote n WHERE n.CustomerId=@CustomerId AND n.IsActive=1 AND n.IsImportant=1) ImportantNoteCount
    FROM dbo.Customer c LEFT JOIN dbo.MemberLevel ml ON ml.MemberLevelId=c.MemberLevelId LEFT JOIN dbo.CustomerPointBalance pb ON pb.CustomerId=c.CustomerId LEFT JOIN dbo.CustomerCredit cc ON cc.CustomerId=c.CustomerId LEFT JOIN dbo.CustomerCreditTransaction ct ON ct.CustomerId=c.CustomerId AND (@DateFrom IS NULL OR ct.CreatedDate>=@DateFrom) AND (@DateTo IS NULL OR ct.CreatedDate<DATEADD(DAY,1,@DateTo))
    WHERE c.CustomerId=@CustomerId GROUP BY c.CustomerId,c.CustomerCode,c.CustomerName,c.PhoneNumber,ml.LevelName,c.TotalSpending,c.TotalPurchaseCount,c.LastPurchaseDate,pb.AvailablePoints,pb.LifetimeEarnedPoints,pb.LifetimeRedeemedPoints,cc.CreditLimit,cc.CurrentOutstandingAmount,cc.AvailableCredit,cc.CreditStatus;
END
