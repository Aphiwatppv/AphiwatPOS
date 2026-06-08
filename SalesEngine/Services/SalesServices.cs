using System.Text.Json;
using AccessEngine.Services;
using Dapper;
using SalesEngine.Models;

namespace SalesEngine.Services;

public sealed class PaymentMethodService : IPaymentMethodService
{
    private readonly IAccessService _accessService;

    public PaymentMethodService(IAccessService accessService) => _accessService = accessService;

    public async Task<IReadOnlyCollection<PaymentMethodModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _accessService.QueryAsync<PaymentMethodModel, object>("dbo.spPaymentMethodGetAll", new { }, cancellationToken);
        return rows.ToArray();
    }

    public async Task<IReadOnlyCollection<PaymentMethodModel>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _accessService.QueryAsync<PaymentMethodModel, object>("dbo.spPaymentMethodGetAllActive", new { }, cancellationToken);
        return rows.ToArray();
    }

    public Task<PaymentMethodModel?> GetByIdAsync(int paymentMethodId, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(paymentMethodId, nameof(paymentMethodId));
        return _accessService.QuerySingleOrDefaultAsync<PaymentMethodModel, object>("dbo.spPaymentMethodGetById", new { PaymentMethodId = paymentMethodId }, cancellationToken);
    }

    public Task<int> CreateAsync(PaymentMethodCreateModel model, CancellationToken cancellationToken = default)
    {
        SalesGuard.Required(model.PaymentMethodCode, nameof(model.PaymentMethodCode));
        SalesGuard.Required(model.PaymentMethodName, nameof(model.PaymentMethodName));
        SalesGuard.Positive(model.CreatedByUserId, nameof(model.CreatedByUserId));
        return _accessService.QuerySingleAsync<int, object>("dbo.spPaymentMethodCreate", new
        {
            PaymentMethodCode = model.PaymentMethodCode.Trim(),
            PaymentMethodName = model.PaymentMethodName.Trim(),
            Description = SalesGuard.TrimOrEmpty(model.Description),
            model.RequireReferenceNo,
            model.IsCash,
            model.DisplayOrder,
            model.CreatedByUserId
        }, cancellationToken);
    }

    public Task UpdateAsync(PaymentMethodUpdateModel model, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(model.PaymentMethodId, nameof(model.PaymentMethodId));
        SalesGuard.Required(model.PaymentMethodCode, nameof(model.PaymentMethodCode));
        SalesGuard.Required(model.PaymentMethodName, nameof(model.PaymentMethodName));
        SalesGuard.Positive(model.UpdatedByUserId, nameof(model.UpdatedByUserId));
        return _accessService.ExecuteAsync("dbo.spPaymentMethodUpdate", new
        {
            model.PaymentMethodId,
            PaymentMethodCode = model.PaymentMethodCode.Trim(),
            PaymentMethodName = model.PaymentMethodName.Trim(),
            Description = SalesGuard.TrimOrEmpty(model.Description),
            model.RequireReferenceNo,
            model.IsCash,
            model.IsActive,
            model.DisplayOrder,
            model.UpdatedByUserId
        }, cancellationToken);
    }

    public Task ToggleActiveAsync(int paymentMethodId, bool isActive, int updatedByUserId, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(paymentMethodId, nameof(paymentMethodId));
        SalesGuard.Positive(updatedByUserId, nameof(updatedByUserId));
        return _accessService.ExecuteAsync("dbo.spPaymentMethodToggleActive", new { PaymentMethodId = paymentMethodId, IsActive = isActive, UpdatedByUserId = updatedByUserId }, cancellationToken);
    }

    public Task<bool> IsCodeExistsAsync(string paymentMethodCode, int? excludePaymentMethodId = null, CancellationToken cancellationToken = default)
    {
        SalesGuard.Required(paymentMethodCode, nameof(paymentMethodCode));
        return _accessService.QuerySingleAsync<bool, object>("dbo.spPaymentMethodCheckCodeExists", new { PaymentMethodCode = paymentMethodCode.Trim(), ExcludePaymentMethodId = excludePaymentMethodId }, cancellationToken);
    }
}

public sealed class SalesCheckoutService : ISalesCheckoutService
{
    private readonly IAccessService _accessService;

    public SalesCheckoutService(IAccessService accessService) => _accessService = accessService;

    public Task<SalesCheckoutProductModel?> GetProductByBarcodeAsync(string barcode, int locationId, CancellationToken cancellationToken = default)
    {
        SalesGuard.Required(barcode, nameof(barcode));
        SalesGuard.Positive(locationId, nameof(locationId));
        return _accessService.QuerySingleOrDefaultAsync<SalesCheckoutProductModel, object>("dbo.spSalesGetProductByBarcode", new { Barcode = barcode.Trim(), LocationId = locationId }, cancellationToken);
    }

