using System.ComponentModel.DataAnnotations;
using AphiwatPOS.Pages.Customer;
using CustomerEngine.Models;
using CustomerEngine.Services;
using Microsoft.AspNetCore.Http;
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

    public async Task<IActionResult> OnPostCreateCreditRepaymentJsonAsync(CancellationToken cancellationToken)
    {
        var before = await _creditService.GetCreditInfoAsync(Input.CustomerId, cancellationToken);
        var methods = (await _paymentMethodService.GetAllActiveAsync(cancellationToken)).ToArray();
        var method = methods.FirstOrDefault(x => x.PaymentMethodId == Input.PaymentMethodId);
        try
        {
            if (before is null) throw new InvalidOperationException("Customer does not exist or is inactive.");
            if (Input.PaymentAmount <= 0) throw new InvalidOperationException("Repayment amount must be greater than zero.");
            if (method is null) throw new InvalidOperationException("Payment method does not exist or is inactive.");
            if (Input.PaymentAmount > before.UsedCredit) throw new InvalidOperationException("Repayment amount cannot exceed used credit.");

            var cashReceived = method.IsCash ? Math.Max(Input.CashReceived, Input.PaymentAmount) : Input.PaymentAmount;
            if (method.IsCash && Input.CashReceived > 0 && Input.CashReceived < Input.PaymentAmount)
                throw new InvalidOperationException("Cash received must cover the repayment amount.");

            var result = await _creditService.CreateRepaymentAsync(new CustomerCreditRepaymentCreateModel
            {
                CustomerId = Input.CustomerId,
                PaymentMethodId = Input.PaymentMethodId,
                PaymentAmount = Input.PaymentAmount,
                ReferenceNo = Input.ReferenceNo,
                Remark = Input.Remark,
                CreatedByUserId = CustomerPageHelpers.CurrentUserId(User)
            }, cancellationToken);

            var after = Math.Max(0, before.UsedCredit - Input.PaymentAmount);
            return new JsonResult(new CreditRepaymentResponseModel
            {
                IsSuccess = true,
                Message = "Credit repayment completed successfully.",
                RepaymentId = result.CustomerCreditRepaymentId,
                ReceiptNo = result.RepaymentNo,
                CustomerId = before.CustomerId,
                CustomerName = before.CustomerName,
                OutstandingBalanceBefore = before.UsedCredit,
                RepaymentAmount = Input.PaymentAmount,
                RemainingBalance = after,
                PaymentMethod = method.PaymentMethodName,
                CashReceived = cashReceived,
                ChangeAmount = method.IsCash ? Math.Max(0, cashReceived - Input.PaymentAmount) : 0
            });
        }
        catch (Exception ex)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return new JsonResult(new CreditRepaymentResponseModel
            {
                IsSuccess = false,
                Message = "Repayment failed",
                CustomerId = before?.CustomerId ?? Input.CustomerId,
                CustomerName = before?.CustomerName ?? string.Empty,
                OutstandingBalanceBefore = before?.UsedCredit ?? 0,
                RepaymentAmount = Input.PaymentAmount,
                RemainingBalance = before?.UsedCredit ?? 0,
                PaymentMethod = method?.PaymentMethodName ?? string.Empty,
                CashReceived = Input.CashReceived,
                ErrorCode = ErrorCodeFor(ex),
                ErrorMessage = ex.Message
            });
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
        public decimal CashReceived { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Remark { get; set; }
    }

    public sealed class VoidInput
    {
        public long CustomerCreditRepaymentId { get; set; }
        [Required] public string Reason { get; set; } = string.Empty;
    }

    private static string ErrorCodeFor(Exception ex)
    {
        var message = ex.Message.ToLowerInvariant();
        if (message.Contains("amount") || message.Contains("cash")) return "INVALID_REPAYMENT_AMOUNT";
        if (message.Contains("customer")) return "CUSTOMER_CREDIT_ERROR";
        if (message.Contains("payment method")) return "INVALID_PAYMENT_METHOD";
        return "CREDIT_REPAYMENT_ERROR";
    }
}
