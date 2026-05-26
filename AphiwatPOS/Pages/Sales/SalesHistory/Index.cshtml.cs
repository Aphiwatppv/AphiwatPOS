using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SalesEngine.Models;
using SalesEngine.Services;

namespace AphiwatPOS.Pages.Sales.SalesHistory;

public sealed class IndexModel : PageModel
{
    private readonly ISalesHistoryService _salesHistoryService;
    private readonly IPaymentMethodService _paymentMethodService;
    private readonly IReceiptService _receiptService;

    public IndexModel(ISalesHistoryService salesHistoryService, IPaymentMethodService paymentMethodService, IReceiptService receiptService)
    {
        _salesHistoryService = salesHistoryService;
        _paymentMethodService = paymentMethodService;
        _receiptService = receiptService;
    }

    [BindProperty(SupportsGet = true)] public string? SearchText { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? FromDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? ToDate { get; set; }
    [BindProperty(SupportsGet = true)] public int? CashierUserId { get; set; }
    [BindProperty(SupportsGet = true)] public int? PaymentMethodId { get; set; }
    [BindProperty(SupportsGet = true)] public bool CustomerCreditOnly { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    [BindProperty] public SalesVoidInput VoidInput { get; set; } = new();
    [BindProperty] public SalesDocumentInput DocumentInput { get; set; } = new();

    public SalesPagedResultModel Sales { get; private set; } = new();
    public IReadOnlyCollection<SalesSummaryModel> Summary { get; private set; } = Array.Empty<SalesSummaryModel>();
    public IReadOnlyCollection<PaymentMethodModel> PaymentMethods { get; private set; } = Array.Empty<PaymentMethodModel>();
    public Dictionary<long, IReadOnlyCollection<SalesPaymentModel>> PaymentsBySale { get; } = new();
    public Dictionary<long, IReadOnlyCollection<SalesDocumentModel>> DocumentsBySale { get; } = new();
    public Dictionary<long, decimal> CreditUsedBySale { get; } = new();
    public decimal CustomerCreditSalesAmount => CreditUsedBySale.Values.Sum();
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool CanVoid { get; private set; }
    public bool CanRefund { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!SalesPageHelpers.HasSalesAccess(User)) return RedirectToPage("/Account/AccessDenied");
        StatusMessage = TempData["StatusMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        await LoadAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostVoidSaleAsync(CancellationToken cancellationToken)
    {
        if (!SalesPageHelpers.HasPermission(User, "SALES_VOID") && !User.IsInRole("Admin")) return RedirectToPage("/Account/AccessDenied");
        await _salesHistoryService.VoidAsync(new SalesVoidModel { SalesHeaderId = VoidInput.SalesHeaderId, Reason = VoidInput.Reason, UpdatedByUserId = SalesPageHelpers.CurrentUserId(User), ReverseInventory = true }, cancellationToken);
        TempData["StatusMessage"] = "Sale voided and inventory reversal was requested through movement logic.";
        return RedirectToPage(new { SearchText, FromDate, ToDate, CashierUserId, PaymentMethodId, Status, PageNumber });
    }

    public async Task<IActionResult> OnPostReprintReceiptAsync(long salesHeaderId, CancellationToken cancellationToken)
    {
        if (salesHeaderId <= 0)
        {
            TempData["ErrorMessage"] = "Unable to print receipt because the sale record was not found.";
            return RedirectToPage(new { SearchText, FromDate, ToDate, CashierUserId, PaymentMethodId, Status, PageNumber });
        }

        var document = await _receiptService.IssueSalesDocumentAsync(new SalesDocumentIssueModel { SalesHeaderId = salesHeaderId, DocumentType = "Receipt", IssuedByUserId = SalesPageHelpers.CurrentUserId(User) }, cancellationToken);
        await _receiptService.RecordSalesDocumentPrintAsync(document.SalesDocumentId, SalesPageHelpers.CurrentUserId(User), cancellationToken);
        TempData["StatusMessage"] = $"Receipt {document.DocumentNo} print recorded.";
        return RedirectToPage(new { SearchText, FromDate, ToDate, CashierUserId, PaymentMethodId, Status, PageNumber });
    }

    public async Task<IActionResult> OnPostIssueDocumentAsync(CancellationToken cancellationToken)
    {
        if (DocumentInput.SalesHeaderId <= 0)
        {
            TempData["ErrorMessage"] = "Unable to issue document because the sale record was not found.";
            return RedirectToPage(new { SearchText, FromDate, ToDate, CashierUserId, PaymentMethodId, Status, PageNumber });
        }

        var userId = SalesPageHelpers.CurrentUserId(User);
        var document = await _receiptService.IssueSalesDocumentAsync(new SalesDocumentIssueModel
        {
            SalesHeaderId = DocumentInput.SalesHeaderId,
            DocumentType = DocumentInput.DocumentType,
            CustomerName = DocumentInput.CustomerName,
            CustomerTaxId = DocumentInput.CustomerTaxId,
            CustomerBranch = DocumentInput.CustomerBranch,
            CustomerAddress = DocumentInput.CustomerAddress,
            IssuedByUserId = userId
        }, cancellationToken);
        await _receiptService.RecordSalesDocumentPrintAsync(document.SalesDocumentId, userId, cancellationToken);
        TempData["StatusMessage"] = $"{DocumentLabel(document.DocumentType)} {document.DocumentNo} issued and print recorded.";
        return RedirectToPage(new { SearchText, FromDate, ToDate, CashierUserId, PaymentMethodId, Status, PageNumber });
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Sales = await _salesHistoryService.GetPagedAsync(new SalesPagedRequestModel { PageNumber = PageNumber, PageSize = 10, SearchText = SearchText, CashierUserId = CashierUserId, Status = Status, FromDate = FromDate, ToDate = ToDate }, cancellationToken);
        PaymentMethods = await _paymentMethodService.GetAllActiveAsync(cancellationToken);
        var creditMethodId = PaymentMethods.FirstOrDefault(x => x.PaymentMethodCode.Equals("CREDIT", StringComparison.OrdinalIgnoreCase))?.PaymentMethodId;
        foreach (var sale in Sales.Sales)
        {
            var payments = await _salesHistoryService.GetPaymentsAsync(sale.SalesHeaderId, cancellationToken);
            PaymentsBySale[sale.SalesHeaderId] = payments;
            DocumentsBySale[sale.SalesHeaderId] = await _receiptService.GetSalesDocumentsAsync(sale.SalesHeaderId, cancellationToken);
            CreditUsedBySale[sale.SalesHeaderId] = creditMethodId.HasValue ? payments.Where(x => x.PaymentMethodId == creditMethodId.Value).Sum(x => x.PaymentAmount) : 0;
        }
        if (CustomerCreditOnly)
        {
            var rows = Sales.Sales.Where(x => CreditUsedBySale.GetValueOrDefault(x.SalesHeaderId) > 0).ToArray();
            Sales = new SalesPagedResultModel { Sales = rows, TotalCount = rows.Length, PageNumber = Sales.PageNumber, PageSize = Sales.PageSize };
        }
        Summary = await _salesHistoryService.GetSummaryByDateRangeAsync(FromDate ?? DateTime.Today.AddDays(-30), ToDate ?? DateTime.Today, CashierUserId, cancellationToken);
        CanVoid = SalesPageHelpers.HasPermission(User, "SALES_VOID") || User.IsInRole("Admin");
        CanRefund = SalesPageHelpers.HasPermission(User, "SALES_REFUND") || User.IsInRole("Admin");
    }

    public sealed class SalesVoidInput
    {
        public long SalesHeaderId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public sealed class SalesDocumentInput
    {
        public long SalesHeaderId { get; set; }
        public string DocumentType { get; set; } = "Receipt";
        public string? CustomerName { get; set; }
        public string? CustomerTaxId { get; set; }
        public string? CustomerBranch { get; set; }
        public string? CustomerAddress { get; set; }
    }

    public static string DocumentLabel(string documentType) => documentType switch
    {
        "Receipt" => "Receipt",
        "ShortTaxInvoice" => "Short Tax Invoice",
        "FullTaxInvoice" => "Full Tax Invoice",
        _ => documentType
    };
}