    public async Task<IReadOnlyCollection<SalesCheckoutProductModel>> SearchProductsAsync(string? searchText, int locationId, int top = 20, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(locationId, nameof(locationId));
        if (top <= 0 || top > 100) throw new ArgumentException("Top must be between 1 and 100.", nameof(top));
        var rows = await _accessService.QueryAsync<SalesCheckoutProductModel, object>("dbo.spSalesSearchProducts", new { SearchText = SalesGuard.TrimOrNull(searchText), LocationId = locationId, Top = top }, cancellationToken);
        return rows.ToArray();
    }

    public Task<SalesCompleteResultModel> CompleteTransactionAsync(SalesCompleteRequestModel request, CancellationToken cancellationToken = default)
    {
        SalesGuard.ValidateSale(request);
        return _accessService.QuerySingleAsync<SalesCompleteResultModel, object>("dbo.spSalesCompleteTransaction", new
        {
            request.CustomerId,
            request.CashierUserId,
            request.HeldSaleHeaderId,
            request.UseCustomerCredit,
            request.CustomerCreditAmount,
            request.OrderDiscountAmount,
            request.TaxAmount,
            Remark = SalesGuard.TrimOrEmpty(request.Remark),
            request.AllowNegativeStock,
            request.CreatedByUserId,
            ItemsJson = SalesJson.Serialize(request.Items),
            PaymentsJson = SalesJson.Serialize(request.Payments)
        }, cancellationToken);
    }
}

public sealed class SalesHistoryService : ISalesHistoryService
{
    private readonly IAccessService _accessService;

    public SalesHistoryService(IAccessService accessService) => _accessService = accessService;

    public async Task<SalesPagedResultModel> GetPagedAsync(SalesPagedRequestModel request, CancellationToken cancellationToken = default)
    {
        SalesGuard.ValidPage(request.PageNumber, request.PageSize);
        var rows = (await _accessService.QueryAsync<SalesPagedRow, object>("dbo.spSalesGetPaged", new
        {
            request.PageNumber,
            request.PageSize,
            SearchText = SalesGuard.TrimOrNull(request.SearchText),
            request.CustomerId,
            request.CashierUserId,
            Status = SalesGuard.TrimOrNull(request.Status),
            request.FromDate,
            request.ToDate
        }, cancellationToken)).ToArray();

        return new SalesPagedResultModel { Sales = rows, TotalCount = rows.FirstOrDefault()?.TotalCount ?? 0, PageNumber = request.PageNumber, PageSize = request.PageSize };
    }

    public Task<SalesDetailModel?> GetDetailAsync(long salesHeaderId, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(salesHeaderId, nameof(salesHeaderId));
        return _accessService.QueryMultipleAsync<object, SalesDetailModel?>("dbo.spSalesGetDetail", new { SalesHeaderId = salesHeaderId }, ToSalesDetailAsync, cancellationToken);
    }

    public Task<SalesHeaderModel?> GetBySaleNoAsync(string saleNo, CancellationToken cancellationToken = default)
    {
        SalesGuard.Required(saleNo, nameof(saleNo));
        return _accessService.QuerySingleOrDefaultAsync<SalesHeaderModel, object>("dbo.spSalesGetBySaleNo", new { SaleNo = saleNo.Trim() }, cancellationToken);
    }

    public async Task<IReadOnlyCollection<SalesItemModel>> GetItemsAsync(long salesHeaderId, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(salesHeaderId, nameof(salesHeaderId));
        var rows = await _accessService.QueryAsync<SalesItemModel, object>("dbo.spSalesGetItemsBySalesHeaderId", new { SalesHeaderId = salesHeaderId }, cancellationToken);
        return rows.ToArray();
    }

    public async Task<IReadOnlyCollection<SalesPaymentModel>> GetPaymentsAsync(long salesHeaderId, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(salesHeaderId, nameof(salesHeaderId));
        var rows = await _accessService.QueryAsync<SalesPaymentModel, object>("dbo.spSalesGetPaymentsBySalesHeaderId", new { SalesHeaderId = salesHeaderId }, cancellationToken);
        return rows.ToArray();
    }

    public async Task<IReadOnlyCollection<SalesSummaryModel>> GetSummaryByDateRangeAsync(DateTime fromDate, DateTime toDate, int? cashierUserId = null, CancellationToken cancellationToken = default)
    {
        SalesGuard.ValidDateRange(fromDate, toDate);
        var rows = await _accessService.QueryAsync<SalesSummaryModel, object>("dbo.spSalesGetSummaryByDateRange", new { FromDate = fromDate, ToDate = toDate, CashierUserId = cashierUserId }, cancellationToken);
        return rows.ToArray();
    }

