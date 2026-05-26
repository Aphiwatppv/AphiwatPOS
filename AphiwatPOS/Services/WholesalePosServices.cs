using CustomerEngine.Models;
using CustomerEngine.Services;
using SalesEngine.Models;
using SalesEngine.Services;

namespace AphiwatPOS.Services;

public interface IWholesalePosService
{
    Task<IReadOnlyCollection<CustomerSearchResultModel>> SearchCustomersAsync(string? searchText, CancellationToken cancellationToken = default);
    Task<CustomerSearchResultModel?> GetCustomerAsync(int customerId, CancellationToken cancellationToken = default);
    Task<WholesaleSaleValidationModel> ValidateCustomerAsync(int? customerId, decimal saleAmount, bool allowWalkInWholesale, CancellationToken cancellationToken = default);
    Task<SalesCheckoutProductModel?> GetWholesaleProductByBarcodeAsync(string barcode, int locationId, int? customerId, CancellationToken cancellationToken = default);
    Task<SalesCompleteResultModel> SaveWholesaleSaleAsync(SalesCompleteRequestModel request, CancellationToken cancellationToken = default);
    Task<SalesDocumentModel?> PrintInvoiceAsync(long salesHeaderId, int userId, CancellationToken cancellationToken = default);
}

public sealed class WholesalePosService : IWholesalePosService
{
    private readonly ICustomerService _customerService;
    private readonly ICustomerCreditService _customerCreditService;
    private readonly ISalesCheckoutService _checkoutService;
    private readonly IReceiptService _receiptService;

    public WholesalePosService(ICustomerService customerService, ICustomerCreditService customerCreditService, ISalesCheckoutService checkoutService, IReceiptService receiptService)
    {
        _customerService = customerService;
        _customerCreditService = customerCreditService;
        _checkoutService = checkoutService;
        _receiptService = receiptService;
    }

    public async Task<IReadOnlyCollection<CustomerSearchResultModel>> SearchCustomersAsync(string? searchText, CancellationToken cancellationToken = default)
    {
        var result = await _customerService.GetPagedAsync(new CustomerPagedRequestModel
        {
            PageNumber = 1,
            PageSize = 50,
            SearchText = searchText,
            IsActive = true
        }, cancellationToken);

        return result.Customers.Select(ToSearchResult).ToArray();
    }

