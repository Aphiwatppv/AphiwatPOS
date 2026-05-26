using System.Text.Json;
using AphiwatPOS.Services;
using CustomerEngine.Models;
using CustomerEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductEngine.Services;
using SalesEngine.Models;
using SalesEngine.Services;

namespace AphiwatPOS.Pages.Sales.POSCheckout;

public sealed class IndexModel : PageModel
{
    private readonly ISalesCheckoutService _checkoutService;
    private readonly IPaymentMethodService _paymentMethodService;
    private readonly IHeldSaleService _heldSaleService;
    private readonly IProductService _productService;
    private readonly ICustomerService _customerService;
    private readonly ICustomerCreditService _customerCreditService;
    private readonly IReceiptService _receiptService;
    private readonly ISalesHistoryService _salesHistoryService;
    private readonly IPromptPayQrService _promptPayQrService;

    public IndexModel(ISalesCheckoutService checkoutService, IPaymentMethodService paymentMethodService, IHeldSaleService heldSaleService, IProductService productService, ICustomerService customerService, ICustomerCreditService customerCreditService, IReceiptService receiptService, ISalesHistoryService salesHistoryService, IPromptPayQrService promptPayQrService)
    {
        _checkoutService = checkoutService;
        _paymentMethodService = paymentMethodService;
        _heldSaleService = heldSaleService;
        _productService = productService;
        _customerService = customerService;
        _customerCreditService = customerCreditService;
        _receiptService = receiptService;
        _salesHistoryService = salesHistoryService;
        _promptPayQrService = promptPayQrService;
    }

