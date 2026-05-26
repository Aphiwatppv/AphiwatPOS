CREATE PROCEDURE [dbo].[spCustomerCreditCheckEligibility] @CustomerId INT,@SaleAmount DECIMAL(18,2) AS
BEGIN
    SELECT CONVERT(BIT,CASE WHEN c.IsActive=1 AND cc.AllowCredit=1 AND cc.CreditStatus=N'Good' AND cc.CreditLimit>0 AND @SaleAmount<=cc.AvailableCredit AND NOT EXISTS(SELECT 1 FROM dbo.CustomerCreditTransaction t WHERE t.CustomerId=@CustomerId AND t.Status IN(N'Overdue') OR (t.CustomerId=@CustomerId AND t.Status IN(N'Unpaid',N'PartiallyPaid') AND t.DueDate<CONVERT(date,SYSDATETIME()))) THEN 1 ELSE 0 END) IsAllowed,
           cc.RequireManagerApproval RequiresManagerApproval,cc.CreditLimit,cc.CurrentOutstandingAmount,cc.AvailableCredit,@SaleAmount RequestedAmount,
           CASE WHEN c.IsActive=0 THEN N'Customer is inactive.' WHEN cc.AllowCredit=0 THEN N'Credit is not allowed.' WHEN cc.CreditStatus<>N'Good' THEN N'Credit status is not good.' WHEN @SaleAmount>cc.AvailableCredit THEN N'Insufficient available credit.' ELSE N'Credit is allowed.' END Message
    FROM dbo.Customer c JOIN dbo.CustomerCredit cc ON cc.CustomerId=c.CustomerId WHERE c.CustomerId=@CustomerId;
END
