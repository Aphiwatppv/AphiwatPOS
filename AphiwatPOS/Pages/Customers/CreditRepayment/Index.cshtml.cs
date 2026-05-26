using System.ComponentModel.DataAnnotations;
using AphiwatPOS.Pages.Customer;
using CustomerEngine.Models;
using CustomerEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SalesEngine.Models;
using SalesEngine.Services;

namespace AphiwatPOS.Pages.Customers.CreditRepayment;

public sealed class IndexModel : PageModel
{
    private readonly ICustomerCreditService _creditService;
    private readonly IPaymentMethodService _paymentMethodService;

    public IndexModel(ICustomerCreditService creditService, IPaymentMethodService paymentMethodService)
    {
        _creditService = creditService;
        _paymentMethodService = paymentMethodService;
    }

    [BindProperty(SupportsGet = true)] public string? SearchText { get; set; }
    [BindProperty(SupportsGet = true)] public int? SelectedCustomerId { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    [BindProperty] public RepaymentInput Input { get; set; } = new();
    [BindProperty] public VoidInput Void { get; set; } = new();

    public IReadOnlyCollection<CustomerCreditPosModel> CustomerResults { get; private set; } = Array.Empty<CustomerCreditPosModel>();
    public CustomerCreditPosModel? SelectedCustomer { get; private set; }
    public IReadOnlyCollection<PaymentMethodModel> PaymentMethods { get; private set; } = Array.Empty<PaymentMethodModel>();
    public CustomerCreditRepaymentPagedResultModel Repayments { get; private set; } = new();
    public CustomerCreditRepaymentModel? LastRepayment { get; private set; }
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated != true) return RedirectToPage("/Account/Login");
        StatusMessage = TempData["StatusMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        await LoadAsync(cancellationToken);
        return Page();
    }

    public IActionResult OnPostSearchCustomer() => RedirectToPage(new { SearchText });

    public async Task<IActionResult> OnPostCreateCreditRepaymentAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _creditService.CreateRepaymentAsync(new CustomerCreditRepaymentCreateModel
            {
                CustomerId = Input.CustomerId,
                PaymentMethodId = Input.PaymentMethodId,
                PaymentAmount = Input.PaymentAmount,
                ReferenceNo = Input.ReferenceNo,
                Remark = Input.Remark,
                CreatedByUserId = CustomerPageHelpers.CurrentUserId(User)
            }, cancellationToken);
            TempData["StatusMessage"] = $"Credit repayment {result.RepaymentNo} completed.";
            TempData["LastRepaymentId"] = result.CustomerCreditRepaymentId;
            return RedirectToPage(new { SelectedCustomerId = Input.CustomerId, SearchText });
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            SelectedCustomerId = Input.CustomerId;
            await LoadAsync(cancellationToken);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostVoidCreditRepaymentAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _creditService.VoidRepaymentAsync(Void.CustomerCreditRepaymentId, Void.Reason, CustomerPageHelpers.CurrentUserId(User), cancellationToken);
            TempData["StatusMessage"] = "Credit repayment voided and balance reversed.";
            return RedirectToPage(new { SelectedCustomerId, SearchText, PageNumber });
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            await LoadAsync(cancellationToken);
            return Page();
        }
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        PaymentMethods = await _paymentMethodService.GetAllActiveAsync(cancellationToken);
        CustomerResults = await _creditService.SearchForPOSAsync(SearchText, 20, cancellationToken);
        if (SelectedCustomerId.HasValue)
        {
            SelectedCustomer = await _creditService.GetCreditInfoAsync(SelectedCustomerId.Value, cancellationToken);
            Input.CustomerId = SelectedCustomerId.Value;
        }

        Repayments = await _creditService.GetRepaymentsPagedAsync(new CustomerCreditRepaymentPagedRequestModel { PageNumber = PageNumber, PageSize = 12, CustomerId = SelectedCustomerId }, cancellationToken);
        if (TempData["LastRepaymentId"] is long id) LastRepayment = await _creditService.GetRepaymentByIdAsync(id, cancellationToken);
    }

    public sealed class RepaymentInput
    {
        [Required] public int CustomerId { get; set; }
        [Required] public int PaymentMethodId { get; set; }
        [Range(0.01, double.MaxValue)] public decimal PaymentAmount { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Remark { get; set; }
    }

    public sealed class VoidInput
    {
        public long CustomerCreditRepaymentId { get; set; }
        [Required] public string Reason { get; set; } = string.Empty;
    }
}
