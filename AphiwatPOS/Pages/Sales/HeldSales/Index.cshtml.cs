using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SalesEngine.Models;
using SalesEngine.Services;

namespace AphiwatPOS.Pages.Sales.HeldSales;

public sealed class IndexModel : PageModel
{
    private readonly IHeldSaleService _heldSaleService;

    public IndexModel(IHeldSaleService heldSaleService) => _heldSaleService = heldSaleService;

    [BindProperty(SupportsGet = true)] public string? SearchText { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? FromDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? ToDate { get; set; }
    [BindProperty(SupportsGet = true)] public int? CashierUserId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    [BindProperty] public HeldSaleActionInput ActionInput { get; set; } = new();

    public HeldSalePagedResultModel HeldSales { get; private set; } = new();
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!SalesPageHelpers.HasSalesAccess(User)) return RedirectToPage("/Account/AccessDenied");
        StatusMessage = TempData["StatusMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        await LoadAsync(cancellationToken);
        return Page();
    }

    public IActionResult OnPostResumeHeldSale()
    {
        return RedirectToPage("/Sales/POSCheckout/Index", new { HeldSaleHeaderId = ActionInput.HeldSaleHeaderId });
    }

    public async Task<IActionResult> OnPostCancelHeldSaleAsync(CancellationToken cancellationToken)
    {
        await _heldSaleService.CancelAsync(new HeldSaleCancelModel { HeldSaleHeaderId = ActionInput.HeldSaleHeaderId, Reason = ActionInput.Reason, UpdatedByUserId = SalesPageHelpers.CurrentUserId(User) }, cancellationToken);
        TempData["StatusMessage"] = "Held sale cancelled. Inventory was not changed.";
        return RedirectToPage(new { SearchText, FromDate, ToDate, CashierUserId, Status, PageNumber });
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        HeldSales = await _heldSaleService.GetPagedAsync(new HeldSalePagedRequestModel { PageNumber = PageNumber, PageSize = 10, SearchText = SearchText, CashierUserId = CashierUserId, Status = Status, FromDate = FromDate, ToDate = ToDate }, cancellationToken);
    }

    public sealed class HeldSaleActionInput
    {
        public long HeldSaleHeaderId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
