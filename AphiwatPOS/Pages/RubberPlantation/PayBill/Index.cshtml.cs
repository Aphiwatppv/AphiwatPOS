using System.ComponentModel.DataAnnotations;
using AphiwatPOS.Pages.Customer;
using CustomerEngine.Models;
using CustomerEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AphiwatPOS.Pages.RubberPlantation.PayBill;

public sealed class IndexModel : PageModel
{
    private readonly IRubberPurchaseService _rubberPurchaseService;
    private readonly ICustomerCreditService _customerCreditService;

    public IndexModel(IRubberPurchaseService rubberPurchaseService, ICustomerCreditService customerCreditService)
    {
        _rubberPurchaseService = rubberPurchaseService;
        _customerCreditService = customerCreditService;
    }

    [BindProperty(SupportsGet = true)] public string? SearchText { get; set; }
    [BindProperty(SupportsGet = true)] public string? HistorySearchText { get; set; }
    [BindProperty(SupportsGet = true)] public string? HistoryPaymentMethod { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? HistoryDateFrom { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? HistoryDateTo { get; set; }
    [BindProperty(SupportsGet = true)] public long? SelectedPurchaseId { get; set; }
    [BindProperty(SupportsGet = true)] public long? ReceiptId { get; set; }
    [BindProperty(SupportsGet = true)] public bool PromptPrint { get; set; }
    [BindProperty] public PayBillInput Input { get; set; } = new();

    public IReadOnlyCollection<RubberPurchaseHeaderModel> DuePurchases { get; private set; } = Array.Empty<RubberPurchaseHeaderModel>();
    public IReadOnlyCollection<RubberPurchaseHeaderModel> PaidHistory { get; private set; } = Array.Empty<RubberPurchaseHeaderModel>();
    public RubberPurchaseHeaderModel? SelectedPurchase { get; private set; }
    public CustomerCreditModel? SelectedCustomerCredit { get; private set; }
    public RubberPurchaseHeaderModel? Receipt { get; private set; }
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public decimal TotalDueAmount => DuePurchases.Sum(GetRemainingAmount);
    public decimal SelectedCreditOutstandingAmount => Math.Max(0, SelectedCustomerCredit?.CurrentOutstandingAmount ?? 0);
    public bool SelectedCustomerHasOutstandingCredit => SelectedCreditOutstandingAmount > 0;
    public bool HasHistoryFilter =>
        !string.IsNullOrWhiteSpace(HistorySearchText)
        || !string.IsNullOrWhiteSpace(HistoryPaymentMethod)
        || HistoryDateFrom.HasValue
        || HistoryDateTo.HasValue;

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        StatusMessage = TempData["StatusMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        await LoadAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostPayAsync(CancellationToken cancellationToken)
    {
        ModelState.Remove("Input.PaidAmount");

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please review payment information.";
            await LoadAsync(cancellationToken);
            return Page();
        }

        var purchase = await _rubberPurchaseService.GetByIdAsync(Input.RubberPurchaseHeaderId, cancellationToken);
        if (purchase is null)
        {
            ErrorMessage = "Rubber bill was not found.";
            await LoadAsync(cancellationToken);
            return Page();
        }

        if (purchase.PaymentStatus is "Paid" or "Cancelled")
        {
            ErrorMessage = "This rubber bill is no longer available for payment.";
            await LoadAsync(cancellationToken);
            return Page();
        }

        var fullPaymentAmount = purchase.TotalAmount ?? 0;
        if (fullPaymentAmount <= 0)
        {
            ErrorMessage = "Rubber bill total amount must be greater than zero before payment.";
            await LoadAsync(cancellationToken);
            return Page();
        }

        var creditPaymentAmount = 0m;
        if (Input.PayCustomerCredit && purchase.CustomerId.HasValue)
        {
            var credit = await _customerCreditService.GetByCustomerIdAsync(purchase.CustomerId.Value, cancellationToken);
            creditPaymentAmount = Math.Min(fullPaymentAmount, Math.Max(0, credit?.CurrentOutstandingAmount ?? 0));
        }

        var paidToSellerAmount = fullPaymentAmount - creditPaymentAmount;
        Input.PaidAmount = paidToSellerAmount;
        var userId = CustomerPageHelpers.CurrentUserId(User);

        var receipt = await _rubberPurchaseService.PayBillAsync(new RubberPurchasePayBillModel
        {
            RubberPurchaseHeaderId = Input.RubberPurchaseHeaderId,
            PaidAmount = paidToSellerAmount,
            CreditDeductedAmount = creditPaymentAmount,
            PaymentMethod = Input.PaymentMethod,
            PaymentRemark = Input.PaymentRemark,
            UpdatedByUserId = userId
        }, cancellationToken);

        TempData["StatusMessage"] = creditPaymentAmount > 0
            ? $"Rubber bill payment saved successfully. Customer credit {creditPaymentAmount:N2} baht was deducted from the receipt."
            : "Rubber bill payment saved successfully.";
        return RedirectToPage(new
        {
            SearchText,
            HistorySearchText,
            HistoryPaymentMethod,
            HistoryDateFrom,
            HistoryDateTo,
            ReceiptId = receipt.RubberPurchaseHeaderId,
            PromptPrint = true
        });
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        var purchases = await _rubberPurchaseService.GetPagedAsync(new RubberPurchaseHeaderPagedRequestModel
        {
            PageNumber = 1,
            PageSize = 500,
            SearchText = SearchText
        }, cancellationToken);

        DuePurchases = purchases.Items
            .Where(x => x.PaymentStatus is "Pending")
            .OrderBy(x => x.TransactionDate)
            .ThenBy(x => x.RubberPurchaseHeaderId)
            .ToArray();

        var paidHistory = purchases.Items
            .Where(x => !string.IsNullOrWhiteSpace(x.ReceiptNo) || x.PaidAmount > 0 || x.PaymentStatus is "Paid")
            .ToArray();

        if (!string.IsNullOrWhiteSpace(HistorySearchText))
        {
            var historySearch = HistorySearchText.Trim();
            paidHistory = paidHistory
                .Where(x => Contains(x.ReceiptNo, historySearch)
                    || Contains(x.CustomerCode, historySearch)
                    || Contains(x.CustomerName, historySearch)
                    || Contains(x.PhoneNumber, historySearch)
                    || Contains(x.RubberAuctionLocationName, historySearch)
                    || Contains(x.LocationName, historySearch))
                .ToArray();
        }

        if (!string.IsNullOrWhiteSpace(HistoryPaymentMethod))
        {
            paidHistory = paidHistory
                .Where(x => string.Equals(x.PaymentMethod, HistoryPaymentMethod, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        if (HistoryDateFrom.HasValue)
        {
            paidHistory = paidHistory
                .Where(x => (x.PaidDate ?? x.CreatedDate).Date >= HistoryDateFrom.Value.Date)
                .ToArray();
        }

        if (HistoryDateTo.HasValue)
        {
            paidHistory = paidHistory
                .Where(x => (x.PaidDate ?? x.CreatedDate).Date <= HistoryDateTo.Value.Date)
                .ToArray();
        }

        PaidHistory = paidHistory
            .OrderByDescending(x => x.PaidDate ?? x.CreatedDate)
            .ThenByDescending(x => x.RubberPurchaseHeaderId)
            .ToArray();

        if (SelectedPurchaseId.HasValue)
        {
            SelectedPurchase = await _rubberPurchaseService.GetByIdAsync(SelectedPurchaseId.Value, cancellationToken);
        }
        else
        {
            SelectedPurchase = DuePurchases.FirstOrDefault();
        }

        if (SelectedPurchase is not null && Input.RubberPurchaseHeaderId <= 0)
        {
            Input.RubberPurchaseHeaderId = SelectedPurchase.RubberPurchaseHeaderId;
            Input.PaidAmount = SelectedPurchase.TotalAmount ?? 0;
        }

        if (SelectedPurchase?.CustomerId.HasValue == true)
        {
            SelectedCustomerCredit = await _customerCreditService.GetByCustomerIdAsync(SelectedPurchase.CustomerId.Value, cancellationToken);
        }

        if (ReceiptId.HasValue)
        {
            Receipt = await _rubberPurchaseService.GetByIdAsync(ReceiptId.Value, cancellationToken);
        }
    }

    public static decimal GetGrossAmount(RubberPurchaseHeaderModel purchase) =>
        purchase.WeightKg * (purchase.PricePerKgSnapshot ?? 0);

    public static decimal GetDeductAmount(RubberPurchaseHeaderModel purchase) =>
        GetGrossAmount(purchase) * ((purchase.PercentageSnapshot ?? 0) / 100m);

    public static decimal GetRemainingAmount(RubberPurchaseHeaderModel purchase) =>
        Math.Max(0, (purchase.TotalAmount ?? 0) - purchase.PaidAmount);

    private static bool Contains(string? value, string searchText) =>
        !string.IsNullOrWhiteSpace(value) && value.Contains(searchText, StringComparison.OrdinalIgnoreCase);

    public sealed class PayBillInput
    {
        [Range(1, long.MaxValue)] public long RubberPurchaseHeaderId { get; set; }
        [Range(0.01, 99999999)] public decimal PaidAmount { get; set; }
        [Required] public string PaymentMethod { get; set; } = "Cash";
        [StringLength(500)] public string? PaymentRemark { get; set; }
        public bool PayCustomerCredit { get; set; }
    }
}
