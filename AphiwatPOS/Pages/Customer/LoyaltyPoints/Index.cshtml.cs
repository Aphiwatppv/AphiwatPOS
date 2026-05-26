using System.ComponentModel.DataAnnotations;
using CustomerEngine.Models;
using CustomerEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AphiwatPOS.Pages.Customer.LoyaltyPoints;

public sealed class IndexModel : PageModel
{
    private readonly ICustomerService _customerService;
    private readonly ILoyaltyPointService _pointService;
    private readonly ICustomerReportService _reportService;

    public IndexModel(ICustomerService customerService, ILoyaltyPointService pointService, ICustomerReportService reportService)
    {
        _customerService = customerService;
        _pointService = pointService;
        _reportService = reportService;
    }

    [BindProperty(SupportsGet = true)] public int? CustomerId { get; set; }
    [BindProperty(SupportsGet = true)] public string? SearchText { get; set; }
    [BindProperty(SupportsGet = true)] public string? MovementType { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateFrom { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateTo { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    [BindProperty] public AdjustFormInput AdjustInput { get; set; } = new();

    public IReadOnlyCollection<CustomerSummaryModel> Customers { get; private set; } = Array.Empty<CustomerSummaryModel>();
    public CustomerModel? SelectedCustomer { get; private set; }
    public LoyaltyPointPagedResultModel Movements { get; private set; } = new();
    public LoyaltyPointSummaryModel Summary { get; private set; } = new();
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        StatusMessage = TempData["StatusMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        await LoadAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAdjustPointsAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(AdjustInput.Remark)) { TempData["ErrorMessage"] = "Reason is required for manual adjustment."; return RedirectToPage(new { CustomerId = AdjustInput.CustomerId }); }
        await _pointService.AdjustPointsAsync(new LoyaltyPointAdjustModel { CustomerId = AdjustInput.CustomerId, AdjustmentType = AdjustInput.AdjustmentType, Points = AdjustInput.Points, ReferenceType = "Manual", Remark = AdjustInput.Remark, CreatedByUserId = CustomerPageHelpers.CurrentUserId(User) }, cancellationToken);
        TempData["StatusMessage"] = "Point adjustment saved.";
        return RedirectToPage(new { CustomerId = AdjustInput.CustomerId });
    }

    public async Task<IActionResult> OnPostExpirePointsAsync(CancellationToken cancellationToken)
    {
        var count = await _pointService.ExpirePointsAsync(DateTime.Today, CustomerPageHelpers.CurrentUserId(User), cancellationToken);
        TempData["StatusMessage"] = $"Expired points for {count} customer balance(s).";
        return RedirectToPage(new { CustomerId, SearchText, MovementType, DateFrom, DateTo, PageNumber });
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Customers = (await _customerService.GetPagedAsync(new CustomerPagedRequestModel { PageNumber = 1, PageSize = 100, SearchText = SearchText, IsActive = true }, cancellationToken)).Customers;
        Summary = await _reportService.GetLoyaltyPointSummaryAsync(new CustomerReportRequestModel(), cancellationToken);
        if (CustomerId is int id)
        {
            SelectedCustomer = await _customerService.GetByIdAsync(id, cancellationToken);
            Movements = await _pointService.GetMovementsPagedAsync(new LoyaltyPointPagedRequestModel { CustomerId = id, PageNumber = PageNumber, PageSize = 20, DateFrom = DateFrom, DateTo = DateTo, MovementType = MovementType }, cancellationToken);
        }
    }

    public sealed class AdjustFormInput
    {
        public int CustomerId { get; set; }
        public string AdjustmentType { get; set; } = "AdjustIn";
        [Range(0.01, double.MaxValue)] public decimal Points { get; set; }
        [Required] public string Remark { get; set; } = string.Empty;
    }
}
