using SalesEngine.Models;

namespace SalesEngine.Services;

public interface IPaymentMethodService
{
    Task<IReadOnlyCollection<PaymentMethodModel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PaymentMethodModel>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<PaymentMethodModel?> GetByIdAsync(int paymentMethodId, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(PaymentMethodCreateModel model, CancellationToken cancellationToken = default);
    Task UpdateAsync(PaymentMethodUpdateModel model, CancellationToken cancellationToken = default);
    Task ToggleActiveAsync(int paymentMethodId, bool isActive, int updatedByUserId, CancellationToken cancellationToken = default);
    Task<bool> IsCodeExistsAsync(string paymentMethodCode, int? excludePaymentMethodId = null, CancellationToken cancellationToken = default);
}

public interface ISalesCheckoutService
{
    Task<SalesCheckoutProductModel?> GetProductByBarcodeAsync(string barcode, int locationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SalesCheckoutProductModel>> SearchProductsAsync(string? searchText, int locationId, int top = 20, CancellationToken cancellationToken = default);
    Task<SalesCompleteResultModel> CompleteTransactionAsync(SalesCompleteRequestModel request, CancellationToken cancellationToken = default);
}

public interface ISalesHistoryService
{
    Task<SalesPagedResultModel> GetPagedAsync(SalesPagedRequestModel request, CancellationToken cancellationToken = default);
    Task<SalesDetailModel?> GetDetailAsync(long salesHeaderId, CancellationToken cancellationToken = default);
    Task<SalesHeaderModel?> GetBySaleNoAsync(string saleNo, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SalesItemModel>> GetItemsAsync(long salesHeaderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SalesPaymentModel>> GetPaymentsAsync(long salesHeaderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SalesSummaryModel>> GetSummaryByDateRangeAsync(DateTime fromDate, DateTime toDate, int? cashierUserId = null, CancellationToken cancellationToken = default);
    Task VoidAsync(SalesVoidModel model, CancellationToken cancellationToken = default);
    Task<ReceiptPrintHistoryModel> ReprintReceiptAsync(long salesHeaderId, int printedByUserId, CancellationToken cancellationToken = default);
}

public interface ISalesClosingService
{
    Task<DailySalesClosingModel> GetDailyClosingAsync(DateTime closingDate, int? cashierUserId = null, CancellationToken cancellationToken = default);
    Task<DailySalesClosingModel> SaveDailyClosingAsync(DailySalesClosingSaveModel model, CancellationToken cancellationToken = default);
}

public interface IHeldSaleService
{
    Task<HeldSalePagedResultModel> GetPagedAsync(HeldSalePagedRequestModel request, CancellationToken cancellationToken = default);
    Task<HeldSaleDetailModel?> GetByIdAsync(long heldSaleHeaderId, CancellationToken cancellationToken = default);
    Task<long> CreateAsync(HeldSaleCreateModel model, CancellationToken cancellationToken = default);
    Task<HeldSaleDetailModel?> ResumeAsync(HeldSaleResumeModel model, CancellationToken cancellationToken = default);
    Task CancelAsync(HeldSaleCancelModel model, CancellationToken cancellationToken = default);
    Task<int> ExpireOldAsync(DateTime expireBeforeDate, int updatedByUserId, CancellationToken cancellationToken = default);
    Task<SalesCompleteResultModel> CompleteAsync(SalesCompleteRequestModel request, CancellationToken cancellationToken = default);
}

public interface ISalesReturnService
{
    Task<SalesReturnPagedResultModel> GetPagedAsync(SalesReturnPagedRequestModel request, CancellationToken cancellationToken = default);
    Task<SalesReturnDetailModel?> GetByIdAsync(long salesReturnHeaderId, CancellationToken cancellationToken = default);
    Task<long> CreateAsync(SalesReturnCreateModel model, CancellationToken cancellationToken = default);
    Task<long> AddItemAsync(SalesReturnItemCreateModel model, CancellationToken cancellationToken = default);
    Task ApproveAsync(SalesReturnApproveModel model, CancellationToken cancellationToken = default);
    Task RejectAsync(SalesReturnRejectModel model, CancellationToken cancellationToken = default);
    Task<SalesReturnHeaderModel> CompleteAsync(SalesReturnCompleteModel model, CancellationToken cancellationToken = default);
    Task CancelAsync(SalesReturnCancelModel model, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SalesReturnItemModel>> GetItemsAsync(long salesReturnHeaderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SalesReturnPaymentModel>> GetPaymentsAsync(long salesReturnHeaderId, CancellationToken cancellationToken = default);
}

public interface IReceiptService
{
    Task<long> CreatePrintHistoryAsync(ReceiptPrintHistoryCreateModel model, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ReceiptPrintHistoryModel>> GetBySalesHeaderIdAsync(long salesHeaderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ReceiptPrintHistoryModel>> GetBySalesReturnHeaderIdAsync(long salesReturnHeaderId, CancellationToken cancellationToken = default);
    Task<SalesDocumentModel> IssueSalesDocumentAsync(SalesDocumentIssueModel model, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SalesDocumentModel>> GetSalesDocumentsAsync(long salesHeaderId, CancellationToken cancellationToken = default);
    Task<SalesDocumentModel> RecordSalesDocumentPrintAsync(long salesDocumentId, int printedByUserId, CancellationToken cancellationToken = default);
}