    [BindProperty(SupportsGet = true)] public string? SearchText { get; set; }
    [BindProperty(SupportsGet = true)] public int LocationId { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public long? HeldSaleHeaderId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Barcode { get; set; }
    [BindProperty] public CheckoutInput Input { get; set; } = new();

    public IReadOnlyCollection<CheckoutProductViewModel> Products { get; private set; } = Array.Empty<CheckoutProductViewModel>();
    public IReadOnlyCollection<PaymentMethodModel> PaymentMethods { get; private set; } = Array.Empty<PaymentMethodModel>();
    public IReadOnlyCollection<CustomerSummaryModel> Customers { get; private set; } = Array.Empty<CustomerSummaryModel>();
    public PaymentMethodModel? CreditPaymentMethod { get; private set; }
    public HeldSaleDetailModel? HeldSale { get; private set; }
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool CanDiscount { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!SalesPageHelpers.HasSalesAccess(User)) return RedirectToPage("/Account/AccessDenied");
        StatusMessage = TempData["StatusMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        await LoadAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnGetProductByBarcodeAsync(string barcode, int locationId, CancellationToken cancellationToken)
    {
        return await OnGetCheckBarcodeAsync(barcode, locationId, cancellationToken);
    }

    public async Task<IActionResult> OnGetCheckBarcodeAsync(string barcode, int locationId, CancellationToken cancellationToken)
    {
        if (!SalesPageHelpers.HasSalesAccess(User)) return new UnauthorizedResult();
        if (string.IsNullOrWhiteSpace(barcode)) return new BadRequestObjectResult(new { message = "Barcode is required." });

        var product = await _checkoutService.GetProductByBarcodeAsync(barcode.Trim(), locationId, cancellationToken);
        if (product is null) return new NotFoundObjectResult(new { message = "Product not found for this barcode." });

        var imageUrl = (await _productService.GetByIdAsync(product.ProductId, cancellationToken))?.ProductImageUrl ?? string.Empty;
        return new JsonResult(CheckoutProductViewModel.From(product, imageUrl));
    }

    public IActionResult OnGetPromptPayQr(decimal amount)
    {
        if (!SalesPageHelpers.HasSalesAccess(User)) return new UnauthorizedResult();
        try
        {
            var qr = _promptPayQrService.Generate(amount);
            return new JsonResult(new
            {
                amount = qr.Amount,
                payload = qr.Payload,
                svg = qr.Svg,
                svgDataUri = qr.SvgDataUri,
                payeeDisplayName = qr.PayeeDisplayName
            });
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return new BadRequestObjectResult(new { message = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostCompleteSaleJsonAsync(CancellationToken cancellationToken)
    {
        if (!SalesPageHelpers.HasSalesAccess(User))
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return new JsonResult(new SaleResponseModel
            {
                IsSuccess = false,
                Message = "Sale failed. Please sign in with a sales user.",
                ErrorCode = "UNAUTHORIZED",
                ErrorMessage = "You do not have permission to complete this sale."
            });
        }

        var userId = SalesPageHelpers.CurrentUserId(User);
        var itemInputs = Deserialize<SalesCartItemInput>(Input.ItemsJson).ToArray();
        var paymentInputs = Deserialize<SalesPaymentInput>(Input.PaymentsJson).ToArray();
        try
        {
            var items = itemInputs.Select(SalesPageHelpers.ToCartItem).ToArray();
            var payments = paymentInputs.Select(SalesPageHelpers.ToPayment).ToArray();
            if (!CanApplyDiscount(userId) && (Input.OrderDiscountAmount > 0 || items.Any(item => item.ItemDiscountAmount > 0)))
                throw new InvalidOperationException("Discount approval is required for this sale.");

            var activeMethods = (await EnsureCustomerCreditPaymentMethodAsync(userId, cancellationToken)).ToArray();
            var creditMethod = FindCreditPaymentMethod(activeMethods);
            var creditAmount = creditMethod is null ? 0 : payments.Where(payment => payment.PaymentMethodId == creditMethod.PaymentMethodId).Sum(payment => payment.PaymentAmount);
            await ValidateCreditPaymentAsync(Input.CustomerId, creditAmount, cancellationToken);
            var nonCreditPayments = creditMethod is null ? payments : payments.Where(payment => payment.PaymentMethodId != creditMethod.PaymentMethodId).ToArray();

            var result = await _checkoutService.CompleteTransactionAsync(new SalesCompleteRequestModel
            {
                CustomerId = Input.CustomerId,
                CashierUserId = userId,
                HeldSaleHeaderId = Input.HeldSaleHeaderId,
                UseCustomerCredit = creditAmount > 0,
                CustomerCreditAmount = creditAmount,
                OrderDiscountAmount = Input.OrderDiscountAmount,
                TaxAmount = Input.TaxAmount,
                Remark = Input.Remark,
                AllowNegativeStock = false,
                CreatedByUserId = userId,
                Items = items,
                Payments = nonCreditPayments
            }, cancellationToken);

            await IssueDefaultDocumentsAsync(result, Input.CustomerId, userId, cancellationToken);
            return new JsonResult(await BuildSaleResponseAsync(result, Input.CustomerId, itemInputs, paymentInputs, activeMethods, creditAmount, userId, cancellationToken));
        }
        catch (Exception ex)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            var activeMethods = await EnsureCustomerCreditPaymentMethodAsync(userId, cancellationToken);
            return new JsonResult(await BuildFailedSaleResponseAsync(ex, Input.CustomerId, itemInputs, paymentInputs, activeMethods, userId, cancellationToken));
        }
    }

    public async Task<IActionResult> OnPostCompleteSaleAsync(CancellationToken cancellationToken)
    {
        if (!SalesPageHelpers.HasSalesAccess(User)) return RedirectToPage("/Account/AccessDenied");
        var userId = SalesPageHelpers.CurrentUserId(User);
        try
        {
            var itemInputs = Deserialize<SalesCartItemInput>(Input.ItemsJson).ToArray();
            var paymentInputs = Deserialize<SalesPaymentInput>(Input.PaymentsJson).ToArray();
            var items = itemInputs.Select(SalesPageHelpers.ToCartItem).ToArray();
            var payments = paymentInputs.Select(SalesPageHelpers.ToPayment).ToArray();
            if (!CanApplyDiscount(userId) && (Input.OrderDiscountAmount > 0 || items.Any(item => item.ItemDiscountAmount > 0)))
                throw new InvalidOperationException("Discount approval is required for this sale.");

            var activeMethods = (await EnsureCustomerCreditPaymentMethodAsync(userId, cancellationToken)).ToArray();
            var creditMethod = FindCreditPaymentMethod(activeMethods);
            var creditAmount = creditMethod is null ? 0 : payments.Where(payment => payment.PaymentMethodId == creditMethod.PaymentMethodId).Sum(payment => payment.PaymentAmount);
            await ValidateCreditPaymentAsync(Input.CustomerId, creditAmount, cancellationToken);
            var nonCreditPayments = creditMethod is null ? payments : payments.Where(payment => payment.PaymentMethodId != creditMethod.PaymentMethodId).ToArray();

            var result = await _checkoutService.CompleteTransactionAsync(new SalesCompleteRequestModel
            {
                CustomerId = Input.CustomerId,
                CashierUserId = userId,
                HeldSaleHeaderId = Input.HeldSaleHeaderId,
                UseCustomerCredit = creditAmount > 0,
                CustomerCreditAmount = creditAmount,
                OrderDiscountAmount = Input.OrderDiscountAmount,
                TaxAmount = Input.TaxAmount,
                Remark = Input.Remark,
                AllowNegativeStock = false,
                CreatedByUserId = userId,
                Items = items,
                Payments = nonCreditPayments
            }, cancellationToken);

            await IssueDefaultDocumentsAsync(result, Input.CustomerId, userId, cancellationToken);
            TempData["LastSaleResultJson"] = await BuildSaleResultJsonAsync(result, Input.CustomerId, itemInputs, paymentInputs, activeMethods, creditAmount, userId, cancellationToken);

            TempData["LastSaleNo"] = result.SaleNo;
            return RedirectToPage(new { LocationId });
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            await LoadAsync(cancellationToken);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostHoldSaleAsync(CancellationToken cancellationToken)
    {
        if (!SalesPageHelpers.HasSalesAccess(User)) return RedirectToPage("/Account/AccessDenied");
        var userId = SalesPageHelpers.CurrentUserId(User);
        try
        {
            var items = Deserialize<SalesCartItemInput>(Input.ItemsJson).Select(SalesPageHelpers.ToCartItem).ToArray();
            if (!CanApplyDiscount(userId) && items.Any(item => item.ItemDiscountAmount > 0))
                throw new InvalidOperationException("Discount approval is required to hold this sale.");

            await _heldSaleService.CreateAsync(new HeldSaleCreateModel
            {
                CustomerId = Input.CustomerId,
                CashierUserId = userId,
                Note = Input.HoldNote,
                EstimatedTaxAmount = Input.TaxAmount,
                CreatedByUserId = userId,
                Items = items
            }, cancellationToken);

            TempData["StatusMessage"] = "Held sale saved. Inventory was not reduced.";
            return RedirectToPage(new { LocationId });
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
        PaymentMethods = await EnsureCustomerCreditPaymentMethodAsync(SalesPageHelpers.CurrentUserId(User), cancellationToken);
        CreditPaymentMethod = FindCreditPaymentMethod(PaymentMethods);
        var products = (await _checkoutService.SearchProductsAsync(SearchText, LocationId, 48, cancellationToken)).ToArray();
        var productImages = new Dictionary<int, string>();
        foreach (var product in await _productService.GetAllActiveAsync(cancellationToken))
        {
            productImages[product.ProductId] = product.ProductImageUrl;
        }

        Products = products.Select(product => CheckoutProductViewModel.From(product, productImages.GetValueOrDefault(product.ProductId) ?? string.Empty)).ToArray();
        Customers = (await _customerService.GetPagedAsync(new CustomerPagedRequestModel { PageNumber = 1, PageSize = 100, IsActive = true }, cancellationToken)).Customers;
        if (HeldSaleHeaderId.HasValue) HeldSale = await _heldSaleService.GetByIdAsync(HeldSaleHeaderId.Value, cancellationToken);
        CanDiscount = SalesPageHelpers.HasPermission(User, "SALES_DISCOUNT") || User.IsInRole("Admin");
    }

    private static IReadOnlyCollection<T> Deserialize<T>(string? json) =>
        string.IsNullOrWhiteSpace(json) ? Array.Empty<T>() : JsonSerializer.Deserialize<T[]>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? Array.Empty<T>();

    private async Task<CustomerCreditCheckResultModel?> ValidateCreditPaymentAsync(int? customerId, decimal creditAmount, CancellationToken cancellationToken)
    {
        if (creditAmount <= 0) return null;
        if (!customerId.HasValue) throw new InvalidOperationException("Customer credit payment requires a selected member customer.");

        var eligibility = await _customerCreditService.CheckEligibilityAsync(customerId.Value, creditAmount, cancellationToken);
        if (!eligibility.IsAllowed) throw new InvalidOperationException(eligibility.Message);
        if (eligibility.RequiresManagerApproval && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            throw new InvalidOperationException("Customer credit payment requires manager approval.");
        }

        return eligibility;
    }

    private async Task<IReadOnlyCollection<PaymentMethodModel>> EnsureCustomerCreditPaymentMethodAsync(int userId, CancellationToken cancellationToken)
    {
        var allMethods = (await _paymentMethodService.GetAllAsync(cancellationToken)).ToArray();
        var creditMethod = FindCreditPaymentMethod(allMethods);

        if (creditMethod is null)
        {
            await _paymentMethodService.CreateAsync(new PaymentMethodCreateModel
            {
                PaymentMethodCode = "CREDIT",
                PaymentMethodName = "Customer Credit",
                Description = "Member customer credit account payment",
                RequireReferenceNo = false,
                IsCash = false,
                DisplayOrder = 60,
                CreatedByUserId = userId
            }, cancellationToken);
        }
        else if (!creditMethod.IsActive)
        {
            await _paymentMethodService.ToggleActiveAsync(creditMethod.PaymentMethodId, true, userId, cancellationToken);
        }

        var activeMethods = (await _paymentMethodService.GetAllActiveAsync(cancellationToken)).ToArray();
        return activeMethods;
    }

    private static PaymentMethodModel? FindCreditPaymentMethod(IEnumerable<PaymentMethodModel> methods) =>
        methods.FirstOrDefault(method => method.PaymentMethodCode.Equals("CREDIT", StringComparison.OrdinalIgnoreCase) ||
                                         method.PaymentMethodName.Contains("Customer Credit", StringComparison.OrdinalIgnoreCase) ||
                                         method.PaymentMethodName.Contains("Store Credit", StringComparison.OrdinalIgnoreCase));

    private bool CanApplyDiscount(int userId) =>
        userId > 0 && (User.IsInRole("Admin") || User.IsInRole("Manager") || SalesPageHelpers.HasPermission(User, "SALES_DISCOUNT"));

    private async Task IssueDefaultDocumentsAsync(SalesCompleteResultModel result, int? customerId, int userId, CancellationToken cancellationToken)
    {
        var salesHeaderId = await ResolveSalesHeaderIdAsync(result, cancellationToken);
        if (salesHeaderId <= 0) return;

        await _receiptService.IssueSalesDocumentAsync(new SalesDocumentIssueModel { SalesHeaderId = salesHeaderId, DocumentType = "Receipt", IssuedByUserId = userId }, cancellationToken);
        await _receiptService.IssueSalesDocumentAsync(new SalesDocumentIssueModel { SalesHeaderId = salesHeaderId, DocumentType = "ShortTaxInvoice", IssuedByUserId = userId }, cancellationToken);
    }

    private async Task<long> ResolveSalesHeaderIdAsync(SalesCompleteResultModel result, CancellationToken cancellationToken)
    {
        if (result.SalesHeaderId > 0) return result.SalesHeaderId;
        if (string.IsNullOrWhiteSpace(result.SaleNo)) return 0;

        var sale = await _salesHistoryService.GetBySaleNoAsync(result.SaleNo, cancellationToken);
        return sale?.SalesHeaderId ?? 0;
    }

    private async Task<SaleResponseModel> BuildSaleResponseAsync(SalesCompleteResultModel result, int? customerId, IReadOnlyCollection<SalesCartItemInput> items, IReadOnlyCollection<SalesPaymentInput> payments, IReadOnlyCollection<PaymentMethodModel> paymentMethods, decimal creditAmount, int userId, CancellationToken cancellationToken)
    {
        var customer = customerId.HasValue ? await _customerService.GetByIdAsync(customerId.Value, cancellationToken) : null;
        var creditInfo = customerId.HasValue ? await _customerCreditService.GetCreditInfoAsync(customerId.Value, cancellationToken) : null;
        var totals = CalculateResponseTotals(items, payments);
        var customerName = string.IsNullOrWhiteSpace(customer?.CustomerName) ? "Walk-in Customer" : customer.CustomerName;
        var cashierName = User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name ?? $"User {userId}";

        return new SaleResponseModel
        {
            IsSuccess = true,
            Message = "Sale completed successfully.",
            SaleId = result.SalesHeaderId > 0 ? result.SalesHeaderId : null,
            SaleNo = result.SaleNo,
            InvoiceNo = result.SaleNo,
            SaleDate = DateTime.Now,
            CustomerId = customerId,
            CustomerName = customerName,
            CashierId = userId,
            CashierName = cashierName,
            TotalItems = items.Sum(item => item.Quantity),
            Subtotal = totals.Subtotal,
            DiscountAmount = totals.Discount,
            TaxAmount = totals.Tax,
            GrandTotal = result.NetAmount > 0 ? result.NetAmount : totals.GrandTotal,
            PaymentMethod = PaymentMethodText(payments, paymentMethods),
            PaidAmount = payments.Sum(payment => payment.PaymentAmount),
            ChangeAmount = result.ChangeAmount,
            CreditUsed = creditAmount,
            RemainingCredit = creditInfo?.AvailableCredit
        };
    }

    private async Task<SaleResponseModel> BuildFailedSaleResponseAsync(Exception ex, int? customerId, IReadOnlyCollection<SalesCartItemInput> items, IReadOnlyCollection<SalesPaymentInput> payments, IReadOnlyCollection<PaymentMethodModel> paymentMethods, int userId, CancellationToken cancellationToken)
    {
        var customer = customerId.HasValue ? await _customerService.GetByIdAsync(customerId.Value, cancellationToken) : null;
        var creditInfo = customerId.HasValue ? await _customerCreditService.GetCreditInfoAsync(customerId.Value, cancellationToken) : null;
        var totals = CalculateResponseTotals(items, payments);
        var creditMethod = FindCreditPaymentMethod(paymentMethods);
        var creditUsed = creditMethod is null ? 0 : payments.Where(payment => payment.PaymentMethodId == creditMethod.PaymentMethodId).Sum(payment => payment.PaymentAmount);

        return new SaleResponseModel
        {
            IsSuccess = false,
            Message = "Sale failed. Please check payment details.",
            SaleDate = DateTime.Now,
            CustomerId = customerId,
            CustomerName = string.IsNullOrWhiteSpace(customer?.CustomerName) ? "Walk-in Customer" : customer.CustomerName,
            CashierId = userId,
            CashierName = User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name ?? $"User {userId}",
            TotalItems = items.Sum(item => item.Quantity),
            Subtotal = totals.Subtotal,
            DiscountAmount = totals.Discount,
            TaxAmount = totals.Tax,
            GrandTotal = totals.GrandTotal,
            PaymentMethod = PaymentMethodText(payments, paymentMethods),
            PaidAmount = totals.Paid,
            ChangeAmount = Math.Max(0, totals.Paid - totals.GrandTotal),
            CreditUsed = creditUsed,
            RemainingCredit = creditInfo?.AvailableCredit,
            ErrorCode = ErrorCodeFor(ex),
            ErrorMessage = ex.Message
        };
    }

    private (decimal Subtotal, decimal Discount, decimal Tax, decimal GrandTotal, decimal Paid) CalculateResponseTotals(IReadOnlyCollection<SalesCartItemInput> items, IReadOnlyCollection<SalesPaymentInput> payments)
    {
        var subtotal = items.Sum(item => item.Quantity * item.UnitPrice);
        var itemDiscount = items.Sum(item => item.ItemDiscountAmount);
        var tax = items.Sum(item => item.TaxAmount) + Input.TaxAmount;
        var grandTotal = Math.Max(0, subtotal - itemDiscount - Input.OrderDiscountAmount + tax);
        return (subtotal, itemDiscount + Input.OrderDiscountAmount, tax, grandTotal, payments.Sum(payment => payment.PaymentAmount));
    }

    private static string PaymentMethodText(IReadOnlyCollection<SalesPaymentInput> payments, IReadOnlyCollection<PaymentMethodModel> paymentMethods)
    {
        var methodLookup = paymentMethods.ToDictionary(method => method.PaymentMethodId);
        return string.Join(", ", payments.Select(payment => !string.IsNullOrWhiteSpace(payment.Name) ? payment.Name : methodLookup.GetValueOrDefault(payment.PaymentMethodId)?.PaymentMethodName ?? "Payment").Distinct());
    }

    private static string ErrorCodeFor(Exception ex)
    {
        var message = ex.Message.ToLowerInvariant();
        if (message.Contains("credit")) return "CUSTOMER_CREDIT_ERROR";
        if (message.Contains("stock")) return "STOCK_NOT_ENOUGH";
        if (message.Contains("payment") || message.Contains("amount")) return "INVALID_PAYMENT";
        if (message.Contains("price")) return "PRICE_CHANGED";
        if (message.Contains("discount")) return "DISCOUNT_APPROVAL_REQUIRED";
        return "SALE_PROCESSING_ERROR";
    }

    private async Task<string> BuildSaleResultJsonAsync(SalesCompleteResultModel result, int? customerId, IReadOnlyCollection<SalesCartItemInput> items, IReadOnlyCollection<SalesPaymentInput> payments, IReadOnlyCollection<PaymentMethodModel> paymentMethods, decimal creditAmount, int userId, CancellationToken cancellationToken)
    {
        var customer = customerId.HasValue ? await _customerService.GetByIdAsync(customerId.Value, cancellationToken) : null;
        var creditInfo = customerId.HasValue ? await _customerCreditService.GetCreditInfoAsync(customerId.Value, cancellationToken) : null;
        var methodLookup = paymentMethods.ToDictionary(method => method.PaymentMethodId);
        var subtotal = items.Sum(item => item.Quantity * item.UnitPrice);
        var itemDiscount = items.Sum(item => item.ItemDiscountAmount);
        var lineTax = items.Sum(item => item.TaxAmount);
        var manualTax = Input.TaxAmount;
        var tax = lineTax + manualTax;
        var net = Math.Max(0, subtotal - itemDiscount - Input.OrderDiscountAmount + tax);
        var paid = payments.Sum(payment => payment.PaymentAmount);
        var change = result.ChangeAmount;
        var remaining = Math.Max(0, net - paid);
        var customerName = string.IsNullOrWhiteSpace(customer?.CustomerName) ? "Walk-in Customer" : customer.CustomerName;
        var cashierName = User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name ?? $"User {userId}";

        var payload = new
        {
            saleNo = result.SaleNo,
            receiptNo = result.SaleNo,
            invoiceNo = result.SaleNo,
            saleDate = DateTime.Now.ToString("G"),
            cashierName,
            customer = customerId.HasValue ? new
            {
                name = customerName,
                phone = customer?.PhoneNumber ?? string.Empty,
                code = customer?.CustomerCode ?? string.Empty,
                level = customer?.MemberLevelName ?? string.Empty
            } : null,
            items = items.Select(item => new
            {
                productId = item.ProductId,
                productName = string.IsNullOrWhiteSpace(item.ProductName) ? $"Product #{item.ProductId}" : item.ProductName,
                productCode = item.ProductCode ?? string.Empty,
                unitSymbol = item.UnitSymbol ?? string.Empty,
                quantity = item.Quantity,
                unitPrice = item.UnitPrice,
                itemDiscountAmount = item.ItemDiscountAmount,
                taxAmount = item.TaxAmount
            }),
            payments = payments.Select(payment => new
            {
                paymentMethodId = payment.PaymentMethodId,
                name = !string.IsNullOrWhiteSpace(payment.Name) ? payment.Name : methodLookup.GetValueOrDefault(payment.PaymentMethodId)?.PaymentMethodName ?? "Payment",
                paymentAmount = payment.PaymentAmount,
                referenceNo = payment.ReferenceNo
            }),
            totals = new
            {
                subtotal,
                itemDiscount,
                orderDiscount = Input.OrderDiscountAmount,
                lineTax,
                manualTax,
                tax,
                net = result.NetAmount > 0 ? result.NetAmount : net,
                paid,
                change,
                remaining,
                creditUsed = creditAmount,
                otherPaid = Math.Max(0, paid - creditAmount),
                count = items.Sum(item => item.Quantity)
            },
            availableCreditAfter = creditInfo?.AvailableCredit ?? 0
        };

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    public sealed class CheckoutInput
    {
        public int? CustomerId { get; set; }
        public long? HeldSaleHeaderId { get; set; }
        public decimal OrderDiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public string? Remark { get; set; }
        public string? HoldNote { get; set; }
        public string ItemsJson { get; set; } = "[]";
        public string PaymentsJson { get; set; } = "[]";
    }

    public sealed class CheckoutProductViewModel
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
        public string ProductImageUrl { get; init; } = string.Empty;

        public static CheckoutProductViewModel From(SalesCheckoutProductModel product, string imageUrl) => new()
        {
            ProductId = product.ProductId,
            ProductCode = product.ProductCode,
            ProductName = product.ProductName,
            Barcode = product.Barcode,
            UnitId = product.UnitId,
            UnitSymbol = product.UnitSymbol,
            CostPrice = product.CostPrice,
            SellingPrice = product.SellingPrice,
            WholesalePrice = product.WholesalePrice,
            WholesaleMinQty = product.WholesaleMinQty,
            TaxRate = product.TaxRate,
            DiscountAllowed = product.DiscountAllowed,
            IsStockTracked = product.IsStockTracked,
            IsActive = product.IsActive,
            Status = product.Status,
            CurrentStock = product.CurrentStock,
            ProductImageUrl = imageUrl
        };
    }
}
