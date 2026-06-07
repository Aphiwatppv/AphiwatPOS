CREATE PROCEDURE [dbo].[spCustomerAuditLogGetPaged]
    @CustomerId INT = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @ActionType NVARCHAR(50) = NULL,
    @DateFrom DATETIME2 = NULL,
    @DateTo DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CustomerAuditLogId,
        CustomerId,
        ActionType,
        EntityName,
        EntityId,
        OldValue,
        NewValue,
        Remark,
        CreatedDate,
        CreatedByUserId,
        COUNT(1) OVER() AS TotalCount
    FROM dbo.CustomerAuditLog
    WHERE (@CustomerId IS NULL OR CustomerId = @CustomerId)
      AND (@ActionType IS NULL OR ActionType = @ActionType)
      AND (@DateFrom IS NULL OR CreatedDate >= @DateFrom)
      AND (@DateTo IS NULL OR CreatedDate < DATEADD(DAY, 1, @DateTo))
    ORDER BY CreatedDate DESC, CustomerAuditLogId DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
