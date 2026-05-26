using CustomerEngine.Models;
using CustomerEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AphiwatPOS.Pages.Customers.Credit;

public sealed class IndexModel : PageModel
{
    private readonly ICustomerCreditService _creditService;
    private readonly ICustomerService _customerService;
    private readonly IMemberLevelService _memberLevelService;

    public IndexModel(ICustomerCreditService creditService, ICustomerService customerService, IMemberLevelService memberLevelService)
    {
        _creditService = creditService;
        _customerService = customerService;
        _memberLevelService = memberLevelService;
    }

    [BindProperty(SupportsGet = true)] public string? SearchText { get; set; }
    [BindProperty(SupportsGet = true)] public int? MemberLevelId { get; set; }
    [BindProperty(SupportsGet = true)] public string? CreditFilter { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

    public CustomerPagedResultModel Customers { get; private set; } = new();
    public IReadOnlyCollection<MemberLevelModel> MemberLevels { get; private set; } = Array.Empty<MemberLevelModel>();
    public Dictionary<int, IReadOnlyCollection<CustomerCreditTransactionModel>> LatestTransactions { get; } = new();
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public decimal TotalCreditLimit => Customers.Customers.Sum(x => x.CreditLimit);
    public decimal TotalUsedCredit => Customers.Customers.Sum(x => x.CurrentOutstandingAmount);
    public decimal TotalAvailableCredit => Customers.Customers.Sum(x => x.AvailableCredit);
    public int CustomersUsingCredit => Customers.Customers.Count(x => x.CurrentOutstandingAmount > 0);
    public int CustomersWithNoAvailableCredit => Customers.Customers.Count(x => x.CreditLimit > 0 && x.AvailableCredit <= 0);

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated != true) return RedirectToPage("/Account/Login");
        StatusMessage = TempData["StatusMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        await LoadAsync(cancellationToken);
        return Page();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        MemberLevels = await _memberLevelService.GetAllActiveAsync(cancellationToken);
        var creditStatus = CreditFilter is "CreditAllowed" or "HasUsedCredit" or "NoAvailableCredit" ? "Good" : null;
        Customers = await _customerService.GetPagedAsync(new CustomerPagedRequestModel { PageNumber = PageNumber, PageSize = 20, SearchText = SearchText, MemberLevelId = MemberLevelId, IsActive = true, CreditStatus = creditStatus }, cancellationToken);

        var filtered = Customers.Customers.AsEnumerable();
        filtered = CreditFilter switch
        {
            "CreditNotAllowed" => filtered.Where(x => x.CreditLimit <= 0),
            "HasUsedCredit" => filtered.Where(x => x.CurrentOutstandingAmount > 0),
            "NoAvailableCredit" => filtered.Where(x => x.CreditLimit > 0 && x.AvailableCredit <= 0),
            _ => filtered
        };
        Customers = new CustomerPagedResultModel { Customers = filtered.ToArray(), TotalCount = Customers.TotalCount, PageNumber = Customers.PageNumber, PageSize = Customers.PageSize };

        foreach (var customer in Customers.Customers.Take(10))
        {
            var tx = await _creditService.GetTransactionsPagedAsync(new CustomerCreditTransactionPagedRequestModel { CustomerId = customer.CustomerId, PageNumber = 1, PageSize = 8 }, cancellationToken);
            LatestTransactions[customer.CustomerId] = tx.Transactions;
        }
    }
}
