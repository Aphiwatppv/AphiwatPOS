using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SalesEngine.Models;
using SalesEngine.Services;

namespace AphiwatPOS.Pages.Sales.ReturnsRefunds;

public sealed class IndexModel : PageModel
{
    private readonly ISalesHistoryService _salesHistoryService;
    private readonly ISalesReturnService _salesReturnService;
    private readonly IPaymentMethodService _paymentMethodService;

    public IndexModel(ISalesHistoryService salesHistoryService, ISalesReturnService salesReturnService, IPaymentMethodService paymentMethodService)
    {
        _salesHistoryService = salesHistoryService;
        _salesReturnService = salesReturnService;
        _paymentMethodService = paymentMethodService;
    }

    [BindProperty(SupportsGet = true)] public string? SearchSaleNo { get; set; }
    [BindProperty(SupportsGet = true)] public string? SearchText { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    [BindProperty] public RefundInput Input { get; set; } = new();
    [BindProperty] public RefundActionInput ActionInput { get; set; } = new();

    public SalesDetailModel? OriginalSale { get; private set; }
    public SalesReturnPagedResultModel Returns { get; private set; } = new();
    public IReadOnlyCollection<PaymentMethodModel> PaymentMethods { get; private set; } = Array.Empty<PaymentMethodModel>();
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool CanApprove { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!SalesPageHelpers.HasPermission(User, "SALES_REFUND") && !User.IsInRole("Admin")) return RedirectToPage("/Account/AccessDenied");
        StatusMessage = TempData["StatusMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        await LoadAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostCreateRefundAsync(CancellationToken cancellationToken)
    {
        var userId = SalesPageHelpers.CurrentUserId(User);
        try
        {
            var selectedItems = Deserialize<RefundItemInput>(Input.ItemsJson).Where(item => item.QuantityReturned > 0).ToArray();
            if (!selectedItems.Any()) throw new InvalidOperationException("Select at least one item to refund.");
            var returnId = await _salesReturnService.CreateAsync(new SalesReturnCreateModel { SalesHeaderId = Input.SalesHeaderId, CashierUserId = userId, Reason = Input.Reason, CreatedByUserId = userId }, cancellationToken);
            foreach (var item in selectedItems)
            {
                await _salesReturnService.AddItemAsync(new SalesReturnItemCreateModel { SalesReturnHeaderId = returnId, SalesItemId = item.SalesItemId, QuantityReturned = item.QuantityReturned, RefundUnitPrice = item.RefundUnitPrice, ReturnToStock = item.ReturnToStock, ReturnCondition = item.ReturnCondition, Reason = Input.Reason }, cancellationToken);
            }
            TempData["StatusMessage"] = "Refund draft created.";
            return RedirectToPage(new { SearchText, Status, PageNumber });
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            await LoadAsync(cancellationToken);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostApproveRefundAsync(CancellationToken cancellationToken)
    {
        await _salesReturnService.ApproveAsync(new SalesReturnApproveModel { SalesReturnHeaderId = ActionInput.SalesReturnHeaderId, ApprovedByUserId = SalesPageHelpers.CurrentUserId(User) }, cancellationToken);
        TempData["StatusMessage"] = "Refund approved.";
        return RedirectToPage(new { SearchText, Status, PageNumber });
    }

    public async Task<IActionResult> OnPostRejectRefundAsync(CancellationToken cancellationToken)
    {
        await _salesReturnService.RejectAsync(new SalesReturnRejectModel { SalesReturnHeaderId = ActionInput.SalesReturnHeaderId, Reason = ActionInput.Reason, UpdatedByUserId = SalesPageHelpers.CurrentUserId(User) }, cancellationToken);
        TempData["StatusMessage"] = "Refund rejected.";
        return RedirectToPage(new { SearchText, Status, PageNumber });
    }

    public async Task<IActionResult> OnPostCompleteRefundAsync(CancellationToken cancellationToken)
    {
        var payments = Deserialize<SalesReturnPaymentInput>(ActionInput.PaymentsJson).Select(SalesPageHelpers.ToReturnPayment).ToArray();
        await _salesReturnService.CompleteAsync(new SalesReturnCompleteModel { SalesReturnHeaderId = ActionInput.SalesReturnHeaderId, CompletedByUserId = SalesPageHelpers.CurrentUserId(User), Payments = payments }, cancellationToken);
        TempData["StatusMessage"] = "Refund completed. Stock return rules were handled by inventory movements.";
        return RedirectToPage(new { SearchText, Status, PageNumber });
    }

    public async Task<IActionResult> OnPostCancelRefundAsync(CancellationToken cancellationToken)
    {
        await _salesReturnService.CancelAsync(new SalesReturnCancelModel { SalesReturnHeaderId = ActionInput.SalesReturnHeaderId, Reason = ActionInput.Reason, UpdatedByUserId = SalesPageHelpers.CurrentUserId(User) }, cancellationToken);
        TempData["StatusMessage"] = "Refund cancelled.";
        return RedirectToPage(new { SearchText, Status, PageNumber });
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(SearchSaleNo))
        {
            var sale = await _salesHistoryService.GetBySaleNoAsync(SearchSaleNo.Trim(), cancellationToken);
            if (sale is not null) OriginalSale = await _salesHistoryService.GetDetailAsync(sale.SalesHeaderId, cancellationToken);
        }
        Returns = await _salesReturnService.GetPagedAsync(new SalesReturnPagedRequestModel { PageNumber = PageNumber, PageSize = 10, SearchText = SearchText, Status = Status }, cancellationToken);
        PaymentMethods = await _paymentMethodService.GetAllActiveAsync(cancellationToken);
        CanApprove = SalesPageHelpers.HasPermission(User, "SALES_REFUND_APPROVE") || User.IsInRole("Admin");
    }

    private static IReadOnlyCollection<T> Deserialize<T>(string? json) =>
        string.IsNullOrWhiteSpace(json) ? Array.Empty<T>() : JsonSerializer.Deserialize<T[]>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? Array.Empty<T>();

    public sealed class RefundInput
    {
        public long SalesHeaderId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string ItemsJson { get; set; } = "[]";
    }

    public sealed class RefundItemInput
    {
        public long SalesItemId { get; set; }
        public decimal QuantityReturned { get; set; }
        public decimal RefundUnitPrice { get; set; }
        public bool ReturnToStock { get; set; }
        public string ReturnCondition { get; set; } = "Good";
    }

    public sealed class RefundActionInput
    {
        public long SalesReturnHeaderId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string PaymentsJson { get; set; } = "[]";
    }
}
