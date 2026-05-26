using System.ComponentModel.DataAnnotations;
using InventoryEngine.Models;
using InventoryEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductEngine.Models;
using ProductEngine.Services;

namespace AphiwatPOS.Pages.Inventory.StockCount;

public sealed class IndexModel : PageModel
{
    private readonly IInventoryLocationService _locationService;
    private readonly IInventoryStockService _stockService;
    private readonly IStockCountService _stockCountService;
    private readonly IProductService _productService;

    public IndexModel(IInventoryLocationService locationService, IInventoryStockService stockService, IStockCountService stockCountService, IProductService productService)
    {
        _locationService = locationService;
        _stockService = stockService;
        _stockCountService = stockCountService;
        _productService = productService;
    }

    [BindProperty(SupportsGet = true)] public string? SearchText { get; set; }
    [BindProperty(SupportsGet = true)] public int? LocationId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? FromDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? ToDate { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

    [BindProperty] public CountInput Input { get; set; } = new();
    [BindProperty] public ActionInput Action { get; set; } = new();

    public PagedResultModel<StockCountModel> StockCounts { get; private set; } = new();
    public IReadOnlyCollection<InventoryLocationModel> Locations { get; private set; } = Array.Empty<InventoryLocationModel>();
    public IReadOnlyCollection<ProductModel> Products { get; private set; } = Array.Empty<ProductModel>();
    public Dictionary<int, decimal> StockByProduct { get; private set; } = new();
    public Dictionary<int, decimal> ProductCostById { get; private set; } = new();
    public decimal VarianceValue { get; private set; }
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
            if (Input.LocationId <= 0) throw new InvalidOperationException("Location is required.");
            if (!Input.Items.Any(i => i.ProductId > 0)) throw new InvalidOperationException("At least one count item is required.");
            var countId = await _stockCountService.CreateAsync(new StockCountCreateModel { LocationId = Input.LocationId, Remarks = Input.Remarks, CreatedByUserId = InventoryUi.CurrentUserId(User) }, cancellationToken);
            foreach (var item in Input.Items.Where(i => i.ProductId > 0))
            {
                await _stockCountService.AddItemAsync(countId, item.ProductId, item.CountedQty, item.Remarks, cancellationToken);
            }
            TempData["StatusMessage"] = "Stock count draft saved.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostApproveAsync(CancellationToken cancellationToken)
    {
        await _stockCountService.ApproveAsync(new StockCountApproveModel { StockCountId = Action.Id, ApprovedByUserId = InventoryUi.CurrentUserId(User) }, cancellationToken);
        TempData["StatusMessage"] = "Stock count approved and variance movements created.";
        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostCancelAsync(CancellationToken cancellationToken)
    {
        await _stockCountService.CancelAsync(Action.Id, InventoryUi.CurrentUserId(User), cancellationToken);
        TempData["StatusMessage"] = "Stock count cancelled.";
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
                CanCreate = InventoryUi.HasPermission(User, "INVENTORY_STOCKCOUNT_CREATE"),
                CanUpdate = InventoryUi.HasPermission(User, "INVENTORY_STOCKCOUNT_UPDATE"),
                CanApprove = InventoryUi.HasPermission(User, "INVENTORY_STOCKCOUNT_APPROVE"),
                CanCancel = InventoryUi.HasPermission(User, "INVENTORY_STOCKCOUNT_CANCEL")
            };
            Locations = await _locationService.GetAllActiveAsync(cancellationToken);
            Products = await _productService.GetAllActiveAsync(cancellationToken);
            ProductCostById = Products.ToDictionary(product => product.ProductId, product => product.CostPrice);

            var pagedCounts = await _stockCountService.GetPagedAsync(PageNumber, 10, SearchText, NormalizeStatus(Status), LocationId, cancellationToken);
            var detailedCounts = new List<StockCountModel>();
            foreach (var count in pagedCounts.Items)
            {
                detailedCounts.Add(await _stockCountService.GetByIdAsync(count.StockCountId, cancellationToken) ?? count);
            }

            StockCounts = new PagedResultModel<StockCountModel>
            {
                Items = detailedCounts,
                TotalCount = pagedCounts.TotalCount,
                PageNumber = pagedCounts.PageNumber,
                PageSize = pagedCounts.PageSize
            };

            VarianceValue = detailedCounts
                .SelectMany(count => count.Items)
                .Sum(item => item.VarianceQty * ProductCostById.GetValueOrDefault(item.ProductId));

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
            ErrorMessage ??= "Failed to load stock counts.";
        }
    }

    private IActionResult RedirectToCurrentFilters() => RedirectToPage(new { SearchText, LocationId, Status, FromDate, ToDate, PageNumber });
    private static string? NormalizeStatus(string? status) => status is "Counting" or "Pending Approval" ? "Draft" : status;

    public sealed class CountInput
    {
        [Range(1, int.MaxValue)] public int LocationId { get; set; }
        public string? Remarks { get; set; }
        public List<CountItemInput> Items { get; set; } = new();
    }

    public sealed class CountItemInput
    {
        public int ProductId { get; set; }
        public decimal CountedQty { get; set; }
        public string? Remarks { get; set; }
    }

    public sealed class ActionInput
    {
        public long Id { get; set; }
        public string? Reason { get; set; }
    }
}
