CREATE PROCEDURE [dbo].[spMemberLevelUpdate]
    @MemberLevelId INT,@LevelCode NVARCHAR(50),@LevelName NVARCHAR(255),@Description NVARCHAR(500)=NULL,@MinSpendingAmount DECIMAL(18,2)=0,@DiscountPercent DECIMAL(9,2)=0,@PointEarnAmount DECIMAL(18,2)=100,@PointEarnPoint DECIMAL(18,2)=1,@PointMultiplier DECIMAL(9,2)=1,@AllowCredit BIT=0,@DefaultCreditLimit DECIMAL(18,2)=0,@DefaultCreditTermDays INT=0,@RequireManagerApprovalForCredit BIT=0,@MaxOverdueDaysAllowed INT=0,@DisplayOrder INT=0,@IsActive BIT=1,@UpdatedByUserId INT
AS
BEGIN
    IF EXISTS(SELECT 1 FROM dbo.MemberLevel WHERE LevelCode=@LevelCode AND MemberLevelId<>@MemberLevelId) THROW 51101,'Level code already exists.',1;
    IF @AllowCredit=0 SELECT @DefaultCreditLimit=0,@DefaultCreditTermDays=0;
    UPDATE dbo.MemberLevel SET LevelCode=@LevelCode,LevelName=@LevelName,Description=@Description,MinSpendingAmount=@MinSpendingAmount,DiscountPercent=@DiscountPercent,PointEarnAmount=@PointEarnAmount,PointEarnPoint=@PointEarnPoint,PointMultiplier=@PointMultiplier,AllowCredit=@AllowCredit,DefaultCreditLimit=@DefaultCreditLimit,DefaultCreditTermDays=@DefaultCreditTermDays,RequireManagerApprovalForCredit=@RequireManagerApprovalForCredit,MaxOverdueDaysAllowed=@MaxOverdueDaysAllowed,DisplayOrder=@DisplayOrder,IsActive=@IsActive,UpdatedDate=SYSDATETIME(),UpdatedByUserId=@UpdatedByUserId WHERE MemberLevelId=@MemberLevelId;
END