    public async Task<IReadOnlyCollection<SalesVatBillReportModel>> GetVatBillReportAsync(DateTime fromDate, DateTime toDate, int? cashierUserId = null, CancellationToken cancellationToken = default)
    {
        SalesGuard.ValidDateRange(fromDate, toDate);
        var rows = await _accessService.QueryAsync<SalesVatBillReportModel, object>("dbo.spSalesVatBillReportGet", new { FromDate = fromDate, ToDate = toDate, CashierUserId = cashierUserId }, cancellationToken);
        return rows.ToArray();
    }

    public Task VoidAsync(SalesVoidModel model, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(model.SalesHeaderId, nameof(model.SalesHeaderId));
        SalesGuard.Required(model.Reason, nameof(model.Reason));
        SalesGuard.Positive(model.UpdatedByUserId, nameof(model.UpdatedByUserId));
        return _accessService.ExecuteAsync("dbo.spSalesVoid", new { model.SalesHeaderId, Reason = model.Reason.Trim(), model.UpdatedByUserId, model.ReverseInventory }, cancellationToken);
    }

    public Task<ReceiptPrintHistoryModel> ReprintReceiptAsync(long salesHeaderId, int printedByUserId, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(salesHeaderId, nameof(salesHeaderId));
        SalesGuard.Positive(printedByUserId, nameof(printedByUserId));
        return _accessService.QuerySingleAsync<ReceiptPrintHistoryModel, object>("dbo.spSalesReprintReceipt", new { SalesHeaderId = salesHeaderId, PrintedByUserId = printedByUserId }, cancellationToken);
    }

    internal static async Task<SalesDetailModel?> ToSalesDetailAsync(SqlMapper.GridReader reader)
    {
        var header = await reader.ReadSingleOrDefaultAsync<SalesDetailRow>();
        if (header is null) return null;
        var items = (await reader.ReadAsync<SalesItemModel>()).ToArray();
        var payments = (await reader.ReadAsync<SalesPaymentModel>()).ToArray();
        return header.ToDetail(items, payments);
    }
}

public sealed class SalesClosingService : ISalesClosingService
{
    private readonly IAccessService _accessService;

    public SalesClosingService(IAccessService accessService) => _accessService = accessService;

    public Task<DailySalesClosingModel> GetDailyClosingAsync(DateTime closingDate, int? cashierUserId = null, CancellationToken cancellationToken = default)
    {
        return _accessService.QuerySingleAsync<DailySalesClosingModel, object>(
            "dbo.spSalesDailyClosingGet",
            new { ClosingDate = closingDate.Date, CashierUserId = cashierUserId },
            cancellationToken);
    }

    public Task<DailySalesClosingModel> SaveDailyClosingAsync(DailySalesClosingSaveModel model, CancellationToken cancellationToken = default)
    {
        if (model.ActualCashAmount < 0) throw new ArgumentException("Actual cash amount cannot be negative.", nameof(model.ActualCashAmount));
        SalesGuard.Positive(model.ClosedByUserId, nameof(model.ClosedByUserId));

        return _accessService.QuerySingleAsync<DailySalesClosingModel, object>(
            "dbo.spSalesDailyClosingSave",
            new
            {
                ClosingDate = model.ClosingDate.Date,
                model.CashierUserId,
                model.ActualCashAmount,
                Notes = SalesGuard.TrimOrEmpty(model.Notes),
                model.ClosedByUserId
            },
            cancellationToken);
    }
}

public sealed class HeldSaleService : IHeldSaleService
{
    private readonly IAccessService _accessService;
    private readonly ISalesCheckoutService _checkoutService;

    public HeldSaleService(IAccessService accessService, ISalesCheckoutService checkoutService)
    {
        _accessService = accessService;
        _checkoutService = checkoutService;
    }

    public async Task<HeldSalePagedResultModel> GetPagedAsync(HeldSalePagedRequestModel request, CancellationToken cancellationToken = default)
    {
        SalesGuard.ValidPage(request.PageNumber, request.PageSize);
        var rows = (await _accessService.QueryAsync<HeldSalePagedRow, object>("dbo.spHeldSaleGetPaged", new
        {
            request.PageNumber,
            request.PageSize,
            SearchText = SalesGuard.TrimOrNull(request.SearchText),
            request.CashierUserId,
            Status = SalesGuard.TrimOrNull(request.Status),
            request.FromDate,
            request.ToDate
        }, cancellationToken)).ToArray();
        return new HeldSalePagedResultModel { HeldSales = rows, TotalCount = rows.FirstOrDefault()?.TotalCount ?? 0, PageNumber = request.PageNumber, PageSize = request.PageSize };
    }

