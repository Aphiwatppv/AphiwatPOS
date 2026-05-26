namespace SalesEngine.Models;

public sealed class TotalCountModel
{
    public int TotalCount { get; init; }
}

public sealed class PagedResultModel<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public sealed class PaymentMethodModel
{
    public int PaymentMethodId { get; init; }
    public string PaymentMethodCode { get; init; } = string.Empty;
    public string PaymentMethodName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool RequireReferenceNo { get; init; }
    public bool IsCash { get; init; }
    public bool IsActive { get; init; }
    public int DisplayOrder { get; init; }
    public int? CreatedByUserId { get; init; }
    public int? UpdatedByUserId { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? UpdatedDate { get; init; }
}

public sealed class PaymentMethodCreateModel
{
    public string PaymentMethodCode { get; init; } = string.Empty;
    public string PaymentMethodName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequireReferenceNo { get; init; }
    public bool IsCash { get; init; }
    public int DisplayOrder { get; init; }
    public int CreatedByUserId { get; init; }
}

public sealed class PaymentMethodUpdateModel
{
    public int PaymentMethodId { get; init; }
    public string PaymentMethodCode { get; init; } = string.Empty;
    public string PaymentMethodName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequireReferenceNo { get; init; }
    public bool IsCash { get; init; }
    public bool IsActive { get; init; }
    public int DisplayOrder { get; init; }
    public int UpdatedByUserId { get; init; }
}

public sealed class SalesCheckoutProductModel
{
    public int ProductId { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string? Barcode { get; init; }
    public int UnitId { get; init; }
    public string UnitSymbol { get; init; } = string.Empty;
    public decimal CostPrice { get; init; }
    public decimal SellingPrice { get; init; }
    public decimal WholesalePrice { get; init; }
    public decimal WholesaleMinQty { get; init; } = 1;
    public decimal TaxRate { get; init; }
    public bool DiscountAllowed { get; init; }
    public bool IsStockTracked { get; init; }
    public bool IsActive { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal CurrentStock { get; init; }
}

public sealed class SalesCartItemModel
{
    public int ProductId { get; init; }
    public int LocationId { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal ItemDiscountAmount { get; init; }
    public decimal TaxAmount { get; init; }
}

public sealed class SalesPaymentInputModel
{
    public int PaymentMethodId { get; init; }
    public decimal PaymentAmount { get; init; }
    public string? ReferenceNo { get; init; }
}

public sealed class SalesCompleteRequestModel
{
    public int? CustomerId { get; init; }
    public int CashierUserId { get; init; }
    public long? HeldSaleHeaderId { get; init; }
    public bool UseCustomerCredit { get; init; }
    public decimal CustomerCreditAmount { get; init; }
    public decimal OrderDiscountAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public string? Remark { get; init; }
    public bool AllowNegativeStock { get; init; }
    public int CreatedByUserId { get; init; }
    public IReadOnlyCollection<SalesCartItemModel> Items { get; init; } = Array.Empty<SalesCartItemModel>();
    public IReadOnlyCollection<SalesPaymentInputModel> Payments { get; init; } = Array.Empty<SalesPaymentInputModel>();
}

public sealed class SalesCompleteResultModel
{
    public long SalesHeaderId { get; init; }
    public string SaleNo { get; init; } = string.Empty;
    public decimal NetAmount { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal ChangeAmount { get; init; }
}

public sealed class SaleResponseModel
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public long? SaleId { get; init; }
    public string? SaleNo { get; init; }
    public string? InvoiceNo { get; init; }
    public DateTime? SaleDate { get; init; }
    public int? CustomerId { get; init; }
    public string CustomerName { get; init; } = "Walk-in Customer";
    public int? CashierId { get; init; }
    public string CashierName { get; init; } = string.Empty;
    public decimal TotalItems { get; init; }
    public decimal Subtotal { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal GrandTotal { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public decimal PaidAmount { get; init; }
    public decimal ChangeAmount { get; init; }
    public decimal CreditUsed { get; init; }
    public decimal? RemainingCredit { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
}

public class SalesHeaderModel
{
    public long SalesHeaderId { get; init; }
    public string SaleNo { get; init; } = string.Empty;
    public DateTime SaleDate { get; init; }
    public int? CustomerId { get; init; }
    public string? CustomerName { get; init; }
    public int CashierUserId { get; init; }
    public string CashierName { get; init; } = string.Empty;
    public decimal SubtotalAmount { get; init; }
    public decimal ItemDiscountAmount { get; init; }
    public decimal OrderDiscountAmount { get; init; }
    public decimal TotalDiscountAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal NetAmount { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal ChangeAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Remark { get; init; } = string.Empty;
    public int? CreatedByUserId { get; init; }
    public int? UpdatedByUserId { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? UpdatedDate { get; init; }
}

public sealed class SalesItemModel
{
    public long SalesItemId { get; init; }
    public long SalesHeaderId { get; init; }
    public int ProductId { get; init; }
    public string ProductCodeSnapshot { get; init; } = string.Empty;
    public string ProductNameSnapshot { get; init; } = string.Empty;
    public string? BarcodeSnapshot { get; init; }
    public int UnitId { get; init; }
    public string UnitSymbolSnapshot { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal CostPriceSnapshot { get; init; }
    public decimal ItemDiscountAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal LineSubtotal { get; init; }
    public decimal LineTotal { get; init; }
    public decimal ReturnedQty { get; init; }
    public DateTime CreatedDate { get; init; }
}

public sealed class SalesPaymentModel
{
    public long SalesPaymentId { get; init; }
    public long SalesHeaderId { get; init; }
    public int PaymentMethodId { get; init; }
    public string PaymentMethodName { get; init; } = string.Empty;
    public decimal PaymentAmount { get; init; }
    public string? ReferenceNo { get; init; }
    public DateTime PaymentDate { get; init; }
    public int CreatedByUserId { get; init; }
    public DateTime CreatedDate { get; init; }
}

public sealed class SalesDetailModel : SalesHeaderModel
{
    public IReadOnlyCollection<SalesItemModel> Items { get; init; } = Array.Empty<SalesItemModel>();
    public IReadOnlyCollection<SalesPaymentModel> Payments { get; init; } = Array.Empty<SalesPaymentModel>();
}

public sealed class SalesPagedRequestModel
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchText { get; init; }
    public int? CustomerId { get; init; }
    public int? CashierUserId { get; init; }
    public string? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public sealed class SalesPagedResultModel
{
    public IReadOnlyCollection<SalesHeaderModel> Sales { get; init; } = Array.Empty<SalesHeaderModel>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public sealed class SalesSummaryModel
{
    public DateTime SaleDate { get; init; }
    public int TransactionCount { get; init; }
    public decimal GrossAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal NetAmount { get; init; }
    public decimal RefundAmount { get; init; }
}

public sealed class DailySalesClosingModel
{
    public DateTime ClosingDate { get; init; }
    public int? CashierUserId { get; init; }
    public int TransactionCount { get; init; }
    public decimal GrossSalesAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal NetSalesAmount { get; init; }
    public decimal RefundAmount { get; init; }
    public decimal CashAmount { get; init; }
    public decimal TransferAmount { get; init; }
    public decimal CreditAmount { get; init; }
    public decimal OtherPaymentAmount { get; init; }
    public decimal ExpectedCashAmount { get; init; }
    public decimal CostOfGoodsSold { get; init; }
    public decimal GrossProfitAmount { get; init; }
    public int StockMovementCount { get; init; }
    public decimal StockMovementValue { get; init; }
    public long? DailySalesClosingId { get; init; }
    public decimal? ActualCashAmount { get; init; }
    public decimal? CashDifferenceAmount { get; init; }
    public string? Notes { get; init; }
    public int? ClosedByUserId { get; init; }
    public DateTime? ClosedAtUtc { get; init; }
    public string? ClosedByName { get; init; }
    public bool IsClosed => DailySalesClosingId.HasValue;
}

public sealed class DailySalesClosingSaveModel
{
    public DateTime ClosingDate { get; init; }
    public int? CashierUserId { get; init; }
    public decimal ActualCashAmount { get; init; }
    public string? Notes { get; init; }
    public int ClosedByUserId { get; init; }
}

public sealed class SalesVoidModel
{
    public long SalesHeaderId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public int UpdatedByUserId { get; init; }
    public bool ReverseInventory { get; init; } = true;
}

public class HeldSaleHeaderModel
{
    public long HeldSaleHeaderId { get; init; }
    public string HeldSaleNo { get; init; } = string.Empty;
    public DateTime HeldDate { get; init; }
    public int? CustomerId { get; init; }
    public string? CustomerName { get; init; }
    public int CashierUserId { get; init; }
    public string CashierName { get; init; } = string.Empty;
    public string Note { get; init; } = string.Empty;
    public decimal EstimatedSubtotalAmount { get; init; }
    public decimal EstimatedDiscountAmount { get; init; }
    public decimal EstimatedTaxAmount { get; init; }
    public decimal EstimatedNetAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public int? CreatedByUserId { get; init; }
    public int? UpdatedByUserId { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? UpdatedDate { get; init; }
}

public sealed class HeldSaleItemModel
{
    public long HeldSaleItemId { get; init; }
    public long HeldSaleHeaderId { get; init; }
    public int ProductId { get; init; }
    public string ProductCodeSnapshot { get; init; } = string.Empty;
    public string ProductNameSnapshot { get; init; } = string.Empty;
    public string? BarcodeSnapshot { get; init; }
    public int UnitId { get; init; }
    public string UnitSymbolSnapshot { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal CostPriceSnapshot { get; init; }
    public decimal ItemDiscountAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal LineSubtotal { get; init; }
    public decimal LineTotal { get; init; }
    public DateTime CreatedDate { get; init; }
}

public sealed class HeldSaleDetailModel : HeldSaleHeaderModel
{
    public IReadOnlyCollection<HeldSaleItemModel> Items { get; init; } = Array.Empty<HeldSaleItemModel>();
}

public sealed class HeldSaleCreateModel
{
    public int? CustomerId { get; init; }
    public int CashierUserId { get; init; }
    public string? Note { get; init; }
    public decimal EstimatedTaxAmount { get; init; }
    public int CreatedByUserId { get; init; }
    public IReadOnlyCollection<SalesCartItemModel> Items { get; init; } = Array.Empty<SalesCartItemModel>();
}

public sealed class HeldSalePagedRequestModel
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchText { get; init; }
    public int? CashierUserId { get; init; }
    public string? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public sealed class HeldSalePagedResultModel
{
    public IReadOnlyCollection<HeldSaleHeaderModel> HeldSales { get; init; } = Array.Empty<HeldSaleHeaderModel>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public sealed class HeldSaleResumeModel
{
    public long HeldSaleHeaderId { get; init; }
    public int UpdatedByUserId { get; init; }
}

public sealed class HeldSaleCancelModel
{
    public long HeldSaleHeaderId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public int UpdatedByUserId { get; init; }
}

public class SalesReturnHeaderModel
{
    public long SalesReturnHeaderId { get; init; }
    public string ReturnNo { get; init; } = string.Empty;
    public long SalesHeaderId { get; init; }
    public string SaleNo { get; init; } = string.Empty;
    public DateTime ReturnDate { get; init; }
    public int? CustomerId { get; init; }
    public int CashierUserId { get; init; }
    public decimal RefundSubtotalAmount { get; init; }
    public decimal RefundDiscountAmount { get; init; }
    public decimal RefundTaxAmount { get; init; }
    public decimal RefundNetAmount { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int? ApprovedByUserId { get; init; }
    public DateTime? ApprovedDate { get; init; }
    public int? CreatedByUserId { get; init; }
    public int? UpdatedByUserId { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? UpdatedDate { get; init; }
}

public sealed class SalesReturnItemModel
{
    public long SalesReturnItemId { get; init; }
    public long SalesReturnHeaderId { get; init; }
    public long SalesItemId { get; init; }
    public int ProductId { get; init; }
    public string ProductCodeSnapshot { get; init; } = string.Empty;
    public string ProductNameSnapshot { get; init; } = string.Empty;
    public string? BarcodeSnapshot { get; init; }
    public int UnitId { get; init; }
    public string UnitSymbolSnapshot { get; init; } = string.Empty;
    public decimal QuantityReturned { get; init; }
    public decimal RefundUnitPrice { get; init; }
    public decimal RefundAmount { get; init; }
    public bool ReturnToStock { get; init; }
    public string ReturnCondition { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public DateTime CreatedDate { get; init; }
}

public sealed class SalesReturnPaymentModel
{
    public long SalesReturnPaymentId { get; init; }
    public long SalesReturnHeaderId { get; init; }
    public int PaymentMethodId { get; init; }
    public string PaymentMethodName { get; init; } = string.Empty;
    public decimal RefundAmount { get; init; }
    public string? ReferenceNo { get; init; }
    public DateTime PaymentDate { get; init; }
    public int CreatedByUserId { get; init; }
    public DateTime CreatedDate { get; init; }
}

public sealed class SalesReturnDetailModel : SalesReturnHeaderModel
{
    public IReadOnlyCollection<SalesReturnItemModel> Items { get; init; } = Array.Empty<SalesReturnItemModel>();
    public IReadOnlyCollection<SalesReturnPaymentModel> Payments { get; init; } = Array.Empty<SalesReturnPaymentModel>();
}

public sealed class SalesReturnCreateModel
{
    public long SalesHeaderId { get; init; }
    public int CashierUserId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public int CreatedByUserId { get; init; }
}

public sealed class SalesReturnItemCreateModel
{
    public long SalesReturnHeaderId { get; init; }
    public long SalesItemId { get; init; }
    public decimal QuantityReturned { get; init; }
    public decimal RefundUnitPrice { get; init; }
    public bool ReturnToStock { get; init; }
    public string ReturnCondition { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

public sealed class SalesReturnPaymentInputModel
{
    public int PaymentMethodId { get; init; }
    public decimal RefundAmount { get; init; }
    public string? ReferenceNo { get; init; }
}

public sealed class SalesReturnApproveModel
{
    public long SalesReturnHeaderId { get; init; }
    public int ApprovedByUserId { get; init; }
}

public sealed class SalesReturnRejectModel
{
    public long SalesReturnHeaderId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public int UpdatedByUserId { get; init; }
}

public sealed class SalesReturnCompleteModel
{
    public long SalesReturnHeaderId { get; init; }
    public int CompletedByUserId { get; init; }
    public IReadOnlyCollection<SalesReturnPaymentInputModel> Payments { get; init; } = Array.Empty<SalesReturnPaymentInputModel>();
}

public sealed class SalesReturnCancelModel
{
    public long SalesReturnHeaderId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public int UpdatedByUserId { get; init; }
}

public sealed class SalesReturnPagedRequestModel
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchText { get; init; }
    public long? SalesHeaderId { get; init; }
    public int? CustomerId { get; init; }
    public int? CashierUserId { get; init; }
    public string? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public sealed class SalesReturnPagedResultModel
{
    public IReadOnlyCollection<SalesReturnHeaderModel> Returns { get; init; } = Array.Empty<SalesReturnHeaderModel>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public sealed class ReceiptPrintHistoryModel
{
    public long ReceiptPrintHistoryId { get; init; }
    public long? SalesHeaderId { get; init; }
    public long? SalesReturnHeaderId { get; init; }
    public string ReceiptNo { get; init; } = string.Empty;
    public string ReceiptType { get; init; } = string.Empty;
    public int PrintedByUserId { get; init; }
    public DateTime PrintedDate { get; init; }
}

public sealed class ReceiptPrintHistoryCreateModel
{
    public long? SalesHeaderId { get; init; }
    public long? SalesReturnHeaderId { get; init; }
    public string ReceiptNo { get; init; } = string.Empty;
    public string ReceiptType { get; init; } = string.Empty;
    public int PrintedByUserId { get; init; }
}

public sealed class SalesDocumentModel
{
    public long SalesDocumentId { get; init; }
    public long SalesHeaderId { get; init; }
    public string SaleNo { get; init; } = string.Empty;
    public string DocumentType { get; init; } = string.Empty;
    public string DocumentNo { get; init; } = string.Empty;
    public DateTime IssueDate { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerTaxId { get; init; } = string.Empty;
    public string CustomerBranch { get; init; } = string.Empty;
    public string CustomerAddress { get; init; } = string.Empty;
    public decimal SubtotalAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal VatAmount { get; init; }
    public decimal NetAmount { get; init; }
    public int PrintedCount { get; init; }
    public string Status { get; init; } = string.Empty;
    public int? IssuedByUserId { get; init; }
    public DateTime CreatedDate { get; init; }
}

public sealed class SalesDocumentIssueModel
{
    public long SalesHeaderId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string? CustomerName { get; init; }
    public string? CustomerTaxId { get; init; }
    public string? CustomerBranch { get; init; }
    public string? CustomerAddress { get; init; }
    public int IssuedByUserId { get; init; }
}
