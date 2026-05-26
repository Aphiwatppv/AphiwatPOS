DECLARE @SeedUserId INT = 1;

MERGE [dbo].[MemberLevel] AS target
USING
(
    VALUES
    (
        N'NORMAL',
        N'Normal',
        N'Default customer level for new customers.',
        CAST(0.00 AS DECIMAL(18,2)),
        CAST(0.00 AS DECIMAL(9,2)),
        CAST(100.00 AS DECIMAL(18,2)),
        CAST(1.00 AS DECIMAL(18,2)),
        CAST(1.00 AS DECIMAL(9,2)),
        CAST(0 AS BIT),
        CAST(0.00 AS DECIMAL(18,2)),
        0,
        CAST(0 AS BIT),
        0,
        10,
        CAST(1 AS BIT)
    ),
    (
        N'SILVER',
        N'Silver',
        N'Entry loyalty tier for returning customers.',
        CAST(5000.00 AS DECIMAL(18,2)),
        CAST(2.00 AS DECIMAL(9,2)),
        CAST(100.00 AS DECIMAL(18,2)),
        CAST(1.00 AS DECIMAL(18,2)),
        CAST(1.10 AS DECIMAL(9,2)),
        CAST(0 AS BIT),
        CAST(0.00 AS DECIMAL(18,2)),
        0,
        CAST(0 AS BIT),
        0,
        20,
        CAST(1 AS BIT)
    ),
    (
        N'GOLD',
        N'Gold',
        N'Higher value customers with stronger benefits.',
        CAST(20000.00 AS DECIMAL(18,2)),
        CAST(5.00 AS DECIMAL(9,2)),
        CAST(100.00 AS DECIMAL(18,2)),
        CAST(1.00 AS DECIMAL(18,2)),
        CAST(1.25 AS DECIMAL(9,2)),
        CAST(1 AS BIT),
        CAST(10000.00 AS DECIMAL(18,2)),
        15,
        CAST(0 AS BIT),
        7,
        30,
        CAST(1 AS BIT)
    ),
    (
        N'PLATINUM',
        N'Platinum',
        N'Premium tier with better discounts, points, and credit.',
        CAST(50000.00 AS DECIMAL(18,2)),
        CAST(8.00 AS DECIMAL(9,2)),
        CAST(100.00 AS DECIMAL(18,2)),
        CAST(1.00 AS DECIMAL(18,2)),
        CAST(1.50 AS DECIMAL(9,2)),
        CAST(1 AS BIT),
        CAST(30000.00 AS DECIMAL(18,2)),
        30,
        CAST(1 AS BIT),
        7,
        40,
        CAST(1 AS BIT)
    ),
    (
        N'VIP',
        N'VIP',
        N'Top tier for strategic and high-value customers.',
        CAST(100000.00 AS DECIMAL(18,2)),
        CAST(12.00 AS DECIMAL(9,2)),
        CAST(100.00 AS DECIMAL(18,2)),
        CAST(2.00 AS DECIMAL(18,2)),
        CAST(2.00 AS DECIMAL(9,2)),
        CAST(1 AS BIT),
        CAST(100000.00 AS DECIMAL(18,2)),
        45,
        CAST(1 AS BIT),
        14,
        50,
        CAST(1 AS BIT)
    )
) AS source
(
    LevelCode,
    LevelName,
    Description,
    MinSpendingAmount,
    DiscountPercent,
    PointEarnAmount,
    PointEarnPoint,
    PointMultiplier,
    AllowCredit,
    DefaultCreditLimit,
    DefaultCreditTermDays,
    RequireManagerApprovalForCredit,
    MaxOverdueDaysAllowed,
    DisplayOrder,
    IsActive
)
ON target.[LevelCode] = source.[LevelCode]
WHEN MATCHED THEN
    UPDATE SET
        [LevelName] = source.[LevelName],
        [Description] = source.[Description],
        [MinSpendingAmount] = source.[MinSpendingAmount],
        [DiscountPercent] = source.[DiscountPercent],
        [PointEarnAmount] = source.[PointEarnAmount],
        [PointEarnPoint] = source.[PointEarnPoint],
        [PointMultiplier] = source.[PointMultiplier],
        [AllowCredit] = source.[AllowCredit],
        [DefaultCreditLimit] = source.[DefaultCreditLimit],
        [DefaultCreditTermDays] = source.[DefaultCreditTermDays],
        [RequireManagerApprovalForCredit] = source.[RequireManagerApprovalForCredit],
        [MaxOverdueDaysAllowed] = source.[MaxOverdueDaysAllowed],
        [DisplayOrder] = source.[DisplayOrder],
        [IsActive] = source.[IsActive],
        [UpdatedDate] = SYSDATETIME(),
        [UpdatedByUserId] = @SeedUserId
WHEN NOT MATCHED BY TARGET THEN
    INSERT
    (
        [LevelCode],
        [LevelName],
        [Description],
        [MinSpendingAmount],
        [DiscountPercent],
        [PointEarnAmount],
        [PointEarnPoint],
        [PointMultiplier],
        [AllowCredit],
        [DefaultCreditLimit],
        [DefaultCreditTermDays],
        [RequireManagerApprovalForCredit],
        [MaxOverdueDaysAllowed],
        [DisplayOrder],
        [IsActive],
        [CreatedByUserId]
    )
    VALUES
    (
        source.[LevelCode],
        source.[LevelName],
        source.[Description],
        source.[MinSpendingAmount],
        source.[DiscountPercent],
        source.[PointEarnAmount],
        source.[PointEarnPoint],
        source.[PointMultiplier],
        source.[AllowCredit],
        source.[DefaultCreditLimit],
        source.[DefaultCreditTermDays],
        source.[RequireManagerApprovalForCredit],
        source.[MaxOverdueDaysAllowed],
        source.[DisplayOrder],
        source.[IsActive],
        @SeedUserId
    );