    public Task<HeldSaleDetailModel?> GetByIdAsync(long heldSaleHeaderId, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(heldSaleHeaderId, nameof(heldSaleHeaderId));
        return _accessService.QueryMultipleAsync<object, HeldSaleDetailModel?>("dbo.spHeldSaleGetById", new { HeldSaleHeaderId = heldSaleHeaderId }, ToHeldSaleDetailAsync, cancellationToken);
    }

    public Task<long> CreateAsync(HeldSaleCreateModel model, CancellationToken cancellationToken = default)
    {
        SalesGuard.ValidateHeldSale(model);
        return _accessService.QuerySingleAsync<long, object>("dbo.spHeldSaleCreate", new
        {
            model.CustomerId,
            model.CashierUserId,
            Note = SalesGuard.TrimOrEmpty(model.Note),
            model.EstimatedTaxAmount,
            model.CreatedByUserId,
            ItemsJson = SalesJson.Serialize(model.Items)
        }, cancellationToken);
    }

    public Task<HeldSaleDetailModel?> ResumeAsync(HeldSaleResumeModel model, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(model.HeldSaleHeaderId, nameof(model.HeldSaleHeaderId));
        SalesGuard.Positive(model.UpdatedByUserId, nameof(model.UpdatedByUserId));
        return _accessService.QueryMultipleAsync<object, HeldSaleDetailModel?>("dbo.spHeldSaleResume", new { model.HeldSaleHeaderId, model.UpdatedByUserId }, ToHeldSaleDetailAsync, cancellationToken);
    }

    public Task CancelAsync(HeldSaleCancelModel model, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(model.HeldSaleHeaderId, nameof(model.HeldSaleHeaderId));
        SalesGuard.Required(model.Reason, nameof(model.Reason));
        SalesGuard.Positive(model.UpdatedByUserId, nameof(model.UpdatedByUserId));
        return _accessService.ExecuteAsync("dbo.spHeldSaleCancel", new { model.HeldSaleHeaderId, Reason = model.Reason.Trim(), model.UpdatedByUserId }, cancellationToken);
    }

    public Task<int> ExpireOldAsync(DateTime expireBeforeDate, int updatedByUserId, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(updatedByUserId, nameof(updatedByUserId));
        return _accessService.QuerySingleAsync<int, object>("dbo.spHeldSaleExpireOld", new { ExpireBeforeDate = expireBeforeDate, UpdatedByUserId = updatedByUserId }, cancellationToken);
    }

    public Task<SalesCompleteResultModel> CompleteAsync(SalesCompleteRequestModel request, CancellationToken cancellationToken = default)
    {
        if (!request.HeldSaleHeaderId.HasValue) throw new ArgumentException("HeldSaleHeaderId is required.", nameof(request.HeldSaleHeaderId));
        return _checkoutService.CompleteTransactionAsync(request, cancellationToken);
    }

    private static async Task<HeldSaleDetailModel?> ToHeldSaleDetailAsync(SqlMapper.GridReader reader)
    {
        var header = await reader.ReadSingleOrDefaultAsync<HeldSaleDetailRow>();
        if (header is null) return null;
        var items = (await reader.ReadAsync<HeldSaleItemModel>()).ToArray();
        return header.ToDetail(items);
    }
}

public sealed class SalesReturnService : ISalesReturnService
{
    private readonly IAccessService _accessService;

    public SalesReturnService(IAccessService accessService) => _accessService = accessService;

    public async Task<SalesReturnPagedResultModel> GetPagedAsync(SalesReturnPagedRequestModel request, CancellationToken cancellationToken = default)
    {
        SalesGuard.ValidPage(request.PageNumber, request.PageSize);
        var rows = (await _accessService.QueryAsync<SalesReturnPagedRow, object>("dbo.spSalesReturnGetPaged", new
        {
            request.PageNumber,
            request.PageSize,
            SearchText = SalesGuard.TrimOrNull(request.SearchText),
            request.SalesHeaderId,
            request.CustomerId,
            request.CashierUserId,
            Status = SalesGuard.TrimOrNull(request.Status),
            request.FromDate,
            request.ToDate
        }, cancellationToken)).ToArray();
        return new SalesReturnPagedResultModel { Returns = rows, TotalCount = rows.FirstOrDefault()?.TotalCount ?? 0, PageNumber = request.PageNumber, PageSize = request.PageSize };
    }

