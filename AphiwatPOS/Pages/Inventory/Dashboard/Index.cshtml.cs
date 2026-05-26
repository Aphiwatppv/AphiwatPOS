using System.Text.Json;
using InventoryEngine.Models;
using InventoryEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductEngine.Models;
using ProductEngine.Services;

namespace AphiwatPOS.Pages.Inventory.Dashboard;

public sealed class IndexModel : PageModel
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IInventoryDashboardService _dashboardService;
    private readonly IInventoryLocationService _locationService;
    private readonly IProductCategoryService _categoryService;
    private readonly IProductBrandService _brandService;

    public IndexModel(
        IInventoryDashboardService dashboardService,
        IInventoryLocationService locationService,
        IProductCategoryService categoryService,
        IProductBrandService brandService)
    {
        _dashboardService = dashboardService;
        _locationService = locationService;
        _categoryService = categoryService;
        _brandService = brandService;
    }

    [BindProperty(SupportsGet = true)] public int? LocationId { get; set; }
    [BindProperty(SupportsGet = true)] public int? CategoryId { get; set; }
    [BindProperty(SupportsGet = true)] public int? BrandId { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateFrom { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateTo { get; set; }
    [BindProperty(SupportsGet = true)] public string GroupBy { get; set; } = "Daily";

    public InventoryDashboardSummaryModel Summary { get; private set; } = new();
    public IReadOnlyCollection<InventoryStockStatusSummaryModel> StockStatusSummary { get; private set; } = Array.Empty<InventoryStockStatusSummaryModel>();
    public IReadOnlyCollection<InventoryValueByCategoryModel> ValueByCategory { get; private set; } = Array.Empty<InventoryValueByCategoryModel>();
    public IReadOnlyCollection<InventoryValueByLocationModel> ValueByLocation { get; private set; } = Array.Empty<InventoryValueByLocationModel>();
    public IReadOnlyCollection<InventoryRecentMovementModel> RecentMovements { get; private set; } = Array.Empty<InventoryRecentMovementModel>();
    public IReadOnlyCollection<InventoryMovementTrendModel> MovementTrend { get; private set; } = Array.Empty<InventoryMovementTrendModel>();
    public IReadOnlyCollection<InventoryLowStockProductModel> LowStockProducts { get; private set; } = Array.Empty<InventoryLowStockProductModel>();
    public IReadOnlyCollection<InventoryTopMovingProductModel> TopMovingProducts { get; private set; } = Array.Empty<InventoryTopMovingProductModel>();
    public IReadOnlyCollection<InventoryLocationModel> Locations { get; private set; } = Array.Empty<InventoryLocationModel>();
    public IReadOnlyCollection<ProductCategoryModel> Categories { get; private set; } = Array.Empty<ProductCategoryModel>();
    public IReadOnlyCollection<ProductBrandModel> Brands { get; private set; } = Array.Empty<ProductBrandModel>();

    public string StockStatusChartJson { get; private set; } = "{}";
    public string MovementTrendChartJson { get; private set; } = "{}";
    public string CategoryValueChartJson { get; private set; } = "{}";
    public string LocationValueChartJson { get; private set; } = "{}";
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DashboardPermissions Permissions { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!InventoryUi.HasAnyInventoryAccess(User))
        {
            return RedirectToPage("/Account/AccessDenied");
        }

        StatusMessage = TempData["StatusMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        await LoadAsync(cancellationToken);
        return Page();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            NormalizeFilters();
            Permissions = BuildPermissions();
            Locations = await _locationService.GetAllActiveAsync(cancellationToken);
            Categories = await _categoryService.GetAllActiveAsync(cancellationToken);
            Brands = await _brandService.GetAllActiveAsync(cancellationToken);

            var filter = BuildFilter();
            Summary = await _dashboardService.GetSummaryAsync(filter, cancellationToken);
            StockStatusSummary = NormalizeStockStatus(await _dashboardService.GetStockStatusSummaryAsync(filter, cancellationToken));
            ValueByCategory = await _dashboardService.GetValueByCategoryAsync(filter, cancellationToken);
            ValueByLocation = await _dashboardService.GetValueByLocationAsync(filter, cancellationToken);
            RecentMovements = await _dashboardService.GetRecentMovementsAsync(filter, 10, cancellationToken);
            MovementTrend = await _dashboardService.GetMovementTrendAsync(filter, cancellationToken);
            LowStockProducts = await _dashboardService.GetLowStockProductsAsync(filter, 10, cancellationToken);
            TopMovingProducts = await _dashboardService.GetTopMovingProductsAsync(filter, 10, cancellationToken);

            BuildChartJson();
        }
        catch (Exception ex)
        {
            ErrorMessage ??= $"Failed to load inventory dashboard. {ex.Message}";
        }
    }

    private InventoryDashboardFilterModel BuildFilter() => new()
    {
        LocationId = LocationId,
        CategoryId = CategoryId,
        BrandId = BrandId,
        DateFrom = DateFrom,
        DateTo = DateTo,
        GroupBy = GroupBy
    };

    private void NormalizeFilters()
    {
        if (string.IsNullOrWhiteSpace(GroupBy) ||
            !new[] { "Daily", "Weekly", "Monthly" }.Contains(GroupBy, StringComparer.OrdinalIgnoreCase))
        {
            GroupBy = "Daily";
        }

        GroupBy = char.ToUpperInvariant(GroupBy[0]) + GroupBy[1..].ToLowerInvariant();
        DateFrom ??= DateTime.Today.AddDays(-30);
        DateTo ??= DateTime.Today;
    }

    private void BuildChartJson()
    {
        StockStatusChartJson = JsonSerializer.Serialize(new
        {
            labels = StockStatusSummary.Select(x => x.StockStatus),
            values = StockStatusSummary.Select(x => x.ProductCount),
            colors = StockStatusSummary.Select(x => x.StockStatus switch
            {
                "Normal" => "#059669",
                "Low Stock" => "#F59E0B",
                "Out of Stock" => "#DC2626",
                _ => "#64748B"
            })
        }, JsonOptions);

        MovementTrendChartJson = JsonSerializer.Serialize(new
        {
            labels = MovementTrend.Select(x => x.PeriodLabel),
            stockIn = MovementTrend.Select(x => x.StockInQty),
            stockOut = MovementTrend.Select(x => x.StockOutQty),
            net = MovementTrend.Select(x => x.NetQty)
        }, JsonOptions);

        CategoryValueChartJson = JsonSerializer.Serialize(new
        {
            labels = ValueByCategory.Take(8).Select(x => x.CategoryName),
            values = ValueByCategory.Take(8).Select(x => x.TotalValue),
            qty = ValueByCategory.Take(8).Select(x => x.TotalQty)
        }, JsonOptions);

        LocationValueChartJson = JsonSerializer.Serialize(new
        {
            labels = ValueByLocation.Select(x => x.LocationName),
            values = ValueByLocation.Select(x => x.TotalValue),
            qty = ValueByLocation.Select(x => x.TotalQty)
        }, JsonOptions);
    }

    private static IReadOnlyCollection<InventoryStockStatusSummaryModel> NormalizeStockStatus(IReadOnlyCollection<InventoryStockStatusSummaryModel> rows)
    {
        var byStatus = rows.ToDictionary(x => x.StockStatus, StringComparer.OrdinalIgnoreCase);
        return new[] { "Normal", "Low Stock", "Out of Stock", "Not Tracked" }
            .Select(status => byStatus.TryGetValue(status, out var row)
                ? row
                : new InventoryStockStatusSummaryModel { StockStatus = status })
            .ToArray();
    }

    private DashboardPermissions BuildPermissions() => new()
    {
        CanViewStock = InventoryUi.HasPermission(User, "INVENTORY_STOCK_VIEW"),
        CanViewMovement = InventoryUi.HasPermission(User, "INVENTORY_MOVEMENT_VIEW"),
        CanCreateAdjustment = InventoryUi.HasPermission(User, "INVENTORY_ADJUSTMENT_CREATE"),
        CanCreateStockCount = InventoryUi.HasPermission(User, "INVENTORY_STOCKCOUNT_CREATE"),
        CanCreateTransfer = InventoryUi.HasPermission(User, "INVENTORY_TRANSFER_CREATE")
    };
}

public sealed class DashboardPermissions
{
    public bool CanViewStock { get; init; }
    public bool CanViewMovement { get; init; }
    public bool CanCreateAdjustment { get; init; }
    public bool CanCreateStockCount { get; init; }
    public bool CanCreateTransfer { get; init; }
}
