using CustomerEngine.Models;

namespace CustomerEngine.Services;

public interface ICustomerService
{
    Task<CustomerPagedResultModel> GetPagedAsync(CustomerPagedRequestModel request, CancellationToken cancellationToken = default);
    Task<CustomerModel?> GetByIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task<CustomerModel?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CustomerCreateModel model, CancellationToken cancellationToken = default);
    Task UpdateAsync(CustomerUpdateModel model, CancellationToken cancellationToken = default);
    Task ToggleActiveAsync(int customerId, bool isActive, int updatedByUserId, CancellationToken cancellationToken = default);
    Task<bool> IsPhoneNumberExistsAsync(string phoneNumber, int? excludeCustomerId = null, CancellationToken cancellationToken = default);
    Task<bool> IsEmailExistsAsync(string email, int? excludeCustomerId = null, CancellationToken cancellationToken = default);
    Task UpdatePurchaseSummaryAsync(int customerId, decimal saleAmount, DateTime purchaseDate, CancellationToken cancellationToken = default);
}

public interface ICustomerMemberTypeService
{
    Task<IReadOnlyCollection<MemberTypeModel>> GetAllActiveTypesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CustomerMembershipModel>> GetActiveMembershipsAsync(int customerId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveMembershipAsync(int customerId, string memberTypeCode, CancellationToken cancellationToken = default);
    Task AssignAsync(int customerId, string memberTypeCode, int createdByUserId, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int customerId, string memberTypeCode, int updatedByUserId, CancellationToken cancellationToken = default);
    Task SyncAsync(int customerId, IReadOnlyCollection<string> memberTypeCodes, WholesaleMemberProfileSaveModel? wholesaleProfile, RubberSupplierMemberProfileSaveModel? rubberSupplierProfile, int updatedByUserId, CancellationToken cancellationToken = default);
}

public interface ICustomerRegistrationService
{
    Task<int> RegisterAsync(CustomerCreateModel model, CancellationToken cancellationToken = default);
    Task UpdateMembershipsAsync(CustomerUpdateModel model, CancellationToken cancellationToken = default);
}

public interface IMemberLoyaltyService
{
    Task<MemberLoyaltyAccountModel?> GetAccountAsync(int customerId, CancellationToken cancellationToken = default);
    Task<LoyaltyPointCalculationResultModel> CalculateFromRubberWeightAsync(int customerId, decimal rubberWeightKg, CancellationToken cancellationToken = default);
    Task<MemberLoyaltyTransactionModel> AddPointsFromRubberPurchaseAsync(int customerId, long rubberPurchaseHeaderId, decimal confirmedWeightKg, int employeeId, CancellationToken cancellationToken = default);
    Task RedeemPointsAsync(int customerId, int points, string? remark, int employeeId, CancellationToken cancellationToken = default);
    Task ReversePointsFromCancelledRubberPurchaseAsync(long rubberPurchaseHeaderId, int employeeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MemberLoyaltyTransactionModel>> GetTransactionsAsync(int customerId, CancellationToken cancellationToken = default);
}

public interface IRubberPurchaseService
{
    Task<long> CreateAsync(RubberPurchaseHeaderCreateModel model, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<long>> CreateBatchAsync(RubberPurchaseBatchCreateModel model, CancellationToken cancellationToken = default);
    Task<RubberPurchaseHeaderPagedModel> GetPagedAsync(RubberPurchaseHeaderPagedRequestModel request, CancellationToken cancellationToken = default);
    Task<RubberPurchaseHeaderModel?> GetByIdAsync(long rubberPurchaseHeaderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RubberPurchaseHeaderModel>> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task<RubberPurchaseHeaderModel> PayBillAsync(RubberPurchasePayBillModel model, CancellationToken cancellationToken = default);
}

public interface IRubberPriceService
{
    Task<IReadOnlyCollection<RubberPriceModel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RubberPriceModel>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<RubberPriceModel?> GetByIdAsync(int rubberPriceId, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(RubberPriceSaveModel model, CancellationToken cancellationToken = default);
    Task UpdateAsync(RubberPriceUpdateModel model, CancellationToken cancellationToken = default);
    Task ToggleActiveAsync(int rubberPriceId, bool isActive, CancellationToken cancellationToken = default);
    Task HardDeleteAsync(int rubberPriceId, CancellationToken cancellationToken = default);
}

public interface IRubberAuctionLocationService
{
    Task<IReadOnlyCollection<RubberAuctionLocationModel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RubberAuctionLocationModel>> GetActiveAsync(CancellationToken cancellationToken = default);
}

public interface IMemberSalesCreditService
{
    Task<CustomerCreditModel?> GetAccountAsync(int customerId, CancellationToken cancellationToken = default);
    Task<decimal> GetAvailableCreditAsync(int customerId, CancellationToken cancellationToken = default);
    Task<bool> HasEnoughCreditAsync(int customerId, decimal requestedAmount, CancellationToken cancellationToken = default);
    Task CreateOrUpdateCreditApprovalAsync(CustomerCreditUpdateModel model, CancellationToken cancellationToken = default);
    Task<long> UseCreditForSaleAsync(int customerId, long saleId, decimal amount, int employeeId, CancellationToken cancellationToken = default);
    Task RecordRepaymentAsync(int customerId, decimal amount, string? paymentMethod, int employeeId, CancellationToken cancellationToken = default);
    Task AdjustCreditLimitAsync(int customerId, decimal newLimit, string reason, int approvedByEmployeeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CustomerCreditTransactionModel>> GetTransactionsAsync(int customerId, CancellationToken cancellationToken = default);
}

public interface IMemberLevelService
{
    Task<IReadOnlyCollection<MemberLevelModel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MemberLevelModel>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<MemberLevelModel?> GetByIdAsync(int memberLevelId, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(MemberLevelCreateModel model, CancellationToken cancellationToken = default);
    Task UpdateAsync(MemberLevelUpdateModel model, CancellationToken cancellationToken = default);
    Task ToggleActiveAsync(int memberLevelId, bool isActive, int updatedByUserId, CancellationToken cancellationToken = default);
    Task<bool> IsLevelCodeExistsAsync(string levelCode, int? excludeMemberLevelId = null, CancellationToken cancellationToken = default);
}

public interface IMemberLevelUpgradeRuleService
{
    Task<IReadOnlyCollection<MemberLevelUpgradeRuleModel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MemberLevelUpgradeRuleModel?> GetByIdAsync(int memberLevelUpgradeRuleId, CancellationToken cancellationToken = default);
    Task<MemberLevelUpgradeRuleModel?> GetByFromLevelIdAsync(int fromMemberLevelId, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(MemberLevelUpgradeRuleCreateModel model, CancellationToken cancellationToken = default);
    Task UpdateAsync(MemberLevelUpgradeRuleUpdateModel model, CancellationToken cancellationToken = default);
    Task ToggleActiveAsync(int memberLevelUpgradeRuleId, bool isActive, int updatedByUserId, CancellationToken cancellationToken = default);
    Task<CustomerLevelEligibilityResultModel> CheckCustomerLevelEligibilityAsync(int customerId, CancellationToken cancellationToken = default);
    Task UpgradeCustomerLevelAsync(int customerId, int changedByUserId, bool applyMemberLevelCreditDefault = true, bool managerApproved = false, CancellationToken cancellationToken = default);
}

public interface ILoyaltyPointService
{
    Task<CustomerPointBalanceModel?> GetBalanceAsync(int customerId, CancellationToken cancellationToken = default);
    Task<LoyaltyPointPagedResultModel> GetMovementsPagedAsync(LoyaltyPointPagedRequestModel request, CancellationToken cancellationToken = default);
    Task<decimal> EarnPointsAsync(int customerId, decimal saleAmount, string? referenceType, long? referenceId, string? referenceNo, DateTime? expiryDate, int createdByUserId, CancellationToken cancellationToken = default);
    Task RedeemPointsAsync(int customerId, decimal points, string? referenceType, long? referenceId, string? referenceNo, string? remark, int createdByUserId, CancellationToken cancellationToken = default);
    Task AdjustPointsAsync(LoyaltyPointAdjustModel model, CancellationToken cancellationToken = default);
    Task ReverseByReferenceAsync(string referenceType, long referenceId, string? remark, int createdByUserId, CancellationToken cancellationToken = default);
    Task<int> ExpirePointsAsync(DateTime expiryDate, int createdByUserId, CancellationToken cancellationToken = default);
}

public interface ICustomerCreditService
{
    Task<CustomerCreditModel?> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CustomerCreditPosModel>> SearchForPOSAsync(string? searchText, int top = 20, CancellationToken cancellationToken = default);
    Task<CustomerCreditPosModel?> GetCreditInfoAsync(int customerId, CancellationToken cancellationToken = default);
    Task SetCreditAsync(CustomerCreditUpdateModel model, CancellationToken cancellationToken = default);
    Task<CustomerCreditCheckResultModel> CheckEligibilityAsync(int customerId, decimal saleAmount, CancellationToken cancellationToken = default);
    Task<long> CreateCreditSaleAsync(int customerId, long saleId, decimal amount, string? referenceNo, bool managerApproved, string? remark, int createdByUserId, CancellationToken cancellationToken = default);
    Task ReceivePaymentAsync(CustomerCreditPaymentModel model, CancellationToken cancellationToken = default);
    Task AdjustCreditAsync(CustomerCreditAdjustmentModel model, CancellationToken cancellationToken = default);
    Task<CustomerCreditTransactionPagedResultModel> GetTransactionsPagedAsync(CustomerCreditTransactionPagedRequestModel request, CancellationToken cancellationToken = default);
    Task<CustomerCreditRepaymentResultModel> CreateRepaymentAsync(CustomerCreditRepaymentCreateModel model, CancellationToken cancellationToken = default);
    Task<CustomerCreditRepaymentPagedResultModel> GetRepaymentsPagedAsync(CustomerCreditRepaymentPagedRequestModel request, CancellationToken cancellationToken = default);
    Task<CustomerCreditRepaymentModel?> GetRepaymentByIdAsync(long customerCreditRepaymentId, CancellationToken cancellationToken = default);
    Task VoidRepaymentAsync(long customerCreditRepaymentId, string reason, int updatedByUserId, CancellationToken cancellationToken = default);
    Task<int> UpdateOverdueStatusAsync(CancellationToken cancellationToken = default);
}

public interface ICustomerHistoryService
{
    Task<CustomerHistorySummaryModel?> GetSummaryAsync(int customerId, DateTime? dateFrom = null, DateTime? dateTo = null, CancellationToken cancellationToken = default);
    Task<PagedResultModel<CustomerPurchaseHistoryModel>> GetPurchaseHistoryAsync(CustomerHistoryPagedRequestModel request, CancellationToken cancellationToken = default);
    Task<PagedResultModel<CustomerPaymentHistoryModel>> GetPaymentHistoryAsync(CustomerHistoryPagedRequestModel request, CancellationToken cancellationToken = default);
    Task<PagedResultModel<CustomerCreditHistoryModel>> GetCreditHistoryAsync(CustomerHistoryPagedRequestModel request, CancellationToken cancellationToken = default);
    Task<PagedResultModel<CustomerPointHistoryModel>> GetPointHistoryAsync(CustomerHistoryPagedRequestModel request, CancellationToken cancellationToken = default);
    Task<PagedResultModel<CustomerLevelHistoryModel>> GetLevelHistoryAsync(CustomerHistoryPagedRequestModel request, CancellationToken cancellationToken = default);
    Task<PagedResultModel<CustomerRefundHistoryModel>> GetRefundHistoryAsync(CustomerHistoryPagedRequestModel request, CancellationToken cancellationToken = default);
    Task<PagedResultModel<CustomerTimelineModel>> GetTimelineAsync(CustomerTimelinePagedRequestModel request, CancellationToken cancellationToken = default);
    Task<CustomerNotePagedResultModel> GetNotesPagedAsync(CustomerNotePagedRequestModel request, CancellationToken cancellationToken = default);
    Task<CustomerNoteModel?> GetNoteByIdAsync(long customerNoteId, CancellationToken cancellationToken = default);
    Task<long> AddNoteAsync(CustomerNoteCreateModel model, CancellationToken cancellationToken = default);
    Task UpdateNoteAsync(CustomerNoteUpdateModel model, CancellationToken cancellationToken = default);
    Task ToggleNoteActiveAsync(long customerNoteId, bool isActive, int updatedByUserId, CancellationToken cancellationToken = default);
}

public interface ICustomerReportService
{
    Task<CustomerReportSummaryModel> GetSummaryAsync(CustomerReportRequestModel request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TopCustomerModel>> GetTopCustomersBySpendingAsync(CustomerReportRequestModel request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TopCustomerModel>> GetTopCustomersByVisitAsync(CustomerReportRequestModel request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MemberLevelSummaryModel>> GetMemberLevelSummaryAsync(CustomerReportRequestModel request, CancellationToken cancellationToken = default);
    Task<LoyaltyPointSummaryModel> GetLoyaltyPointSummaryAsync(CustomerReportRequestModel request, CancellationToken cancellationToken = default);
    Task<CustomerCreditSummaryModel> GetCreditSummaryAsync(CustomerReportRequestModel request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InactiveCustomerModel>> GetInactiveCustomersAsync(CustomerReportRequestModel request, CancellationToken cancellationToken = default);
}

public interface ICustomerAuditService
{
    Task<CustomerAuditLogPagedResultModel> GetPagedAsync(CustomerAuditLogPagedRequestModel request, CancellationToken cancellationToken = default);
}