    public Task<SalesReturnDetailModel?> GetByIdAsync(long salesReturnHeaderId, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(salesReturnHeaderId, nameof(salesReturnHeaderId));
        return _accessService.QueryMultipleAsync<object, SalesReturnDetailModel?>("dbo.spSalesReturnGetById", new { SalesReturnHeaderId = salesReturnHeaderId }, ToReturnDetailAsync, cancellationToken);
    }

    public Task<long> CreateAsync(SalesReturnCreateModel model, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(model.SalesHeaderId, nameof(model.SalesHeaderId));
        SalesGuard.Positive(model.CashierUserId, nameof(model.CashierUserId));
        SalesGuard.Required(model.Reason, nameof(model.Reason));
        SalesGuard.Positive(model.CreatedByUserId, nameof(model.CreatedByUserId));
        return _accessService.QuerySingleAsync<long, object>("dbo.spSalesReturnCreate", new { model.SalesHeaderId, model.CashierUserId, Reason = model.Reason.Trim(), model.CreatedByUserId }, cancellationToken);
    }

    public Task<long> AddItemAsync(SalesReturnItemCreateModel model, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(model.SalesReturnHeaderId, nameof(model.SalesReturnHeaderId));
        SalesGuard.Positive(model.SalesItemId, nameof(model.SalesItemId));
        SalesGuard.Positive(model.QuantityReturned, nameof(model.QuantityReturned));
        SalesGuard.NonNegative(model.RefundUnitPrice, nameof(model.RefundUnitPrice));
        SalesGuard.ValidateReturnCondition(model.ReturnCondition, model.ReturnToStock);
        SalesGuard.Required(model.Reason, nameof(model.Reason));
        return _accessService.QuerySingleAsync<long, object>("dbo.spSalesReturnAddItem", new
        {
            model.SalesReturnHeaderId,
            model.SalesItemId,
            model.QuantityReturned,
            model.RefundUnitPrice,
            model.ReturnToStock,
            ReturnCondition = model.ReturnCondition.Trim(),
            Reason = model.Reason.Trim()
        }, cancellationToken);
    }

    public Task ApproveAsync(SalesReturnApproveModel model, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(model.SalesReturnHeaderId, nameof(model.SalesReturnHeaderId));
        SalesGuard.Positive(model.ApprovedByUserId, nameof(model.ApprovedByUserId));
        return _accessService.ExecuteAsync("dbo.spSalesReturnApprove", new { model.SalesReturnHeaderId, model.ApprovedByUserId }, cancellationToken);
    }

    public Task RejectAsync(SalesReturnRejectModel model, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(model.SalesReturnHeaderId, nameof(model.SalesReturnHeaderId));
        SalesGuard.Required(model.Reason, nameof(model.Reason));
        SalesGuard.Positive(model.UpdatedByUserId, nameof(model.UpdatedByUserId));
        return _accessService.ExecuteAsync("dbo.spSalesReturnReject", new { model.SalesReturnHeaderId, Reason = model.Reason.Trim(), model.UpdatedByUserId }, cancellationToken);
    }

    public Task<SalesReturnHeaderModel> CompleteAsync(SalesReturnCompleteModel model, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(model.SalesReturnHeaderId, nameof(model.SalesReturnHeaderId));
        SalesGuard.Positive(model.CompletedByUserId, nameof(model.CompletedByUserId));
        SalesGuard.ValidateRefundPayments(model.Payments);
        return _accessService.QuerySingleAsync<SalesReturnHeaderModel, object>("dbo.spSalesReturnComplete", new
        {
            model.SalesReturnHeaderId,
            model.CompletedByUserId,
            PaymentsJson = SalesJson.Serialize(model.Payments)
        }, cancellationToken);
    }

    public Task CancelAsync(SalesReturnCancelModel model, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(model.SalesReturnHeaderId, nameof(model.SalesReturnHeaderId));
        SalesGuard.Required(model.Reason, nameof(model.Reason));
        SalesGuard.Positive(model.UpdatedByUserId, nameof(model.UpdatedByUserId));
        return _accessService.ExecuteAsync("dbo.spSalesReturnCancel", new { model.SalesReturnHeaderId, Reason = model.Reason.Trim(), model.UpdatedByUserId }, cancellationToken);
    }

    public async Task<IReadOnlyCollection<SalesReturnItemModel>> GetItemsAsync(long salesReturnHeaderId, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(salesReturnHeaderId, nameof(salesReturnHeaderId));
        var rows = await _accessService.QueryAsync<SalesReturnItemModel, object>("dbo.spSalesReturnGetItemsByReturnId", new { SalesReturnHeaderId = salesReturnHeaderId }, cancellationToken);
        return rows.ToArray();
    }

