CREATE PROCEDURE [dbo].[spCustomerHistoryGetTimeline] @CustomerId INT,@DateFrom DATETIME2=NULL,@DateTo DATETIME2=NULL,@HistoryType NVARCHAR(30)=NULL,@PageNumber INT=1,@PageSize INT=20 AS
BEGIN
    ;WITH t AS
    (
        SELECT CreatedDate ActivityDate,N'Credit' HistoryType,CustomerCreditTransactionId ReferenceId,ReferenceNo,TransactionType Title,Remark Description,Amount,CAST(NULL AS DECIMAL(18,2)) Points,Status,CONVERT(NVARCHAR(255),CreatedByUserId) CreatedByName FROM dbo.CustomerCreditTransaction WHERE CustomerId=@CustomerId
        UNION ALL SELECT CreatedDate,N'Point',CustomerPointMovementId,ReferenceNo,MovementType,Remark,NULL,PointsIn-PointsOut,NULL,CONVERT(NVARCHAR(255),CreatedByUserId) FROM dbo.CustomerPointMovement WHERE CustomerId=@CustomerId
        UNION ALL SELECT ChangedDate,N'MemberLevel',CustomerLevelHistoryId,NULL,N'Member level changed',ChangeReason,NULL,NULL,NULL,CONVERT(NVARCHAR(255),ChangedByUserId) FROM dbo.CustomerLevelHistory WHERE CustomerId=@CustomerId
        UNION ALL SELECT CreatedDate,N'Note',CustomerNoteId,NULL,NoteType,NoteText,NULL,NULL,CASE WHEN IsActive=1 THEN N'Active' ELSE N'Inactive' END,CONVERT(NVARCHAR(255),CreatedByUserId) FROM dbo.CustomerNote WHERE CustomerId=@CustomerId
    )
    SELECT *,COUNT(1) OVER() TotalCount FROM t WHERE (@HistoryType IS NULL OR HistoryType=@HistoryType) AND (@DateFrom IS NULL OR ActivityDate>=@DateFrom) AND (@DateTo IS NULL OR ActivityDate<DATEADD(DAY,1,@DateTo)) ORDER BY ActivityDate DESC OFFSET (@PageNumber-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
