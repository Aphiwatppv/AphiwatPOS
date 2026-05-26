CREATE PROCEDURE [dbo].[spCustomerGetPaged]
    @PageNumber INT, @PageSize INT, @SearchText NVARCHAR(255)=NULL, @MemberLevelId INT=NULL, @IsActive BIT=NULL, @CreditStatus NVARCHAR(30)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH q AS
    (
        SELECT c.CustomerId,c.CustomerCode,c.CustomerName,c.PhoneNumber,c.Email,ml.LevelName AS MemberLevelName,
               ISNULL(pb.AvailablePoints,0) AvailablePoints,ISNULL(cc.CreditLimit,0) CreditLimit,
               ISNULL(cc.CurrentOutstandingAmount,0) CurrentOutstandingAmount,ISNULL(cc.AvailableCredit,0) AvailableCredit,
               ISNULL(cc.CreditStatus,N'Good') CreditStatus,c.TotalSpending,c.TotalPurchaseCount,c.LastPurchaseDate,c.IsActive,
               COUNT(1) OVER() TotalCount
        FROM dbo.Customer c
        LEFT JOIN dbo.MemberLevel ml ON ml.MemberLevelId=c.MemberLevelId
        LEFT JOIN dbo.CustomerPointBalance pb ON pb.CustomerId=c.CustomerId
        LEFT JOIN dbo.CustomerCredit cc ON cc.CustomerId=c.CustomerId
        WHERE (@SearchText IS NULL OR c.CustomerCode LIKE N'%'+@SearchText+N'%' OR c.CustomerName LIKE N'%'+@SearchText+N'%' OR c.PhoneNumber LIKE N'%'+@SearchText+N'%' OR c.Email LIKE N'%'+@SearchText+N'%')
          AND (@MemberLevelId IS NULL OR c.MemberLevelId=@MemberLevelId)
          AND (@IsActive IS NULL OR c.IsActive=@IsActive)
          AND (@CreditStatus IS NULL OR cc.CreditStatus=@CreditStatus)
    )
    SELECT * FROM q ORDER BY CustomerName OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