    public async Task<IReadOnlyCollection<SalesReturnPaymentModel>> GetPaymentsAsync(long salesReturnHeaderId, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(salesReturnHeaderId, nameof(salesReturnHeaderId));
        var rows = await _accessService.QueryAsync<SalesReturnPaymentModel, object>("dbo.spSalesReturnGetPaymentsByReturnId", new { SalesReturnHeaderId = salesReturnHeaderId }, cancellationToken);
        return rows.ToArray();
    }

    private static async Task<SalesReturnDetailModel?> ToReturnDetailAsync(SqlMapper.GridReader reader)
    {
        var header = await reader.ReadSingleOrDefaultAsync<SalesReturnDetailRow>();
        if (header is null) return null;
        var items = (await reader.ReadAsync<SalesReturnItemModel>()).ToArray();
        var payments = (await reader.ReadAsync<SalesReturnPaymentModel>()).ToArray();
        return header.ToDetail(items, payments);
    }
}

public sealed class ReceiptService : IReceiptService
{
    private readonly IAccessService _accessService;

    public ReceiptService(IAccessService accessService) => _accessService = accessService;

    public Task<long> CreatePrintHistoryAsync(ReceiptPrintHistoryCreateModel model, CancellationToken cancellationToken = default)
    {
        SalesGuard.ValidateReceipt(model);
        return _accessService.QuerySingleAsync<long, object>("dbo.spReceiptPrintHistoryCreate", new
        {
            model.SalesHeaderId,
            model.SalesReturnHeaderId,
            ReceiptNo = model.ReceiptNo.Trim(),
            ReceiptType = model.ReceiptType.Trim(),
            model.PrintedByUserId
        }, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ReceiptPrintHistoryModel>> GetBySalesHeaderIdAsync(long salesHeaderId, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(salesHeaderId, nameof(salesHeaderId));
        var rows = await _accessService.QueryAsync<ReceiptPrintHistoryModel, object>("dbo.spReceiptPrintHistoryGetBySalesHeaderId", new { SalesHeaderId = salesHeaderId }, cancellationToken);
        return rows.ToArray();
    }

    public async Task<IReadOnlyCollection<ReceiptPrintHistoryModel>> GetBySalesReturnHeaderIdAsync(long salesReturnHeaderId, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(salesReturnHeaderId, nameof(salesReturnHeaderId));
        var rows = await _accessService.QueryAsync<ReceiptPrintHistoryModel, object>("dbo.spReceiptPrintHistoryGetBySalesReturnHeaderId", new { SalesReturnHeaderId = salesReturnHeaderId }, cancellationToken);
        return rows.ToArray();
    }

    public Task<SalesDocumentModel> IssueSalesDocumentAsync(SalesDocumentIssueModel model, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(model.SalesHeaderId, nameof(model.SalesHeaderId));
        SalesGuard.Required(model.DocumentType, nameof(model.DocumentType));
        SalesGuard.Positive(model.IssuedByUserId, nameof(model.IssuedByUserId));
        return _accessService.QuerySingleAsync<SalesDocumentModel, object>("dbo.spSalesDocumentIssue", new
        {
            model.SalesHeaderId,
            DocumentType = model.DocumentType.Trim(),
            CustomerName = SalesGuard.TrimOrEmpty(model.CustomerName),
            CustomerTaxId = SalesGuard.TrimOrEmpty(model.CustomerTaxId),
            CustomerBranch = SalesGuard.TrimOrEmpty(model.CustomerBranch),
            CustomerAddress = SalesGuard.TrimOrEmpty(model.CustomerAddress),
            model.IssuedByUserId
        }, cancellationToken);
    }

    public async Task<IReadOnlyCollection<SalesDocumentModel>> GetSalesDocumentsAsync(long salesHeaderId, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(salesHeaderId, nameof(salesHeaderId));
        var rows = await _accessService.QueryAsync<SalesDocumentModel, object>("dbo.spSalesDocumentGetBySalesHeaderId", new { SalesHeaderId = salesHeaderId }, cancellationToken);
        return rows.ToArray();
    }

    public Task<SalesDocumentModel> RecordSalesDocumentPrintAsync(long salesDocumentId, int printedByUserId, CancellationToken cancellationToken = default)
    {
        SalesGuard.Positive(salesDocumentId, nameof(salesDocumentId));
        SalesGuard.Positive(printedByUserId, nameof(printedByUserId));
        return _accessService.QuerySingleAsync<SalesDocumentModel, object>("dbo.spSalesDocumentRecordPrint", new { SalesDocumentId = salesDocumentId, PrintedByUserId = printedByUserId }, cancellationToken);
    }
}

internal sealed class SalesPagedRow : SalesHeaderModel { public int TotalCount { get; init; } }
internal sealed class HeldSalePagedRow : HeldSaleHeaderModel { public int TotalCount { get; init; } }
internal sealed class SalesReturnPagedRow : SalesReturnHeaderModel { public int TotalCount { get; init; } }

internal sealed class SalesDetailRow : SalesHeaderModel
{
    public SalesDetailModel ToDetail(IReadOnlyCollection<SalesItemModel> items, IReadOnlyCollection<SalesPaymentModel> payments) => new()
    {
        SalesHeaderId = SalesHeaderId,
        SaleNo = SaleNo,
        SaleDate = SaleDate,
        CustomerId = CustomerId,
        CustomerName = CustomerName,
        CashierUserId = CashierUserId,
        CashierName = CashierName,
        SubtotalAmount = SubtotalAmount,
        ItemDiscountAmount = ItemDiscountAmount,
        OrderDiscountAmount = OrderDiscountAmount,
        TotalDiscountAmount = TotalDiscountAmount,
        TaxAmount = TaxAmount,
        NetAmount = NetAmount,
        PaidAmount = PaidAmount,
        ChangeAmount = ChangeAmount,
        Status = Status,
        Remark = Remark,
        CreatedByUserId = CreatedByUserId,
        UpdatedByUserId = UpdatedByUserId,
        CreatedDate = CreatedDate,
        UpdatedDate = UpdatedDate,
        Items = items,
        Payments = payments
    };
}

internal sealed class HeldSaleDetailRow : HeldSaleHeaderModel
{
    public HeldSaleDetailModel ToDetail(IReadOnlyCollection<HeldSaleItemModel> items) => new()
    {
        HeldSaleHeaderId = HeldSaleHeaderId,
        HeldSaleNo = HeldSaleNo,
        HeldDate = HeldDate,
        CustomerId = CustomerId,
        CustomerName = CustomerName,
        CashierUserId = CashierUserId,
        CashierName = CashierName,
        Note = Note,
        EstimatedSubtotalAmount = EstimatedSubtotalAmount,
        EstimatedDiscountAmount = EstimatedDiscountAmount,
        EstimatedTaxAmount = EstimatedTaxAmount,
        EstimatedNetAmount = EstimatedNetAmount,
        Status = Status,
        CreatedByUserId = CreatedByUserId,
        UpdatedByUserId = UpdatedByUserId,
        CreatedDate = CreatedDate,
        UpdatedDate = UpdatedDate,
        Items = items
    };
}

internal sealed class SalesReturnDetailRow : SalesReturnHeaderModel
{
    public SalesReturnDetailModel ToDetail(IReadOnlyCollection<SalesReturnItemModel> items, IReadOnlyCollection<SalesReturnPaymentModel> payments) => new()
    {
        SalesReturnHeaderId = SalesReturnHeaderId,
        ReturnNo = ReturnNo,
        SalesHeaderId = SalesHeaderId,
        SaleNo = SaleNo,
        ReturnDate = ReturnDate,
        CustomerId = CustomerId,
        CashierUserId = CashierUserId,
        RefundSubtotalAmount = RefundSubtotalAmount,
        RefundDiscountAmount = RefundDiscountAmount,
        RefundTaxAmount = RefundTaxAmount,
        RefundNetAmount = RefundNetAmount,
        Reason = Reason,
        Status = Status,
        ApprovedByUserId = ApprovedByUserId,
        ApprovedDate = ApprovedDate,
        CreatedByUserId = CreatedByUserId,
        UpdatedByUserId = UpdatedByUserId,
        CreatedDate = CreatedDate,
        UpdatedDate = UpdatedDate,
        Items = items,
        Payments = payments
    };
}

internal static class SalesJson
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static string Serialize<T>(IReadOnlyCollection<T> value) => JsonSerializer.Serialize(value, Options);
}

internal static class SalesGuard
{
    public static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    public static string TrimOrEmpty(string? value) => value?.Trim() ?? string.Empty;

