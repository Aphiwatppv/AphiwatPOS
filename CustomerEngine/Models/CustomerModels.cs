namespace CustomerEngine.Models;

public sealed class PagedResultModel<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public sealed class TotalCountModel
{
    public int TotalCount { get; init; }
}

public class CustomerModel
{
    public int CustomerId { get; init; }
    public string CustomerCode { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string MemberType { get; init; } = "Retail";
    public string ActiveMemberTypeCodes { get; init; } = string.Empty;
    public IReadOnlyCollection<string> ActiveMembershipCodes => CustomerMembershipText.SplitCodes(ActiveMemberTypeCodes, MemberType);
    public int? MemberLevelId { get; init; }
    public string? MemberLevelCode { get; init; }
    public string? MemberLevelName { get; init; }
    public decimal DiscountPercent { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public string? Address { get; init; }
    public decimal TotalSpending { get; init; }
    public int TotalPurchaseCount { get; init; }
    public DateTime? LastPurchaseDate { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedDate { get; init; }
    public int CreatedByUserId { get; init; }
    public DateTime? UpdatedDate { get; init; }
    public int? UpdatedByUserId { get; init; }
    public decimal AvailablePoints { get; init; }
    public decimal LifetimeEarnedPoints { get; init; }
    public decimal LifetimeRedeemedPoints { get; init; }
    public bool AllowCredit { get; init; }
    public decimal CreditLimit { get; init; }
    public int CreditTermDays { get; init; }
    public decimal CurrentOutstandingAmount { get; init; }
    public decimal AvailableCredit { get; init; }
    public string CreditStatus { get; init; } = "Good";
    public bool RequireManagerApproval { get; init; }
    public string? WholesaleBusinessName { get; init; }
    public bool WholesaleApproved { get; init; }
    public int WholesalePaymentTermDays { get; init; }
    public string? RubberSupplierCode { get; init; }
    public decimal RubberWeightCarryForwardKg { get; init; }
    public decimal RubberLoyaltyPointBalance { get; init; }
}

public class CustomerSummaryModel
{
    public int CustomerId { get; init; }
    public string CustomerCode { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string MemberType { get; init; } = "Retail";
    public string ActiveMemberTypeCodes { get; init; } = string.Empty;
    public IReadOnlyCollection<string> ActiveMembershipCodes => CustomerMembershipText.SplitCodes(ActiveMemberTypeCodes, MemberType);
    public string? MemberLevelName { get; init; }
    public decimal AvailablePoints { get; init; }
    public decimal RubberWeightCarryForwardKg { get; init; }
    public decimal CreditLimit { get; init; }
    public decimal CurrentOutstandingAmount { get; init; }
    public decimal AvailableCredit { get; init; }
    public string CreditStatus { get; init; } = "Good";
    public decimal TotalSpending { get; init; }
    public int TotalPurchaseCount { get; init; }
    public DateTime? LastPurchaseDate { get; init; }
    public bool IsActive { get; init; }
}

public sealed class CustomerCreateModel
{
    public string? CustomerCode { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string MemberType { get; init; } = "Retail";
    public IReadOnlyCollection<string> MemberTypeCodes { get; init; } = Array.Empty<string>();
    public WholesaleMemberProfileSaveModel? WholesaleProfile { get; init; }
    public RubberSupplierMemberProfileSaveModel? RubberSupplierProfile { get; init; }
    public int? MemberLevelId { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public string? Address { get; init; }
    public int CreatedByUserId { get; init; }
}

public sealed class CustomerUpdateModel
{
    public int CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string MemberType { get; init; } = "Retail";
    public IReadOnlyCollection<string> MemberTypeCodes { get; init; } = Array.Empty<string>();
    public WholesaleMemberProfileSaveModel? WholesaleProfile { get; init; }
    public RubberSupplierMemberProfileSaveModel? RubberSupplierProfile { get; init; }
    public int? MemberLevelId { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public string? Address { get; init; }
    public bool ApplyMemberLevelCreditDefault { get; init; }
    public int UpdatedByUserId { get; init; }
}

public sealed class CustomerPagedRequestModel
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchText { get; init; }
    public string? MemberType { get; init; }
    public int? MemberLevelId { get; init; }
    public bool? IsActive { get; init; }
    public string? CreditStatus { get; init; }
}

public static class MemberTypeCodes
{
    public const string Retail = "RETAIL";
    public const string Wholesale = "WHOLESALE";
    public const string RubberSupplier = "RUBBER_SUPPLIER";
}

public sealed class MemberTypeModel
{
    public int MemberTypeId { get; init; }
    public string MemberTypeCode { get; init; } = string.Empty;
    public string MemberTypeName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

public sealed class CustomerMembershipModel
{
    public long CustomerMemberTypeId { get; init; }
    public int CustomerId { get; init; }
    public int MemberTypeId { get; init; }
    public string MemberTypeCode { get; init; } = string.Empty;
    public string MemberTypeName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public sealed class WholesaleMemberProfileModel
{
    public int CustomerId { get; init; }
    public string? BusinessName { get; init; }
    public int? WholesaleLevelId { get; init; }
    public bool IsApproved { get; init; }
    public int PaymentTermDays { get; init; }
    public int? ApprovedByUserId { get; init; }
    public DateTime? ApprovedDate { get; init; }
}

public sealed class WholesaleMemberProfileSaveModel
{
    public string? BusinessName { get; init; }
    public int? WholesaleLevelId { get; init; }
    public bool IsApproved { get; init; }
    public int PaymentTermDays { get; init; }
    public int? ApprovedByUserId { get; init; }
}

public sealed class RubberSupplierMemberProfileModel
{
    public int CustomerId { get; init; }
    public string SupplierCode { get; init; } = string.Empty;
    public int? DefaultBusinessLocationId { get; init; }
    public string? Remark { get; init; }
}

public sealed class RubberSupplierMemberProfileSaveModel
{
    public string? SupplierCode { get; init; }
    public int? DefaultBusinessLocationId { get; init; }
    public string? Remark { get; init; }
}

public sealed class RubberPriceModel
{
    public int RubberPriceId { get; init; }
    public decimal PricePerKg { get; init; }
    public decimal PercentageOfService { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? UpdatedDate { get; init; }
}

public sealed class RubberAuctionLocationModel
{
    public int RubberAuctionLocationId { get; init; }
    public string LocationName { get; init; } = string.Empty;
    public string? Address { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? UpdatedDate { get; init; }
}

public class RubberPurchaseHeaderModel
{
    public long RubberPurchaseHeaderId { get; init; }
    public int? CustomerId { get; init; }
    public string CustomerCode { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string? NonMemberFarmerName { get; init; }
    public string? NonMemberFarmerPhone { get; init; }
    public int BusinessLocationId { get; init; }
    public string LocationName { get; init; } = string.Empty;
    public int? RubberAuctionLocationId { get; init; }
    public string? RubberAuctionLocationName { get; init; }
    public DateTime TransactionDate { get; init; }
    public decimal WeightKg { get; init; }
    public int? RubberPriceId { get; init; }
    public long? MarketingPriceId { get; init; }
    public decimal? PricePerKgSnapshot { get; init; }
    public decimal? PercentageSnapshot { get; init; }
    public decimal? TotalAmount { get; init; }
    public string PaymentStatus { get; init; } = "Pending";
    public string? ReceiptNo { get; init; }
    public decimal PaidAmount { get; init; }
    public DateTime? PaidDate { get; init; }
    public string? PaymentMethod { get; init; }
    public string? PaymentRemark { get; init; }
    public decimal CreditDeductedAmount { get; init; }
    public int PointsEarned { get; init; }
    public decimal CarryForwardWeightAfterKg { get; init; }
    public int CreatedByUserId { get; init; }
    public DateTime CreatedDate { get; init; }
}

public sealed class RubberPurchaseHeaderCreateModel
{
    public int? CustomerId { get; init; }
    public string? NonMemberFarmerName { get; init; }
    public string? NonMemberFarmerPhone { get; init; }
    public int BusinessLocationId { get; init; }
    public int? RubberAuctionLocationId { get; init; }
    public DateTime TransactionDate { get; init; }
    public decimal WeightKg { get; init; }
    public int? RubberPriceId { get; init; }
    public long? MarketingPriceId { get; init; }
    public decimal? PricePerKgSnapshot { get; init; }
    public decimal? PercentageSnapshot { get; init; }
    public decimal? TotalAmount { get; init; }
    public string PaymentStatus { get; init; } = "Pending";
    public int CreatedByUserId { get; init; }
}

public sealed class RubberPurchaseBatchCreateModel
{
    public IReadOnlyCollection<RubberPurchaseHeaderCreateModel> Purchases { get; init; } = Array.Empty<RubberPurchaseHeaderCreateModel>();
}

public sealed class RubberPurchaseHeaderPagedRequestModel
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int? CustomerId { get; init; }
    public int? BusinessLocationId { get; init; }
    public int? RubberAuctionLocationId { get; init; }
    public string? PaymentStatus { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public string? SearchText { get; init; }
}

public sealed class RubberPurchaseHeaderPagedModel
{
    public IReadOnlyCollection<RubberPurchaseHeaderModel> Items { get; init; } = Array.Empty<RubberPurchaseHeaderModel>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public sealed class RubberPurchasePayBillModel
{
    public long RubberPurchaseHeaderId { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal CreditDeductedAmount { get; init; }
    public string PaymentMethod { get; init; } = "Cash";
    public string? PaymentRemark { get; init; }
    public int UpdatedByUserId { get; init; }
}

public sealed class LoyaltyRuleModel
{
    public int LoyaltyRuleId { get; init; }
    public string RuleCode { get; init; } = string.Empty;
    public string RuleName { get; init; } = string.Empty;
    public decimal WeightKgPerPoint { get; init; }
    public bool IsCarryForwardEnabled { get; init; }
    public bool IsActive { get; init; }
}

public sealed class MemberLoyaltyAccountModel
{
    public long MemberLoyaltyAccountId { get; init; }
    public int CustomerId { get; init; }
    public decimal PointBalance { get; init; }
    public decimal RubberWeightCarryForwardKg { get; init; }
}

public sealed class LoyaltyPointCalculationResultModel
{
    public decimal PreviousCarryForwardWeightKg { get; init; }
    public decimal RubberWeightKg { get; init; }
    public decimal WeightKgPerPoint { get; init; }
    public int PointsEarned { get; init; }
    public decimal CarryForwardWeightAfterKg { get; init; }
}

public sealed class MemberLoyaltyTransactionModel
{
    public long MemberLoyaltyTransactionId { get; init; }
    public long MemberLoyaltyAccountId { get; init; }
    public string TransactionType { get; init; } = string.Empty;
    public string SourceType { get; init; } = string.Empty;
    public long? ReferenceId { get; init; }
    public decimal RubberWeightKg { get; init; }
    public decimal WeightKgPerPointSnapshot { get; init; }
    public decimal PreviousCarryForwardWeightKg { get; init; }
    public decimal CarryForwardWeightAfterKg { get; init; }
    public int Points { get; init; }
    public decimal PointBalanceAfterTransaction { get; init; }
    public string? Remark { get; init; }
    public DateTime CreatedDate { get; init; }
    public int CreatedByUserId { get; init; }
}

public sealed class MemberSalesCreditAccountModel
{
    public long MemberSalesCreditAccountId { get; init; }
    public int CustomerId { get; init; }
    public decimal CreditLimit { get; init; }
    public decimal OutstandingBalance { get; init; }
    public decimal AvailableCredit => CreditLimit - OutstandingBalance;
    public bool IsApproved { get; init; }
    public bool IsActive { get; init; }
}

public sealed class MemberSalesCreditTransactionModel
{
    public long MemberSalesCreditTransactionId { get; init; }
    public long MemberSalesCreditAccountId { get; init; }
    public string TransactionType { get; init; } = string.Empty;
    public string? ReferenceType { get; init; }
    public long? ReferenceId { get; init; }
    public decimal Amount { get; init; }
    public decimal OutstandingBalanceAfterTransaction { get; init; }
    public string? Remark { get; init; }
    public DateTime CreatedDate { get; init; }
    public int CreatedByUserId { get; init; }
}

internal static class CustomerMembershipText
{
    public static IReadOnlyCollection<string> SplitCodes(string? activeCodes, string legacyMemberType)
    {
        var codes = (activeCodes ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeCode)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (codes.Length > 0) return codes;
        return new[] { legacyMemberType.Equals("Wholesale", StringComparison.OrdinalIgnoreCase) ? MemberTypeCodes.Wholesale : MemberTypeCodes.Retail };
    }

    public static string NormalizeCode(string value) => value.Trim().ToUpperInvariant() switch
    {
        "RETAIL" => MemberTypeCodes.Retail,
        "WHOLESALE" => MemberTypeCodes.Wholesale,
        "RUBBER_SUPPLIER" or "RUBBERSUPPLIER" or "RUBBER SUPPLIER" => MemberTypeCodes.RubberSupplier,
        _ => value.Trim().ToUpperInvariant()
    };
}

public sealed class CustomerPagedResultModel
{
    public IReadOnlyCollection<CustomerSummaryModel> Customers { get; init; } = Array.Empty<CustomerSummaryModel>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public class MemberLevelModel
{
    public int MemberLevelId { get; init; }
    public string LevelCode { get; init; } = string.Empty;
    public string LevelName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal MinSpendingAmount { get; init; }
    public decimal DiscountPercent { get; init; }
    public decimal PointEarnAmount { get; init; }
    public decimal PointEarnPoint { get; init; }
    public decimal PointMultiplier { get; init; }
    public bool AllowCredit { get; init; }
    public decimal DefaultCreditLimit { get; init; }
    public int DefaultCreditTermDays { get; init; }
    public bool RequireManagerApprovalForCredit { get; init; }
    public int MaxOverdueDaysAllowed { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedDate { get; init; }
    public int CreatedByUserId { get; init; }
    public DateTime? UpdatedDate { get; init; }
    public int? UpdatedByUserId { get; init; }
}

public sealed class MemberLevelCreateModel
{
    public string LevelCode { get; init; } = string.Empty;
    public string LevelName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal MinSpendingAmount { get; init; }
    public decimal DiscountPercent { get; init; }
    public decimal PointEarnAmount { get; init; } = 100;
    public decimal PointEarnPoint { get; init; } = 1;
    public decimal PointMultiplier { get; init; } = 1;
    public bool AllowCredit { get; init; }
    public decimal DefaultCreditLimit { get; init; }
    public int DefaultCreditTermDays { get; init; }
    public bool RequireManagerApprovalForCredit { get; init; }
    public int MaxOverdueDaysAllowed { get; init; }
    public int DisplayOrder { get; init; }
    public int CreatedByUserId { get; init; }
}

public sealed class MemberLevelUpdateModel
{
    public int MemberLevelId { get; init; }
    public string LevelCode { get; init; } = string.Empty;
    public string LevelName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal MinSpendingAmount { get; init; }
    public decimal DiscountPercent { get; init; }
    public decimal PointEarnAmount { get; init; }
    public decimal PointEarnPoint { get; init; }
    public decimal PointMultiplier { get; init; }
    public bool AllowCredit { get; init; }
    public decimal DefaultCreditLimit { get; init; }
    public int DefaultCreditTermDays { get; init; }
    public bool RequireManagerApprovalForCredit { get; init; }
    public int MaxOverdueDaysAllowed { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public int UpdatedByUserId { get; init; }
}

public class MemberLevelUpgradeRuleModel
{
    public int MemberLevelUpgradeRuleId { get; init; }
    public int FromMemberLevelId { get; init; }
    public string FromMemberLevelName { get; init; } = string.Empty;
    public int ToMemberLevelId { get; init; }
    public string ToMemberLevelName { get; init; } = string.Empty;
    public decimal RequiredTotalSpending { get; init; }
    public int RequiredPurchaseCount { get; init; }
    public int RequiredMembershipDays { get; init; }
    public bool RequireNoOverduePayment { get; init; }
    public bool RequireManagerApproval { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedDate { get; init; }
    public int CreatedByUserId { get; init; }
    public DateTime? UpdatedDate { get; init; }
    public int? UpdatedByUserId { get; init; }
}

public sealed class MemberLevelUpgradeRuleCreateModel
{
    public int FromMemberLevelId { get; init; }
    public int ToMemberLevelId { get; init; }
    public decimal RequiredTotalSpending { get; init; }
    public int RequiredPurchaseCount { get; init; }
    public int RequiredMembershipDays { get; init; }
    public bool RequireNoOverduePayment { get; init; } = true;
    public bool RequireManagerApproval { get; init; }
    public int CreatedByUserId { get; init; }
}

public sealed class MemberLevelUpgradeRuleUpdateModel
{
    public int MemberLevelUpgradeRuleId { get; init; }
    public int FromMemberLevelId { get; init; }
    public int ToMemberLevelId { get; init; }
    public decimal RequiredTotalSpending { get; init; }
    public int RequiredPurchaseCount { get; init; }
    public int RequiredMembershipDays { get; init; }
    public bool RequireNoOverduePayment { get; init; }
    public bool RequireManagerApproval { get; init; }
    public bool IsActive { get; init; }
    public int UpdatedByUserId { get; init; }
}

public sealed class CustomerLevelEligibilityResultModel
{
    public bool IsEligible { get; init; }
    public int CustomerId { get; init; }
    public int? CurrentMemberLevelId { get; init; }
    public string? CurrentMemberLevelName { get; init; }
    public int? NextMemberLevelId { get; init; }
    public string? NextMemberLevelName { get; init; }
    public decimal RequiredTotalSpending { get; init; }
    public decimal CurrentTotalSpending { get; init; }
    public decimal MissingSpendingAmount { get; init; }
    public int RequiredPurchaseCount { get; init; }
    public int CurrentPurchaseCount { get; init; }
    public int MissingPurchaseCount { get; init; }
    public int RequiredMembershipDays { get; init; }
    public int CurrentMembershipDays { get; init; }
    public bool HasOverdueCredit { get; init; }
    public bool RequireManagerApproval { get; init; }
    public string Message { get; init; } = string.Empty;
}

public class CustomerPointBalanceModel
{
    public long CustomerPointBalanceId { get; init; }
    public int CustomerId { get; init; }
    public decimal AvailablePoints { get; init; }
    public decimal LifetimeEarnedPoints { get; init; }
    public decimal LifetimeRedeemedPoints { get; init; }
    public DateTime? LastMovementDate { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? UpdatedDate { get; init; }
}

public class CustomerPointMovementModel
{
    public long CustomerPointMovementId { get; init; }
    public int CustomerId { get; init; }
    public string MovementType { get; init; } = string.Empty;
    public decimal PointsIn { get; init; }
    public decimal PointsOut { get; init; }
    public decimal BalanceAfter { get; init; }
    public string? ReferenceType { get; init; }
    public long? ReferenceId { get; init; }
    public string? ReferenceNo { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public string? Remark { get; init; }
    public DateTime CreatedDate { get; init; }
    public int CreatedByUserId { get; init; }
}

public sealed class LoyaltyPointPagedRequestModel
{
    public int CustomerId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public string? MovementType { get; init; }
}

public sealed class LoyaltyPointPagedResultModel
{
    public IReadOnlyCollection<CustomerPointMovementModel> Movements { get; init; } = Array.Empty<CustomerPointMovementModel>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public sealed class LoyaltyPointAdjustModel
{
    public int CustomerId { get; init; }
    public string AdjustmentType { get; init; } = string.Empty;
    public decimal Points { get; init; }
    public string? ReferenceType { get; init; }
    public long? ReferenceId { get; init; }
    public string? ReferenceNo { get; init; }
    public string? Remark { get; init; }
    public int CreatedByUserId { get; init; }
}

public class CustomerCreditModel
{
    public long CustomerCreditId { get; init; }
    public int CustomerId { get; init; }
    public bool AllowCredit { get; init; }
    public decimal CreditLimit { get; init; }
    public int CreditTermDays { get; init; }
    public decimal CurrentOutstandingAmount { get; init; }
    public decimal AvailableCredit { get; init; }
    public string CreditStatus { get; init; } = "Good";
    public bool RequireManagerApproval { get; init; }
    public int? ApprovedByUserId { get; init; }
    public DateTime? ApprovedDate { get; init; }
    public string? Remark { get; init; }
    public DateTime CreatedDate { get; init; }
    public int CreatedByUserId { get; init; }
    public DateTime? UpdatedDate { get; init; }
    public int? UpdatedByUserId { get; init; }
}

public sealed class CustomerCreditPosModel
{
    public int CustomerId { get; init; }
    public string CustomerCode { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public int? MemberLevelId { get; init; }
    public string? MemberLevelName { get; init; }
    public decimal CreditLimit { get; init; }
    public decimal UsedCredit { get; init; }
    public decimal AvailableCredit { get; init; }
    public bool IsCreditAllowed { get; init; }
    public bool IsActive { get; init; }
}

public sealed class CustomerCreditUpdateModel
{
    public int CustomerId { get; init; }
    public bool AllowCredit { get; init; }
    public decimal CreditLimit { get; init; }
    public int CreditTermDays { get; init; }
    public string CreditStatus { get; init; } = "Good";
    public bool RequireManagerApproval { get; init; }
    public int? ApprovedByUserId { get; init; }
    public string? Remark { get; init; }
    public int UpdatedByUserId { get; init; }
}

public sealed class CustomerCreditCheckResultModel
{
    public bool IsAllowed { get; init; }
    public bool RequiresManagerApproval { get; init; }
    public decimal CreditLimit { get; init; }
    public decimal CurrentOutstandingAmount { get; init; }
    public decimal AvailableCredit { get; init; }
    public decimal RequestedAmount { get; init; }
    public string Message { get; init; } = string.Empty;
}

public class CustomerCreditTransactionModel
{
    public long CustomerCreditTransactionId { get; init; }
    public int CustomerId { get; init; }
    public long? SaleId { get; init; }
    public string TransactionType { get; init; } = string.Empty;
    public string? ReferenceType { get; init; }
    public long? ReferenceId { get; init; }
    public decimal Amount { get; init; }
    public decimal BalanceBefore { get; init; }
    public decimal BalanceAfter { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? PaidDate { get; init; }
    public string? ReferenceNo { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Remark { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedDate { get; init; }
    public int CreatedByUserId { get; init; }
}

public sealed class CustomerCreditTransactionPagedRequestModel
{
    public int CustomerId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public string? TransactionType { get; init; }
    public string? Status { get; init; }
}

public sealed class CustomerCreditTransactionPagedResultModel
{
    public IReadOnlyCollection<CustomerCreditTransactionModel> Transactions { get; init; } = Array.Empty<CustomerCreditTransactionModel>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public class CustomerCreditRepaymentModel
{
    public long CustomerCreditRepaymentId { get; init; }
    public string RepaymentNo { get; init; } = string.Empty;
    public int CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public DateTime RepaymentDate { get; init; }
    public int PaymentMethodId { get; init; }
    public string PaymentMethodName { get; init; } = string.Empty;
    public decimal PaymentAmount { get; init; }
    public string? ReferenceNo { get; init; }
    public string? Remark { get; init; }
    public string Status { get; init; } = string.Empty;
    public int CreatedByUserId { get; init; }
    public int? UpdatedByUserId { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? UpdatedDate { get; init; }
}

public sealed class CustomerCreditRepaymentCreateModel
{
    public int CustomerId { get; init; }
    public int PaymentMethodId { get; init; }
    public decimal PaymentAmount { get; init; }
    public string? ReferenceNo { get; init; }
    public string? Remark { get; init; }
    public int CreatedByUserId { get; init; }
}

public sealed class CustomerCreditRepaymentResultModel
{
    public long CustomerCreditRepaymentId { get; init; }
    public string RepaymentNo { get; init; } = string.Empty;
}

public sealed class CustomerCreditRepaymentPagedRequestModel
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int? CustomerId { get; init; }
    public string? Status { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
}

public sealed class CustomerCreditRepaymentPagedResultModel
{
    public IReadOnlyCollection<CustomerCreditRepaymentModel> Repayments { get; init; } = Array.Empty<CustomerCreditRepaymentModel>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public sealed class CreditRepaymentDisplayModel
{
    public string TerminalId { get; init; } = "default";
    public string DisplayState { get; init; } = "Ready";
    public int? CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string MemberCode { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string MemberLevel { get; init; } = string.Empty;
    public string CustomerType { get; init; } = string.Empty;
    public decimal OutstandingBalance { get; init; }
    public decimal AvailableCreditLimit { get; init; }
    public decimal OverdueAmount { get; init; }
    public int UnpaidInvoiceCount { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? LastRepaymentDate { get; init; }
    public decimal RepaymentAmount { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public decimal CashReceived { get; init; }
    public decimal ChangeAmount { get; init; }
    public decimal RemainingBalance { get; init; }
    public string ReceiptNo { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
}

public sealed class CreditRepaymentResponseModel
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public long? RepaymentId { get; init; }
    public string ReceiptNo { get; init; } = string.Empty;
    public int CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public decimal OutstandingBalanceBefore { get; init; }
    public decimal RepaymentAmount { get; init; }
    public decimal RemainingBalance { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public decimal CashReceived { get; init; }
    public decimal ChangeAmount { get; init; }
    public string ErrorCode { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
}

public sealed class CustomerCreditPaymentModel
{
    public int CustomerId { get; init; }
    public decimal Amount { get; init; }
    public DateTime? PaidDate { get; init; }
    public string? ReferenceNo { get; init; }
    public string? Remark { get; init; }
    public int CreatedByUserId { get; init; }
}

public sealed class CustomerCreditAdjustmentModel
{
    public int CustomerId { get; init; }
    public string AdjustmentType { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string? ReferenceNo { get; init; }
    public string? Remark { get; init; }
    public int CreatedByUserId { get; init; }
}

public sealed class CustomerHistorySummaryModel
{
    public int CustomerId { get; init; }
    public string CustomerCode { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string MemberType { get; init; } = "Retail";
    public string? MemberLevelName { get; init; }
    public decimal TotalSpending { get; init; }
    public int TotalPurchaseCount { get; init; }
    public DateTime? LastPurchaseDate { get; init; }
    public decimal AvailablePoints { get; init; }
    public decimal LifetimeEarnedPoints { get; init; }
    public decimal LifetimeRedeemedPoints { get; init; }
    public decimal CreditLimit { get; init; }
    public decimal CurrentOutstandingAmount { get; init; }
    public decimal AvailableCredit { get; init; }
    public string CreditStatus { get; init; } = "Good";
    public decimal TotalCreditSales { get; init; }
    public decimal TotalCreditPayments { get; init; }
    public decimal OverdueAmount { get; init; }
    public int OverdueCount { get; init; }
    public int ImportantNoteCount { get; init; }
}

public class CustomerPurchaseHistoryModel
{
    public long SaleId { get; init; }
    public string SaleNo { get; init; } = string.Empty;
    public DateTime SaleDate { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal NetAmount { get; init; }
    public string PaymentStatus { get; init; } = string.Empty;
    public string SaleStatus { get; init; } = string.Empty;
    public string CreatedByName { get; init; } = string.Empty;
}

public class CustomerPaymentHistoryModel
{
    public long PaymentId { get; init; }
    public long? SaleId { get; init; }
    public string? SaleNo { get; init; }
    public DateTime PaymentDate { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string? ReferenceNo { get; init; }
    public string PaymentStatus { get; init; } = string.Empty;
    public string CreatedByName { get; init; } = string.Empty;
}

public class CustomerCreditHistoryModel : CustomerCreditTransactionModel;

public class CustomerPointHistoryModel : CustomerPointMovementModel;

public class CustomerLevelHistoryModel
{
    public long CustomerLevelHistoryId { get; init; }
    public int CustomerId { get; init; }
    public int? OldMemberLevelId { get; init; }
    public string? OldMemberLevelName { get; init; }
    public int NewMemberLevelId { get; init; }
    public string NewMemberLevelName { get; init; } = string.Empty;
    public string? ChangeReason { get; init; }
    public DateTime ChangedDate { get; init; }
    public int ChangedByUserId { get; init; }
    public string ChangedByName { get; init; } = string.Empty;
}

public class CustomerRefundHistoryModel
{
    public long RefundId { get; init; }
    public long? SaleId { get; init; }
    public string? SaleNo { get; init; }
    public DateTime RefundDate { get; init; }
    public decimal RefundAmount { get; init; }
    public string RefundReason { get; init; } = string.Empty;
    public string RefundStatus { get; init; } = string.Empty;
    public string CreatedByName { get; init; } = string.Empty;
}

public class CustomerTimelineModel
{
    public DateTime ActivityDate { get; init; }
    public string HistoryType { get; init; } = string.Empty;
    public long? ReferenceId { get; init; }
    public string? ReferenceNo { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal? Amount { get; init; }
    public decimal? Points { get; init; }
    public string? Status { get; init; }
    public string CreatedByName { get; init; } = string.Empty;
}

public class CustomerHistoryPagedRequestModel
{
    public int CustomerId { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class CustomerTimelinePagedRequestModel : CustomerHistoryPagedRequestModel
{
    public string? HistoryType { get; init; }
}

public class CustomerNoteModel
{
    public long CustomerNoteId { get; init; }
    public int CustomerId { get; init; }
    public string NoteType { get; init; } = string.Empty;
    public string NoteText { get; init; } = string.Empty;
    public bool IsImportant { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedDate { get; init; }
    public int CreatedByUserId { get; init; }
    public DateTime? UpdatedDate { get; init; }
    public int? UpdatedByUserId { get; init; }
}

public sealed class CustomerNoteCreateModel
{
    public int CustomerId { get; init; }
    public string NoteType { get; init; } = string.Empty;
    public string NoteText { get; init; } = string.Empty;
    public bool IsImportant { get; init; }
    public int CreatedByUserId { get; init; }
}

public sealed class CustomerNoteUpdateModel
{
    public long CustomerNoteId { get; init; }
    public string NoteType { get; init; } = string.Empty;
    public string NoteText { get; init; } = string.Empty;
    public bool IsImportant { get; init; }
    public bool IsActive { get; init; }
    public int UpdatedByUserId { get; init; }
}

public sealed class CustomerNotePagedRequestModel
{
    public int CustomerId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? NoteType { get; init; }
    public bool? IsActive { get; init; }
}

public sealed class CustomerNotePagedResultModel
{
    public IReadOnlyCollection<CustomerNoteModel> Notes { get; init; } = Array.Empty<CustomerNoteModel>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public sealed class CustomerReportRequestModel
{
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public string? MemberType { get; init; }
    public int? MemberLevelId { get; init; }
    public bool? IsActive { get; init; }
    public int Top { get; init; } = 20;
    public DateTime? NoPurchaseAfterDate { get; init; }
}

public sealed class CustomerReportSummaryModel
{
    public int TotalCustomers { get; init; }
    public int ActiveCustomers { get; init; }
    public int RetailMemberCount { get; init; }
    public int WholesaleMemberCount { get; init; }
    public int RubberSupplierMemberCount { get; init; }
    public int NewCustomers { get; init; }
    public decimal TotalCustomerSpending { get; init; }
    public decimal TotalOutstandingCredit { get; init; }
    public decimal TotalAvailablePoints { get; init; }
    public int TotalCreditCustomers { get; init; }
    public int TotalOverdueCustomers { get; init; }
}

public sealed class TopCustomerModel
{
    public int CustomerId { get; init; }
    public string CustomerCode { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string? MemberLevelName { get; init; }
    public decimal TotalSpending { get; init; }
    public int TotalPurchaseCount { get; init; }
    public DateTime? LastPurchaseDate { get; init; }
}

public sealed class MemberLevelSummaryModel
{
    public int? MemberLevelId { get; init; }
    public string MemberLevelName { get; init; } = string.Empty;
    public int CustomerCount { get; init; }
    public decimal TotalSpending { get; init; }
    public decimal TotalOutstandingCredit { get; init; }
}

public sealed class LoyaltyPointSummaryModel
{
    public decimal TotalEarnedPoints { get; init; }
    public decimal TotalRedeemedPoints { get; init; }
    public decimal TotalAvailablePoints { get; init; }
    public decimal TotalExpiredPoints { get; init; }
}

public sealed class CustomerCreditSummaryModel
{
    public decimal TotalCreditLimit { get; init; }
    public decimal TotalOutstandingAmount { get; init; }
    public decimal TotalAvailableCredit { get; init; }
    public decimal OverdueAmount { get; init; }
    public int OverdueCount { get; init; }
    public int BlockedCustomerCount { get; init; }
}

public sealed class InactiveCustomerModel
{
    public int CustomerId { get; init; }
    public string CustomerCode { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string? MemberLevelName { get; init; }
    public decimal TotalSpending { get; init; }
    public int TotalPurchaseCount { get; init; }
    public DateTime? LastPurchaseDate { get; init; }
}

public class CustomerAuditLogModel
{
    public long CustomerAuditLogId { get; init; }
    public int? CustomerId { get; init; }
    public string ActionType { get; init; } = string.Empty;
    public string EntityName { get; init; } = string.Empty;
    public long? EntityId { get; init; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public string? Remark { get; init; }
    public DateTime CreatedDate { get; init; }
    public int CreatedByUserId { get; init; }
}

public sealed class CustomerAuditLogPagedRequestModel
{
    public int? CustomerId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? ActionType { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
}

public sealed class CustomerAuditLogPagedResultModel
{
    public IReadOnlyCollection<CustomerAuditLogModel> Logs { get; init; } = Array.Empty<CustomerAuditLogModel>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}
