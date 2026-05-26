using System.Security.Claims;
using AphiwatPOS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AphiwatPOS.Pages;

public sealed class IndexModel : PageModel
{
    private readonly IManagerDashboardService _managerDashboardService;
    private readonly ICashierDashboardService _cashierDashboardService;

    public IndexModel(IManagerDashboardService managerDashboardService, ICashierDashboardService cashierDashboardService)
    {
        _managerDashboardService = managerDashboardService;
        _cashierDashboardService = cashierDashboardService;
    }

    [BindProperty(SupportsGet = true)] public string? ViewMode { get; set; }
    [BindProperty(SupportsGet = true)] public string? TerminalId { get; set; }

    public bool CanViewManagerDashboard { get; private set; }
    public bool IsCashierDashboard { get; private set; }
    public ManagerDashboardModel? ManagerDashboard { get; private set; }
    public CashierDashboardModel? CashierDashboard { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        DisplayName = User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name ?? "User";
        CanViewManagerDashboard = IsManagerOrOwner();
        var userId = CurrentUserId();
        var requestedCashier = ViewMode?.Equals("cashier", StringComparison.OrdinalIgnoreCase) == true;

        IsCashierDashboard = !CanViewManagerDashboard || requestedCashier;
        if (IsCashierDashboard)
        {
            CashierDashboard = await _cashierDashboardService.GetDashboardAsync(userId, DisplayName, TerminalId, cancellationToken);
            return Page();
        }

        ManagerDashboard = await _managerDashboardService.GetDashboardAsync(cancellationToken);
        return Page();
    }

    private bool IsManagerOrOwner() =>
        User.IsInRole("Admin") ||
        User.IsInRole("Owner") ||
        User.IsInRole("Manager") ||
        User.HasClaim("Permission", "REPORT_VIEW");

    private int CurrentUserId() =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ? userId : 0;
}
