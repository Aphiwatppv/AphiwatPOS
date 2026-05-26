using CustomerEngine.Models;
using CustomerEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AphiwatPOS.Pages.Customer.CustomerReport;

public sealed class IndexModel : PageModel
{
    private readonly ICustomerReportService _reportService;
    private readonly IMemberLevelService _memberLevelService;

    public IndexModel(ICustomerReportService reportService, IMemberLevelService memberLevelService)
    {
        _reportService = reportService;
        _memberLevelService = memberLevelService;
    }

    [BindProperty(SupportsGet = true)] public DateTime? DateFrom { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateTo { get; set; }
    [BindProperty(SupportsGet = true)] public int? MemberLevelId { get; set; }
    [BindProperty(SupportsGet = true)] public bool? IsActive { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? NoPurchaseAfterDate { get; set; }

    public IReadOnlyCollection<MemberLevelModel> MemberLevels { get; private set; } = Array.Empty<MemberLevelModel>();
    public CustomerReportSummaryModel Summary { get; private set; } = new();
    public IReadOnlyCollection<TopCustomerModel> TopSpending { get; private set; } = Array.Empty<TopCustomerModel>();
    public IReadOnlyCollection<TopCustomerModel> TopVisits { get; private set; } = Array.Empty<TopCustomerModel>();
    public IReadOnlyCollection<MemberLevelSummaryModel> LevelSummary { get; private set; } = Array.Empty<MemberLevelSummaryModel>();
    public LoyaltyPointSummaryModel LoyaltySummary { get; private set; } = new();
    public CustomerCreditSummaryModel CreditSummary { get; private set; } = new();
    public IReadOnlyCollection<InactiveCustomerModel> InactiveCustomers { get; private set; } = Array.Empty<InactiveCustomerModel>();
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken) => await LoadAsync(cancellationToken);
    public IActionResult OnPostFilter() => RedirectToPage(new { DateFrom, DateTo, MemberLevelId, IsActive, NoPurchaseAfterDate });

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            var request = new CustomerReportRequestModel { DateFrom = DateFrom, DateTo = DateTo, MemberLevelId = MemberLevelId, IsActive = IsActive, NoPurchaseAfterDate = NoPurchaseAfterDate, Top = 10 };
            MemberLevels = await _memberLevelService.GetAllActiveAsync(cancellationToken);
            Summary = await _reportService.GetSummaryAsync(request, cancellationToken);
            TopSpending = await _reportService.GetTopCustomersBySpendingAsync(request, cancellationToken);
            TopVisits = await _reportService.GetTopCustomersByVisitAsync(request, cancellationToken);
            LevelSummary = await _reportService.GetMemberLevelSummaryAsync(request, cancellationToken);
            LoyaltySummary = await _reportService.GetLoyaltyPointSummaryAsync(request, cancellationToken);
            CreditSummary = await _reportService.GetCreditSummaryAsync(request, cancellationToken);
            InactiveCustomers = await _reportService.GetInactiveCustomersAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load customer report. {ex.Message}";
        }
    }
}
