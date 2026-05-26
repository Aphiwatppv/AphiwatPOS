CREATE PROCEDURE [dbo].[spCustomerGetById] @CustomerId INT AS
BEGIN
    SET NOCOUNT ON;
    SELECT c.*,ml.LevelCode AS MemberLevelCode,ml.LevelName AS MemberLevelName,ISNULL(ml.DiscountPercent,0) DiscountPercent,
           ISNULL(pb.AvailablePoints,0) AvailablePoints,ISNULL(pb.LifetimeEarnedPoints,0) LifetimeEarnedPoints,ISNULL(pb.LifetimeRedeemedPoints,0) LifetimeRedeemedPoints,
           ISNULL(cc.AllowCredit,0) AllowCredit,ISNULL(cc.CreditLimit,0) CreditLimit,ISNULL(cc.CreditTermDays,0) CreditTermDays,
           ISNULL(cc.CurrentOutstandingAmount,0) CurrentOutstandingAmount,ISNULL(cc.AvailableCredit,0) AvailableCredit,ISNULL(cc.CreditStatus,N'Good') CreditStatus,
           ISNULL(cc.RequireManagerApproval,0) RequireManagerApproval
    FROM dbo.Customer c
    LEFT JOIN dbo.MemberLevel ml ON ml.MemberLevelId=c.MemberLevelId
    LEFT JOIN dbo.CustomerPointBalance pb ON pb.CustomerId=c.CustomerId
    LEFT JOIN dbo.CustomerCredit cc ON cc.CustomerId=c.CustomerId
    WHERE c.CustomerId=@CustomerId;
END
