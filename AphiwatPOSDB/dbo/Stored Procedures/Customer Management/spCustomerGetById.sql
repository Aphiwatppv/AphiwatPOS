CREATE PROCEDURE [dbo].[spCustomerGetById] @CustomerId INT AS
BEGIN
    SET NOCOUNT ON;

    ;WITH type_agg AS
    (
        SELECT
            cmt.CustomerId,
            STRING_AGG(mt.MemberTypeCode, N',') WITHIN GROUP (ORDER BY mt.MemberTypeCode) AS ActiveMemberTypeCodes
        FROM dbo.CustomerMemberType cmt
        JOIN dbo.MemberType mt ON mt.MemberTypeId = cmt.MemberTypeId
        WHERE cmt.IsActive = 1 AND cmt.CustomerId = @CustomerId
        GROUP BY cmt.CustomerId
    )
    SELECT c.*,
           ISNULL(ta.ActiveMemberTypeCodes, CASE WHEN c.MemberType = N'Wholesale' THEN N'WHOLESALE' ELSE N'RETAIL' END) ActiveMemberTypeCodes,
           ml.LevelCode AS MemberLevelCode,ml.LevelName AS MemberLevelName,ISNULL(ml.DiscountPercent,0) DiscountPercent,
           ISNULL(pb.AvailablePoints,0) AvailablePoints,ISNULL(pb.LifetimeEarnedPoints,0) LifetimeEarnedPoints,ISNULL(pb.LifetimeRedeemedPoints,0) LifetimeRedeemedPoints,
           ISNULL(cc.AllowCredit,0) AllowCredit,ISNULL(cc.CreditLimit,0) CreditLimit,ISNULL(cc.CreditTermDays,0) CreditTermDays,
           ISNULL(cc.CurrentOutstandingAmount,0) CurrentOutstandingAmount,ISNULL(cc.AvailableCredit,0) AvailableCredit,ISNULL(cc.CreditStatus,N'Good') CreditStatus,
           ISNULL(cc.RequireManagerApproval,0) RequireManagerApproval,
           wp.BusinessName WholesaleBusinessName,ISNULL(wp.IsApproved,0) WholesaleApproved,ISNULL(wp.PaymentTermDays,0) WholesalePaymentTermDays,
           rp.SupplierCode RubberSupplierCode,
           ISNULL(mla.RubberWeightCarryForwardKg,0) RubberWeightCarryForwardKg,ISNULL(mla.PointBalance,0) RubberLoyaltyPointBalance
    FROM dbo.Customer c
    LEFT JOIN type_agg ta ON ta.CustomerId=c.CustomerId
    LEFT JOIN dbo.MemberLevel ml ON ml.MemberLevelId=c.MemberLevelId
    LEFT JOIN dbo.CustomerPointBalance pb ON pb.CustomerId=c.CustomerId
    LEFT JOIN dbo.CustomerCredit cc ON cc.CustomerId=c.CustomerId
    LEFT JOIN dbo.WholesaleMemberProfile wp ON wp.CustomerId=c.CustomerId
    LEFT JOIN dbo.RubberSupplierMemberProfile rp ON rp.CustomerId=c.CustomerId
    LEFT JOIN dbo.MemberLoyaltyAccount mla ON mla.CustomerId=c.CustomerId
    WHERE c.CustomerId=@CustomerId;
END