    public static void Required(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{name} is required.", name);
    }

    public static void Positive(int value, string name)
    {
        if (value <= 0) throw new ArgumentException($"{name} must be greater than zero.", name);
    }

    public static void Positive(long value, string name)
    {
        if (value <= 0) throw new ArgumentException($"{name} must be greater than zero.", name);
    }

    public static void Positive(decimal value, string name)
    {
        if (value <= 0) throw new ArgumentException($"{name} must be greater than zero.", name);
    }

    public static void NonNegative(decimal value, string name)
    {
        if (value < 0) throw new ArgumentException($"{name} must be zero or greater.", name);
    }

    public static void ValidPage(int pageNumber, int pageSize)
    {
        if (pageNumber <= 0) throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));
        if (pageSize <= 0 || pageSize > 500) throw new ArgumentException("Page size must be between 1 and 500.", nameof(pageSize));
    }

    public static void ValidDateRange(DateTime fromDate, DateTime toDate)
    {
        if (toDate < fromDate) throw new ArgumentException("To date must be greater than or equal to from date.", nameof(toDate));
    }

    public static void ValidateSale(SalesCompleteRequestModel request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Positive(request.CashierUserId, nameof(request.CashierUserId));
        Positive(request.CreatedByUserId, nameof(request.CreatedByUserId));
        NonNegative(request.CustomerCreditAmount, nameof(request.CustomerCreditAmount));
        NonNegative(request.OrderDiscountAmount, nameof(request.OrderDiscountAmount));
        NonNegative(request.TaxAmount, nameof(request.TaxAmount));
        if (request.Items.Count == 0) throw new InvalidOperationException("Sale must have at least one item.");
        if (request.Payments.Count == 0 && !request.UseCustomerCredit) throw new InvalidOperationException("Sale must have at least one payment.");

        var subtotal = 0m;
        var itemDiscount = 0m;
        foreach (var item in request.Items)
        {
            Positive(item.ProductId, nameof(item.ProductId));
            Positive(item.LocationId, nameof(item.LocationId));
            Positive(item.Quantity, nameof(item.Quantity));
            NonNegative(item.UnitPrice, nameof(item.UnitPrice));
            NonNegative(item.ItemDiscountAmount, nameof(item.ItemDiscountAmount));
            NonNegative(item.TaxAmount, nameof(item.TaxAmount));
            var lineSubtotal = item.Quantity * item.UnitPrice;
            if (item.ItemDiscountAmount > lineSubtotal) throw new InvalidOperationException("Item discount cannot exceed line subtotal.");
            subtotal += lineSubtotal;
            itemDiscount += item.ItemDiscountAmount;
        }

        var netAmount = subtotal - itemDiscount - request.OrderDiscountAmount + request.TaxAmount;
        if (netAmount < 0) throw new InvalidOperationException("Net amount cannot be negative.");
        var creditAmount = request.UseCustomerCredit && request.CustomerCreditAmount <= 0 ? netAmount : request.CustomerCreditAmount;
        if (request.Payments.Sum(payment => payment.PaymentAmount) + creditAmount < netAmount) throw new InvalidOperationException("Total payment amount must be greater than or equal to net amount.");
        foreach (var payment in request.Payments) Positive(payment.PaymentAmount, nameof(payment.PaymentAmount));
    }

    public static void ValidateHeldSale(HeldSaleCreateModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        Positive(model.CashierUserId, nameof(model.CashierUserId));
        Positive(model.CreatedByUserId, nameof(model.CreatedByUserId));
        NonNegative(model.EstimatedTaxAmount, nameof(model.EstimatedTaxAmount));
        if (model.Items.Count == 0) throw new InvalidOperationException("Held sale must have at least one item.");
        foreach (var item in model.Items)
        {
            Positive(item.ProductId, nameof(item.ProductId));
            Positive(item.Quantity, nameof(item.Quantity));
            NonNegative(item.UnitPrice, nameof(item.UnitPrice));
            NonNegative(item.ItemDiscountAmount, nameof(item.ItemDiscountAmount));
            NonNegative(item.TaxAmount, nameof(item.TaxAmount));
        }
    }

    public static void ValidateReturnCondition(string returnCondition, bool returnToStock)
    {
        Required(returnCondition, nameof(returnCondition));
        var isGood = StringComparer.OrdinalIgnoreCase.Equals(returnCondition.Trim(), "Good");
        if (returnToStock && !isGood) throw new InvalidOperationException("Return to stock is allowed only when item condition is Good.");
    }

    public static void ValidateRefundPayments(IReadOnlyCollection<SalesReturnPaymentInputModel> payments)
    {
        if (payments.Count == 0) throw new InvalidOperationException("Refund must have at least one refund payment.");
        foreach (var payment in payments)
        {
            Positive(payment.PaymentMethodId, nameof(payment.PaymentMethodId));
            Positive(payment.RefundAmount, nameof(payment.RefundAmount));
        }
    }

    public static void ValidateReceipt(ReceiptPrintHistoryCreateModel model)
    {
        if (model.SalesHeaderId.HasValue == model.SalesReturnHeaderId.HasValue) throw new InvalidOperationException("Exactly one receipt parent is required.");
        Required(model.ReceiptNo, nameof(model.ReceiptNo));
        Required(model.ReceiptType, nameof(model.ReceiptType));
        Positive(model.PrintedByUserId, nameof(model.PrintedByUserId));
    }
}
