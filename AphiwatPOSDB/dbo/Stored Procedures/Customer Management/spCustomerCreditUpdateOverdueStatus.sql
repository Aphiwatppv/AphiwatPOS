CREATE PROCEDURE [dbo].[spCustomerCreditUpdateOverdueStatus] AS
BEGIN UPDATE dbo.CustomerCreditTransaction SET Status=N'Overdue' WHERE Status IN(N'Unpaid',N'PartiallyPaid') AND DueDate<CONVERT(date,SYSDATETIME()); SELECT @@ROWCOUNT; END
