using System.ComponentModel.DataAnnotations;
using InventoryEngine.Models;
using InventoryEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductEngine.Models;
using ProductEngine.Services;

namespace AphiwatPOS.Pages.Inventory.Adjustment;

public sealed class IndexModel : PageModel
{
    private readonly IInventoryLocationService _locationService;
    private readonly IInventoryStockService _stockService;
    private readonly IStockAdjustmentService _adjustmentService;
    private readonly IProductService _productService;

    public IndexModel(IInventoryLocationService locationService, IInventoryStockService stockService, IStockAdjustmentService adjustmentService, IProductService productService)
    {
        _locationService = locationService;
        _stockService = stockService;
        _adjustmentService = adjustmentService;
        _productService = productService;
    }

    [BindProperty(SupportsGet = true)] public string? SearchText { get; set; }
    [BindProperty(SupportsGet = true)] public int? LocationId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? FromDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? ToDate { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

    [BindProperty] public AdjustmentInput Input { get; set; } = new();
    [BindProperty] public ActionInput Action { get; set; } = new();

    public PagedResultModel<StockAdjustmentModel> Adjustments { get; private set; } = new();
    public IReadOnlyCollection<InventoryLocationModel> Locations { get; private set; } = Array.Empty<InventoryLocationModel>();
    public IReadOnlyCollection<ProductModel> Products { get; private set; } = Array.Empty<ProductModel>();
    public Dictionary<int, decimal> StockByProduct { get; private set; } = new();
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }
    public InventoryActionPermissions Permissions { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        StatusMessage = TempData["StatusMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        await LoadAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (Input.LocationId <= 0 || string.IsNullOrWhiteSpace(Input.Reason)) throw new InvalidOperationException("Location and reason are required.");
            if (!Input.Items.Any(i => i.ProductId > 0 && i.Quantity > 0)) throw new InvalidOperationException("At least one adjustment item is required.");

            var adjustmentId = await _adjustmentService.CreateAsync(new StockAdjustmentCreateModel { LocationId = Input.LocationId, Reason = Input.Reason, Remarks = Input.Remarks, CreatedByUserId = InventoryUi.CurrentUserId(User) }, cancellationToken);
            foreach (var item in Input.Items.Where(i => i.ProductId > 0 && i.Quantity > 0))
            {
                await _adjustmentService.AddItemAsync(adjustmentId, item.ProductId, item.Quantity, item.AdjustmentType, item.UnitCost, item.Reason ?? Input.Reason, cancellationToken);
            }

            if (Input.SubmitForApproval)
            {
                TempData["StatusMessage"] = "Adjustment draft saved. Approve it to create inventory movement records.";
            }
            else
            {
                TempData["StatusMessage"] = "Adjustment draft saved.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostApproveAsync(CancellationToken cancellationToken)
    {
        await _adjustmentService.ApproveAsync(new StockAdjustmentApproveModel { StockAdjustmentId = Action.Id, ApprovedByUserId = InventoryUi.CurrentUserId(User) }, cancellationToken);
        TempData["StatusMessage"] = "Adjustment approved and inventory movement records created.";
        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostRejectAsync(CancellationToken cancellationToken)
    {
        await _adjustmentService.RejectAsync(Action.Id, Action.Reason ?? "Rejected", InventoryUi.CurrentUserId(User), cancellationToken);
        TempData["StatusMessage"] = "Adjustment rejected.";
        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostCancelAsync(CancellationToken cancellationToken)
    {
        await _adjustmentService.CancelAsync(Action.Id, InventoryUi.CurrentUserId(User), cancellationToken);
        TempData["StatusMessage"] = "Adjustment cancelled.";
        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnGetProductStockAsync(int locationId, CancellationToken cancellationToken)
    {
        if (locationId <= 0) return new JsonResult(Array.Empty<object>());

        var products = await _productService.GetAllActiveAsync(cancellationToken);
        var rows = new List<object>();
        foreach (var product in products)
        {
            var stock = await _stockService.GetByProductIdAsync(product.ProductId, cancellationToken);
            rows.Add(new
            {
                productId = product.ProductId,
                currentStock = stock.FirstOrDefault(item => item.LocationId == locationId)?.CurrentStock ?? 0,
                unit = product.UnitName,
                unitCost = product.CostPrice
            });
        }

        return new JsonResult(rows);
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            Permissions = new InventoryActionPermissions
            {
                CanView = InventoryUi.HasAnyInventoryAccess(User),
                CanCreate = InventoryUi.HasPermission(User, "INVENTORY_ADJUSTMENT_CREATE"),
                CanUpdate = InventoryUi.HasPermission(User, "INVENTORY_ADJUSTMENT_UPDATE"),
                CanApprove = InventoryUi.HasPermission(User, "INVENTORY_ADJUSTMENT_APPROVE"),
                CanCancel = InventoryUi.HasPermission(User, "INVENTORY_ADJUSTMENT_CANCEL")
            };
            Locations = await _locationService.GetAllActiveAsync(cancellationToken);
            Products = await _productService.GetAllActiveAsync(cancellationToken);
            Adjustments = await _adjustmentService.GetPagedAsync(PageNumber, 10, SearchText, NormalizeStatus(Status), LocationId, cancellationToken);
            if (LocationId.HasValue)
            {
                foreach (var product in Products)
                {
                    var stock = await _stockService.GetByProductIdAsync(product.ProductId, cancellationToken);
                    StockByProduct[product.ProductId] = stock.FirstOrDefault(s => s.LocationId == LocationId)?.CurrentStock ?? 0;
                }
            }
        }
        catch
        {
            ErrorMessage ??= "Failed to load stock adjustments.";
        }
    }

    private IActionResult RedirectToCurrentFilters() => RedirectToPage(new { SearchText, LocationId, Status, FromDate, ToDate, PageNumber });
    private static string? NormalizeStatus(string? status) => status is "Pending" or "Pending Approval" ? "Draft" : status;

    public sealed class AdjustmentInput
    {
        [Range(1, int.MaxValue)] public int LocationId { get; set; }
        [Required] public string Reason { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public bool SubmitForApproval { get; set; }
        public List<AdjustmentItemInput> Items { get; set; } = new();
    }

    public sealed class AdjustmentItemInput
    {
        public int ProductId { get; set; }
        public string AdjustmentType { get; set; } = "Increase";
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public string? Reason { get; set; }
    }

    public sealed class ActionInput
    {
        public long Id { get; set; }
        public string? Reason { get; set; }
    }
}