DECLARE @NormalId INT = (SELECT [MemberLevelId] FROM [dbo].[MemberLevel] WHERE [LevelCode] = N'NORMAL');
DECLARE @SilverId INT = (SELECT [MemberLevelId] FROM [dbo].[MemberLevel] WHERE [LevelCode] = N'SILVER');
DECLARE @GoldId INT = (SELECT [MemberLevelId] FROM [dbo].[MemberLevel] WHERE [LevelCode] = N'GOLD');
DECLARE @PlatinumId INT = (SELECT [MemberLevelId] FROM [dbo].[MemberLevel] WHERE [LevelCode] = N'PLATINUM');
DECLARE @VipId INT = (SELECT [MemberLevelId] FROM [dbo].[MemberLevel] WHERE [LevelCode] = N'VIP');

IF @NormalId IS NOT NULL AND @SilverId IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM [dbo].[MemberLevelUpgradeRule] WHERE [FromMemberLevelId] = @NormalId)
        UPDATE [dbo].[MemberLevelUpgradeRule]
        SET [ToMemberLevelId] = @SilverId,
            [RequiredTotalSpending] = 5000.00,
            [RequiredPurchaseCount] = 5,
            [RequiredMembershipDays] = 0,
            [RequireNoOverduePayment] = 1,
            [RequireManagerApproval] = 0,
            [IsActive] = 1,
            [UpdatedDate] = SYSDATETIME(),
            [UpdatedByUserId] = @SeedUserId
        WHERE [FromMemberLevelId] = @NormalId;
    ELSE
        INSERT INTO [dbo].[MemberLevelUpgradeRule] ([FromMemberLevelId], [ToMemberLevelId], [RequiredTotalSpending], [RequiredPurchaseCount], [RequiredMembershipDays], [RequireNoOverduePayment], [RequireManagerApproval], [IsActive], [CreatedByUserId])
        VALUES (@NormalId, @SilverId, 5000.00, 5, 0, 1, 0, 1, @SeedUserId);
END;

IF @SilverId IS NOT NULL AND @GoldId IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM [dbo].[MemberLevelUpgradeRule] WHERE [FromMemberLevelId] = @SilverId)
        UPDATE [dbo].[MemberLevelUpgradeRule]
        SET [ToMemberLevelId] = @GoldId,
            [RequiredTotalSpending] = 20000.00,
            [RequiredPurchaseCount] = 15,
            [RequiredMembershipDays] = 30,
            [RequireNoOverduePayment] = 1,
            [RequireManagerApproval] = 0,
            [IsActive] = 1,
            [UpdatedDate] = SYSDATETIME(),
            [UpdatedByUserId] = @SeedUserId
        WHERE [FromMemberLevelId] = @SilverId;
    ELSE
        INSERT INTO [dbo].[MemberLevelUpgradeRule] ([FromMemberLevelId], [ToMemberLevelId], [RequiredTotalSpending], [RequiredPurchaseCount], [RequiredMembershipDays], [RequireNoOverduePayment], [RequireManagerApproval], [IsActive], [CreatedByUserId])
        VALUES (@SilverId, @GoldId, 20000.00, 15, 30, 1, 0, 1, @SeedUserId);
END;

IF @GoldId IS NOT NULL AND @PlatinumId IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM [dbo].[MemberLevelUpgradeRule] WHERE [FromMemberLevelId] = @GoldId)
        UPDATE [dbo].[MemberLevelUpgradeRule]
        SET [ToMemberLevelId] = @PlatinumId,
            [RequiredTotalSpending] = 50000.00,
            [RequiredPurchaseCount] = 30,
            [RequiredMembershipDays] = 60,
            [RequireNoOverduePayment] = 1,
            [RequireManagerApproval] = 1,
            [IsActive] = 1,
            [UpdatedDate] = SYSDATETIME(),
            [UpdatedByUserId] = @SeedUserId
        WHERE [FromMemberLevelId] = @GoldId;
    ELSE
        INSERT INTO [dbo].[MemberLevelUpgradeRule] ([FromMemberLevelId], [ToMemberLevelId], [RequiredTotalSpending], [RequiredPurchaseCount], [RequiredMembershipDays], [RequireNoOverduePayment], [RequireManagerApproval], [IsActive], [CreatedByUserId])
        VALUES (@GoldId, @PlatinumId, 50000.00, 30, 60, 1, 1, 1, @SeedUserId);
END;

IF @PlatinumId IS NOT NULL AND @VipId IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM [dbo].[MemberLevelUpgradeRule] WHERE [FromMemberLevelId] = @PlatinumId)
        UPDATE [dbo].[MemberLevelUpgradeRule]
        SET [ToMemberLevelId] = @VipId,
            [RequiredTotalSpending] = 100000.00,
            [RequiredPurchaseCount] = 50,
            [RequiredMembershipDays] = 90,
            [RequireNoOverduePayment] = 1,
            [RequireManagerApproval] = 1,
            [IsActive] = 1,
            [UpdatedDate] = SYSDATETIME(),
            [UpdatedByUserId] = @SeedUserId
        WHERE [FromMemberLevelId] = @PlatinumId;
    ELSE
        INSERT INTO [dbo].[MemberLevelUpgradeRule] ([FromMemberLevelId], [ToMemberLevelId], [RequiredTotalSpending], [RequiredPurchaseCount], [RequiredMembershipDays], [RequireNoOverduePayment], [RequireManagerApproval], [IsActive], [CreatedByUserId])
        VALUES (@PlatinumId, @VipId, 100000.00, 50, 90, 1, 1, 1, @SeedUserId);
END;
