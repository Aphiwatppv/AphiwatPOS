CREATE PROCEDURE [dbo].[spMemberLevelCreate]
    @LevelCode NVARCHAR(50),@LevelName NVARCHAR(255),@Description NVARCHAR(500)=NULL,@MinSpendingAmount DECIMAL(18,2)=0,@DiscountPercent DECIMAL(9,2)=0,@PointEarnAmount DECIMAL(18,2)=100,@PointEarnPoint DECIMAL(18,2)=1,@PointMultiplier DECIMAL(9,2)=1,@AllowCredit BIT=0,@DefaultCreditLimit DECIMAL(18,2)=0,@DefaultCreditTermDays INT=0,@RequireManagerApprovalForCredit BIT=0,@MaxOverdueDaysAllowed INT=0,@DisplayOrder INT=0,@CreatedByUserId INT
AS
BEGIN
    IF EXISTS(SELECT 1 FROM dbo.MemberLevel WHERE LevelCode=@LevelCode) THROW 51100,'Level code already exists.',1;
    IF @AllowCredit=0 SELECT @DefaultCreditLimit=0,@DefaultCreditTermDays=0;
    INSERT dbo.MemberLevel(LevelCode,LevelName,Description,MinSpendingAmount,DiscountPercent,PointEarnAmount,PointEarnPoint,PointMultiplier,AllowCredit,DefaultCreditLimit,DefaultCreditTermDays,RequireManagerApprovalForCredit,MaxOverdueDaysAllowed,DisplayOrder,CreatedByUserId)
    VALUES(@LevelCode,@LevelName,@Description,@MinSpendingAmount,@DiscountPercent,@PointEarnAmount,@PointEarnPoint,@PointMultiplier,@AllowCredit,@DefaultCreditLimit,@DefaultCreditTermDays,@RequireManagerApprovalForCredit,@MaxOverdueDaysAllowed,@DisplayOrder,@CreatedByUserId);
    SELECT CONVERT(INT,SCOPE_IDENTITY());
END