    public async Task<CustomerSearchResultModel?> GetCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _customerService.GetByIdAsync(customerId, cancellationToken);
        return customer is null ? null : ToSearchResult(customer);
    }

    public async Task<WholesaleSaleValidationModel> ValidateCustomerAsync(int? customerId, decimal saleAmount, bool allowWalkInWholesale, CancellationToken cancellationToken = default)
    {
        var warnings = new List<string>();
        if (!customerId.HasValue)
        {
            return new WholesaleSaleValidationModel
            {
                IsValid = allowWalkInWholesale,
                Message = allowWalkInWholesale ? "Walk-in wholesale sale is allowed." : "Customer must be selected for wholesale sale.",
                HasEnoughCredit = true,
                WarningMessages = allowWalkInWholesale ? new[] { "Walk-in wholesale customer selected. Credit, payment terms, and customer-specific discounts will not apply." } : Array.Empty<string>()
            };
        }

        var customer = await _customerService.GetByIdAsync(customerId.Value, cancellationToken);
        if (customer is null)
        {
            return new WholesaleSaleValidationModel
            {
                IsValid = false,
                Message = "Customer was not found.",
                CustomerId = customerId,
                HasEnoughCredit = false,
                WarningMessages = new[] { "Select another customer or register a new wholesale customer." }
            };
        }

        var searchResult = ToSearchResult(customer);
        if (!searchResult.IsWholesaleAllowed) warnings.Add("Customer is not marked as Wholesale, VIP, or Business. Manager review may be required.");
        if (searchResult.OutstandingBalance > 0) warnings.Add($"Outstanding balance: {searchResult.OutstandingBalance:N2}.");

        var hasEnoughCredit = true;
        if (saleAmount > 0 && searchResult.CreditLimit > 0)
        {
            var eligibility = await _customerCreditService.CheckEligibilityAsync(customerId.Value, saleAmount, cancellationToken);
            hasEnoughCredit = eligibility.IsAllowed;
            if (!eligibility.IsAllowed) warnings.Add(eligibility.Message);
            if (eligibility.RequiresManagerApproval) warnings.Add("Customer credit requires manager approval.");
        }

        var hasOverduePayment = searchResult.CreditStatus.Equals("Overdue", StringComparison.OrdinalIgnoreCase) ||
                                searchResult.CreditStatus.Equals("Blocked", StringComparison.OrdinalIgnoreCase);
        if (hasOverduePayment) warnings.Add($"Credit status is {searchResult.CreditStatus}.");

        return new WholesaleSaleValidationModel
        {
            IsValid = hasEnoughCredit && !searchResult.CreditStatus.Equals("Blocked", StringComparison.OrdinalIgnoreCase),
            Message = warnings.Count == 0 ? "Customer verified for wholesale sale." : "Customer verified with warnings.",
            CustomerId = customerId,
            IsWholesaleAllowed = searchResult.IsWholesaleAllowed,
            HasEnoughCredit = hasEnoughCredit,
            HasOverduePayment = hasOverduePayment,
            WarningMessages = warnings
        };
    }

    public Task<SalesCheckoutProductModel?> GetWholesaleProductByBarcodeAsync(string barcode, int locationId, int? customerId, CancellationToken cancellationToken = default) =>
        _checkoutService.GetProductByBarcodeAsync(barcode, locationId, cancellationToken);

    public Task<SalesCompleteResultModel> SaveWholesaleSaleAsync(SalesCompleteRequestModel request, CancellationToken cancellationToken = default) =>
        _checkoutService.CompleteTransactionAsync(request, cancellationToken);

    public async Task<SalesDocumentModel?> PrintInvoiceAsync(long salesHeaderId, int userId, CancellationToken cancellationToken = default)
    {
        if (salesHeaderId <= 0) return null;
        return await _receiptService.IssueSalesDocumentAsync(new SalesDocumentIssueModel
        {
            SalesHeaderId = salesHeaderId,
            DocumentType = "FullTaxInvoice",
            IssuedByUserId = userId
        }, cancellationToken);
    }

    private static CustomerSearchResultModel ToSearchResult(CustomerSummaryModel customer) => new()
    {
        CustomerId = customer.CustomerId,
        CustomerName = customer.CustomerName,
        PhoneNumber = customer.PhoneNumber,
        MemberCode = customer.CustomerCode,
        CustomerType = ResolveCustomerType(customer.MemberLevelName, customer.CreditLimit),
        MemberLevel = customer.MemberLevelName ?? "Standard",
        IsWholesaleAllowed = IsWholesaleAllowed(customer.MemberLevelName, customer.CreditLimit),
        CreditLimit = customer.CreditLimit,
        AvailableCredit = customer.AvailableCredit,
        OutstandingBalance = customer.CurrentOutstandingAmount,
        PaymentTerms = customer.CreditLimit > 0 ? "Customer credit terms" : "Pay now",
        DiscountCondition = string.IsNullOrWhiteSpace(customer.MemberLevelName) ? "Standard wholesale price" : $"{customer.MemberLevelName} member price",
        TaxId = string.Empty,
        Address = string.Empty,
        CreditStatus = customer.CreditStatus
    };

    private static CustomerSearchResultModel ToSearchResult(CustomerModel customer) => new()
    {
        CustomerId = customer.CustomerId,
        CustomerName = customer.CustomerName,
        PhoneNumber = customer.PhoneNumber,
        MemberCode = customer.CustomerCode,
        CustomerType = ResolveCustomerType(customer.MemberLevelName, customer.CreditLimit),
        MemberLevel = customer.MemberLevelName ?? "Standard",
        IsWholesaleAllowed = IsWholesaleAllowed(customer.MemberLevelName, customer.CreditLimit),
        CreditLimit = customer.CreditLimit,
        AvailableCredit = customer.AvailableCredit,
        OutstandingBalance = customer.CurrentOutstandingAmount,
        PaymentTerms = customer.CreditLimit > 0 ? $"{customer.CreditTermDays} day credit" : "Pay now",
        DiscountCondition = customer.DiscountPercent > 0 ? $"{customer.DiscountPercent:N2}% member discount" : "Standard wholesale price",
        TaxId = string.Empty,
        Address = customer.Address ?? string.Empty,
        CreditStatus = customer.CreditStatus
    };

    private static bool IsWholesaleAllowed(string? memberLevel, decimal creditLimit)
    {
        var level = memberLevel ?? string.Empty;
        return creditLimit > 0 ||
               level.Contains("wholesale", StringComparison.OrdinalIgnoreCase) ||
               level.Contains("vip", StringComparison.OrdinalIgnoreCase) ||
               level.Contains("business", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveCustomerType(string? memberLevel, decimal creditLimit)
    {
        var level = memberLevel ?? string.Empty;
        if (level.Contains("vip", StringComparison.OrdinalIgnoreCase)) return "VIP";
        if (level.Contains("business", StringComparison.OrdinalIgnoreCase) || level.Contains("company", StringComparison.OrdinalIgnoreCase)) return "Business Customer";
        if (level.Contains("wholesale", StringComparison.OrdinalIgnoreCase) || creditLimit > 0) return "Wholesale";
        return "Retail";
    }
}

public sealed class CustomerSearchResultModel
{
    public int CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string MemberCode { get; init; } = string.Empty;
    public string CustomerType { get; init; } = "Retail";
    public string MemberLevel { get; init; } = "Standard";
    public bool IsWholesaleAllowed { get; init; }
    public decimal CreditLimit { get; init; }
    public decimal AvailableCredit { get; init; }
    public decimal OutstandingBalance { get; init; }
    public string PaymentTerms { get; init; } = "Pay now";
    public string DiscountCondition { get; init; } = "Standard wholesale price";
    public string TaxId { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string CreditStatus { get; init; } = "Good";
}

public sealed class WholesaleSaleValidationModel
{
    public bool IsValid { get; init; }
    public string Message { get; init; } = string.Empty;
    public int? CustomerId { get; init; }
    public bool IsWholesaleAllowed { get; init; }
    public bool HasEnoughCredit { get; init; } = true;
    public bool HasOverduePayment { get; init; }
    public IReadOnlyCollection<string> WarningMessages { get; init; } = Array.Empty<string>();
}

public sealed class WholesaleSaleResponseModel
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public long? SaleId { get; init; }
    public string? SaleNo { get; init; }
    public string? InvoiceNo { get; init; }
    public string CustomerName { get; init; } = "Walk-in Wholesale Customer";
    public decimal TotalItems { get; init; }
    public decimal Subtotal { get; init; }
    public decimal WholesaleDiscount { get; init; }
    public decimal GrandTotal { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public decimal PaidAmount { get; init; }
    public decimal CreditUsed { get; init; }
    public decimal? RemainingCredit { get; init; }
    public string? ErrorMessage { get; init; }
}
