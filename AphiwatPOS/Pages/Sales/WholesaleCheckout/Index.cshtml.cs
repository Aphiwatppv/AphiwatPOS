using System.Text.Json;
using AphiwatPOS.Services;
using CustomerEngine.Models;
using CustomerEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductEngine.Services;
using SalesEngine.Models;
using SalesEngine.Services;

namespace AphiwatPOS.Pages.Sales.WholesaleCheckout;

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
    private readonly IWholesalePosService _wholesalePosService;

    public IndexModel(ISalesCheckoutService checkoutService, IPaymentMethodService paymentMethodService, IHeldSaleService heldSaleService, IProductService productService, ICustomerService customerService, ICustomerCreditService customerCreditService, IReceiptService receiptService, ISalesHistoryService salesHistoryService, IPromptPayQrService promptPayQrService, IWholesalePosService wholesalePosService)
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
        _wholesalePosService = wholesalePosService;
    }

    [BindProperty(SupportsGet = true)] public string? SearchText { get; set; }
    [BindProperty(SupportsGet = true)] public int LocationId { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public long? HeldSaleHeaderId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Barcode { get; set; }
    [BindProperty] public CheckoutInput Input { get; set; } = new();

    public IReadOnlyCollection<CheckoutProductViewModel> Products { get; private set; } = Array.Empty<CheckoutProductViewModel>();
    public IReadOnlyCollection<PaymentMethodModel> PaymentMethods { get; private set; } = Array.Empty<PaymentMethodModel>();
    public IReadOnlyCollection<CustomerSummaryModel> Customers { get; private set; } = Array.Empty<CustomerSummaryModel>();
    public IReadOnlyCollection<CustomerSearchResultModel> WholesaleCustomers { get; private set; } = Array.Empty<CustomerSearchResultModel>();
    public PaymentMethodModel? CreditPaymentMethod { get; private set; }
    public HeldSaleDetailModel? HeldSale { get; private set; }
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool CanDiscount { get; private set; }
    public bool AllowWalkInWholesale { get; private set; } = true;

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

        var product = await _wholesalePosService.GetWholesaleProductByBarcodeAsync(barcode.Trim(), locationId, null, cancellationToken);
        if (product is null) return new NotFoundObjectResult(new { message = "Product not found for this barcode." });

        var imageUrl = (await _productService.GetByIdAsync(product.ProductId, cancellationToken))?.ProductImageUrl ?? string.Empty;
        return new JsonResult(CheckoutProductViewModel.From(product, imageUrl, useWholesalePrice: true));
    }

    public async Task<IActionResult> OnGetSearchCustomersAsync(string? term, CancellationToken cancellationToken)
    {
        if (!SalesPageHelpers.HasSalesAccess(User)) return new UnauthorizedResult();
        return new JsonResult(await _wholesalePosService.SearchCustomersAsync(term, cancellationToken));
    }

    public async Task<IActionResult> OnGetValidateCustomerAsync(int? customerId, decimal saleAmount, CancellationToken cancellationToken)
    {
        if (!SalesPageHelpers.HasSalesAccess(User)) return new UnauthorizedResult();
        return new JsonResult(await _wholesalePosService.ValidateCustomerAsync(customerId, saleAmount, AllowWalkInWholesale, cancellationToken));
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

    public async Task<IActionResult> OnPostCompleteSaleAsync(CancellationToken cancellationToken)
    {
        if (!SalesPageHelpers.HasSalesAccess(User)) return RedirectToPage("/Account/AccessDenied");
        var userId = SalesPageHelpers.CurrentUserId(User);
        try
        {
            var items = Deserialize<SalesCartItemInput>(Input.ItemsJson).Select(SalesPageHelpers.ToCartItem).ToArray();
            var payments = Deserialize<SalesPaymentInput>(Input.PaymentsJson).Select(SalesPageHelpers.ToPayment).ToArray();
            if (!CanApplyDiscount(userId) && (Input.OrderDiscountAmount > 0 || items.Any(item => item.ItemDiscountAmount > 0)))
                throw new InvalidOperationException("Discount approval is required for this sale.");

            await ValidateWholesaleSaleAsync(Input.CustomerId, items, cancellationToken);

            var activeMethods = (await EnsureCustomerCreditPaymentMethodAsync(userId, cancellationToken)).ToArray();
            var creditMethod = FindCreditPaymentMethod(activeMethods);
            var creditAmount = creditMethod is null ? 0 : payments.Where(payment => payment.PaymentMethodId == creditMethod.PaymentMethodId).Sum(payment => payment.PaymentAmount);
            await ValidateCreditPaymentAsync(Input.CustomerId, creditAmount, cancellationToken);
            var nonCreditPayments = creditMethod is null ? payments : payments.Where(payment => payment.PaymentMethodId != creditMethod.PaymentMethodId).ToArray();

            var result = await _wholesalePosService.SaveWholesaleSaleAsync(new SalesCompleteRequestModel
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

            TempData["StatusMessage"] = $"Wholesale sale {result.SaleNo} completed successfully.";
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

            await ValidateWholesaleSaleAsync(Input.CustomerId, items, cancellationToken);

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

        Products = products.Select(product => CheckoutProductViewModel.From(product, productImages.GetValueOrDefault(product.ProductId) ?? string.Empty, useWholesalePrice: true)).ToArray();
        Customers = (await _customerService.GetPagedAsync(new CustomerPagedRequestModel { PageNumber = 1, PageSize = 100, IsActive = true }, cancellationToken)).Customers;
        WholesaleCustomers = await _wholesalePosService.SearchCustomersAsync(null, cancellationToken);
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

    private async Task ValidateWholesaleSaleAsync(int? customerId, IReadOnlyCollection<SalesCartItemModel> items, CancellationToken cancellationToken)
    {
        var saleAmount = items.Sum(item => Math.Max(0, item.Quantity * item.UnitPrice - item.ItemDiscountAmount)) + Input.TaxAmount - Input.OrderDiscountAmount;
        var validation = await _wholesalePosService.ValidateCustomerAsync(customerId, saleAmount, AllowWalkInWholesale, cancellationToken);
        if (!validation.IsValid)
        {
            var warningText = validation.WarningMessages.Count > 0 ? $" {string.Join(" ", validation.WarningMessages)}" : string.Empty;
            throw new InvalidOperationException($"{validation.Message}{warningText}");
        }

        var productCache = new Dictionary<int, IReadOnlyCollection<SalesCheckoutProductModel>>();
        foreach (var item in items)
        {
            if (!productCache.TryGetValue(item.LocationId, out var locationProducts))
            {
                locationProducts = await _checkoutService.SearchProductsAsync(null, item.LocationId, 100, cancellationToken);
                productCache[item.LocationId] = locationProducts;
            }

            var product = locationProducts.FirstOrDefault(product => product.ProductId == item.ProductId);
            if (product is not null && item.Quantity < product.WholesaleMinQty)
            {
                throw new InvalidOperationException($"Wholesale minimum for {product.ProductName} is {product.WholesaleMinQty:N0}.");
            }
        }
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
        if (customerId.HasValue)
        {
            await _receiptService.IssueSalesDocumentAsync(new SalesDocumentIssueModel { SalesHeaderId = salesHeaderId, DocumentType = "FullTaxInvoice", IssuedByUserId = userId }, cancellationToken);
        }
    }

    private async Task<long> ResolveSalesHeaderIdAsync(SalesCompleteResultModel result, CancellationToken cancellationToken)
    {
        if (result.SalesHeaderId > 0) return result.SalesHeaderId;
        if (string.IsNullOrWhiteSpace(result.SaleNo)) return 0;

        var sale = await _salesHistoryService.GetBySaleNoAsync(result.SaleNo, cancellationToken);
        return sale?.SalesHeaderId ?? 0;
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

        public static CheckoutProductViewModel From(SalesCheckoutProductModel product, string imageUrl, bool useWholesalePrice = false)
        {
            var effectiveWholesalePrice = product.WholesalePrice > 0 ? product.WholesalePrice : product.SellingPrice;
            var effectivePrice = useWholesalePrice ? effectiveWholesalePrice : product.SellingPrice;
            return new CheckoutProductViewModel
            {
                ProductId = product.ProductId,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                Barcode = product.Barcode,
                UnitId = product.UnitId,
                UnitSymbol = product.UnitSymbol,
                CostPrice = product.CostPrice,
                SellingPrice = effectivePrice,
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
}
