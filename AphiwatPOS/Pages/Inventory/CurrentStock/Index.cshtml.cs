using InventoryEngine.Models;
using InventoryEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductEngine.Models;
using ProductEngine.Services;

namespace AphiwatPOS.Pages.Inventory.CurrentStock;

public sealed class IndexModel : PageModel
{
    private readonly IInventoryLocationService _locationService;
    private readonly IInventoryStockService _stockService;
    private readonly IInventoryMovementService _movementService;
    private readonly IProductService _productService;
    private readonly IProductCategoryService _categoryService;
    private readonly IProductBrandService _brandService;

    public IndexModel(IInventoryLocationService locationService, IInventoryStockService stockService, IInventoryMovementService movementService, IProductService productService, IProductCategoryService categoryService, IProductBrandService brandService)
    {
        _locationService = locationService;
        _stockService = stockService;
        _movementService = movementService;
        _productService = productService;
        _categoryService = categoryService;
        _brandService = brandService;
    }

    [BindProperty(SupportsGet = true)] public string? SearchText { get; set; }
    [BindProperty(SupportsGet = true)] public int? LocationId { get; set; }
    [BindProperty(SupportsGet = true)] public int? CategoryId { get; set; }
    [BindProperty(SupportsGet = true)] public int? BrandId { get; set; }
    [BindProperty(SupportsGet = true)] public string? StockStatus { get; set; }
    [BindProperty(SupportsGet = true)] public string? ScanBarcode { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

    public InventoryStockPagedResultModel Stocks { get; private set; } = new();
    public IReadOnlyCollection<InventoryStockRow> StockRows { get; private set; } = Array.Empty<InventoryStockRow>();
    public InventoryStockSummaryModel Summary { get; private set; } = new();
    public IReadOnlyCollection<InventoryLocationModel> Locations { get; private set; } = Array.Empty<InventoryLocationModel>();
    public IReadOnlyCollection<ProductCategoryModel> Categories { get; private set; } = Array.Empty<ProductCategoryModel>();
    public IReadOnlyCollection<ProductBrandModel> Brands { get; private set; } = Array.Empty<ProductBrandModel>();
    public IReadOnlyCollection<InventoryMovementModel> RecentMovements { get; private set; } = Array.Empty<InventoryMovementModel>();
    public decimal TotalStockValue { get; private set; }
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }
    public long? ScannedStockRowId { get; private set; }
    public InventoryActionPermissions Permissions { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        StatusMessage = TempData["StatusMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        await LoadAsync(cancellationToken);
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            Permissions = BuildPermissions();
            Locations = await _locationService.GetAllActiveAsync(cancellationToken);
            Categories = await _categoryService.GetAllActiveAsync(cancellationToken);
            Brands = await _brandService.GetAllActiveAsync(cancellationToken);
            Summary = await _stockService.GetSummaryAsync(LocationId, cancellationToken);

            if (!string.IsNullOrWhiteSpace(ScanBarcode))
            {
                await LoadScannedProductAsync(cancellationToken);
                return;
            }

            Stocks = StockStatus switch
            {
                "Low Stock" => await _stockService.GetLowStockPagedAsync(RequestModel(), cancellationToken),
                "Out of Stock" => await _stockService.GetOutOfStockPagedAsync(RequestModel(), cancellationToken),
                _ => await _stockService.GetPagedAsync(RequestModel(), cancellationToken)
            };

            var products = (await _productService.GetAllActiveAsync(cancellationToken)).ToDictionary(product => product.ProductId);
            var rows = Stocks.Stocks
                .Select(stock => InventoryUi.ToStockRow(stock, products.GetValueOrDefault(stock.ProductId)))
                .Where(row => !BrandId.HasValue || string.Equals(products.GetValueOrDefault(row.ProductId)?.BrandId?.ToString(), BrandId.Value.ToString(), StringComparison.Ordinal))
                .Where(row => string.IsNullOrWhiteSpace(StockStatus) || StockStatus == "All" || row.StockStatus == StockStatus || StockStatus == "Normal" && row.StockStatus == "Normal")
                .ToArray();

            StockRows = rows;
            TotalStockValue = rows.Sum(row => row.StockValue);
            RecentMovements = (await _movementService.GetPagedAsync(new InventoryMovementPagedRequestModel { PageNumber = 1, PageSize = 8, LocationId = LocationId }, cancellationToken)).Movements;
        }
        catch
        {
            ErrorMessage ??= "Failed to load current stock. Confirm the Inventory SQL has been deployed.";
        }
    }

    private async Task LoadScannedProductAsync(CancellationToken cancellationToken)
    {
        var barcode = ScanBarcode?.Trim() ?? string.Empty;
        var product = await _productService.GetByBarcodeAsync(barcode, cancellationToken);
        if (product is null)
        {
            ErrorMessage = $"No product was found for barcode {barcode}.";
            Stocks = new InventoryStockPagedResultModel { PageNumber = 1, PageSize = 10 };
            StockRows = Array.Empty<InventoryStockRow>();
            RecentMovements = Array.Empty<InventoryMovementModel>();
            return;
        }

        var stockModels = (await _stockService.GetByProductIdAsync(product.ProductId, cancellationToken))
            .Where(stock => !LocationId.HasValue || stock.LocationId == LocationId.Value)
            .ToArray();
        var stockRows = stockModels
            .Select(stock => InventoryUi.ToStockRow(stock, product))
            .ToArray();

        Stocks = new InventoryStockPagedResultModel
        {
            Stocks = stockModels,
            TotalCount = stockRows.Length,
            PageNumber = 1,
            PageSize = Math.Max(10, stockRows.Length)
        };

        StockRows = stockRows;
        TotalStockValue = stockRows.Sum(row => row.StockValue);
        ScannedStockRowId = stockRows.FirstOrDefault()?.InventoryStockId;
        RecentMovements = (await _movementService.GetByProductIdAsync(product.ProductId, cancellationToken))
            .Where(movement => !LocationId.HasValue || movement.LocationId == LocationId.Value)
            .ToArray();

        if (stockRows.Length == 0)
        {
            ErrorMessage = $"Product {product.ProductName} was found, but it has no stock count for the selected location.";
        }
    }

    private InventoryStockPagedRequestModel RequestModel() => new()
    {
        PageNumber = PageNumber,
        PageSize = 10,
        SearchText = SearchText,
        LocationId = LocationId,
        CategoryId = CategoryId
    };

    private InventoryActionPermissions BuildPermissions() => new()
    {
        CanView = InventoryUi.HasAnyInventoryAccess(User),
        CanCreate = InventoryUi.HasPermission(User, "INVENTORY_ADJUSTMENT_CREATE"),
        CanUpdate = InventoryUi.HasPermission(User, "INVENTORY_ADJUSTMENT_CREATE"),
        CanApprove = InventoryUi.HasPermission(User, "INVENTORY_ADJUSTMENT_APPROVE"),
        CanCancel = InventoryUi.HasPermission(User, "INVENTORY_ADJUSTMENT_CANCEL")
    };
}
