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
}

public class CustomerSummaryModel
{
    public int CustomerId { get; init; }
    public string CustomerCode { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? MemberLevelName { get; init; }
    public decimal AvailablePoints { get; init; }
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
    public int? MemberLevelId { get; init; }
    public bool? IsActive { get; init; }
    public string? CreditStatus { get; init; }
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
    public int? MemberLevelId { get; init; }
    public bool? IsActive { get; init; }
    public int Top { get; init; } = 20;
    public DateTime? NoPurchaseAfterDate { get; init; }
}

public sealed class CustomerReportSummaryModel
{
    public int TotalCustomers { get; init; }
    public int ActiveCustomers { get; init; }
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
