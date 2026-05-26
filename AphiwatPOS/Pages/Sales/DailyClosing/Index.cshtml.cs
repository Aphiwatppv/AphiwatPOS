using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AuthenticationEngine.Models;
using AuthenticationEngine.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SalesEngine.Models;
using SalesEngine.Services;

namespace AphiwatPOS.Pages.Sales.DailyClosing;

[Authorize(Policy = "ReportView")]
public sealed class IndexModel : PageModel
{
    private readonly ISalesClosingService _salesClosingService;
    private readonly IUserManagementService _userManagementService;

    public IndexModel(ISalesClosingService salesClosingService, IUserManagementService userManagementService)
    {
        _salesClosingService = salesClosingService;
        _userManagementService = userManagementService;
    }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime ClosingDate { get; set; } = DateTime.Today;

    [BindProperty(SupportsGet = true)]
    public int? CashierUserId { get; set; }

    [BindProperty]
    [Range(0, double.MaxValue)]
    public decimal ActualCashAmount { get; set; }

    [BindProperty]
    [StringLength(1000)]
    public string? Notes { get; set; }

    public DailySalesClosingModel Closing { get; private set; } = new();
    public IReadOnlyCollection<UserSummary> Cashiers { get; private set; } = Array.Empty<UserSummary>();

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
        ActualCashAmount = Closing.ActualCashAmount ?? Closing.ExpectedCashAmount;
        Notes = Closing.Notes;
    }

    public async Task<IActionResult> OnPostCloseAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync(cancellationToken);
            return Page();
        }

        Closing = await _salesClosingService.SaveDailyClosingAsync(new DailySalesClosingSaveModel
        {
            ClosingDate = ClosingDate,
            CashierUserId = CashierUserId,
            ActualCashAmount = ActualCashAmount,
            Notes = Notes,
            ClosedByUserId = CurrentUserId()
        }, cancellationToken);

        StatusMessage = "Daily sales closing saved.";
        return RedirectToPage(new { ClosingDate = ClosingDate.ToString("yyyy-MM-dd"), CashierUserId });
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Cashiers = (await _userManagementService.GetUsersAsync(cancellationToken))
            .Where(user => user.IsActive)
            .OrderBy(user => user.DisplayName)
            .ToArray();

        Closing = await _salesClosingService.GetDailyClosingAsync(ClosingDate, CashierUserId, cancellationToken);
    }

    private int CurrentUserId()
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : 0;
    }
}
