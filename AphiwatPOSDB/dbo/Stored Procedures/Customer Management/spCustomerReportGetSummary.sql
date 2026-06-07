CREATE PROCEDURE [dbo].[spCustomerReportGetSummary]
    @DateFrom DATETIME2=NULL,
    @DateTo DATETIME2=NULL,
    @MemberType NVARCHAR(30)=NULL,
    @MemberLevelId INT=NULL,
    @IsActive BIT=NULL,
    @Top INT=20,
    @NoPurchaseAfterDate DATETIME2=NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(1) TotalCustomers,
        SUM(CASE WHEN c.IsActive=1 THEN 1 ELSE 0 END) ActiveCustomers,
        SUM(CASE WHEN c.MemberType=N'Retail' THEN 1 ELSE 0 END) RetailMemberCount,
        SUM(CASE WHEN c.MemberType=N'Wholesale' THEN 1 ELSE 0 END) WholesaleMemberCount,
        SUM(CASE WHEN (@DateFrom IS NOT NULL AND c.CreatedDate>=@DateFrom AND (@DateTo IS NULL OR c.CreatedDate<DATEADD(DAY,1,@DateTo))) THEN 1 ELSE 0 END) NewCustomers,
        SUM(c.TotalSpending) TotalCustomerSpending,
        ISNULL(SUM(cc.CurrentOutstandingAmount),0) TotalOutstandingCredit,
        ISNULL(SUM(pb.AvailablePoints),0) TotalAvailablePoints,
        SUM(CASE WHEN cc.AllowCredit=1 THEN 1 ELSE 0 END) TotalCreditCustomers,
        SUM(CASE WHEN overdue.CustomerId IS NULL THEN 0 ELSE 1 END) TotalOverdueCustomers
    FROM dbo.Customer c
    LEFT JOIN dbo.CustomerCredit cc ON cc.CustomerId=c.CustomerId
    LEFT JOIN dbo.CustomerPointBalance pb ON pb.CustomerId=c.CustomerId
    LEFT JOIN (SELECT DISTINCT CustomerId FROM dbo.CustomerCreditTransaction WHERE Status=N'Overdue') overdue ON overdue.CustomerId=c.CustomerId
    WHERE (@MemberType IS NULL OR c.MemberType=@MemberType)
      AND (@MemberLevelId IS NULL OR c.MemberLevelId=@MemberLevelId)
      AND (@IsActive IS NULL OR c.IsActive=@IsActive);
END
