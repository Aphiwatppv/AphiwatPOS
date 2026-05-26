using InventoryEngine.Models;
using InventoryEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductEngine.Models;
using ProductEngine.Services;

namespace AphiwatPOS.Pages.Inventory.LowStock;

public sealed class IndexModel : PageModel
{
    private readonly IInventoryLocationService _locationService;
    private readonly IInventoryStockService _stockService;
    private readonly IProductService _productService;
    private readonly IProductCategoryService _categoryService;
    private readonly IProductBrandService _brandService;

    public IndexModel(IInventoryLocationService locationService, IInventoryStockService stockService, IProductService productService, IProductCategoryService categoryService, IProductBrandService brandService)
    {
        _locationService = locationService;
        _stockService = stockService;
        _productService = productService;
        _categoryService = categoryService;
        _brandService = brandService;
    }

    [BindProperty(SupportsGet = true)] public string? SearchText { get; set; }
    [BindProperty(SupportsGet = true)] public int? LocationId { get; set; }
    [BindProperty(SupportsGet = true)] public int? CategoryId { get; set; }
    [BindProperty(SupportsGet = true)] public int? BrandId { get; set; }
    [BindProperty(SupportsGet = true)] public string StockStatus { get; set; } = "Low Stock";
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

    public InventoryStockPagedResultModel Stocks { get; private set; } = new();
    public IReadOnlyCollection<InventoryStockRow> StockRows { get; private set; } = Array.Empty<InventoryStockRow>();
    public IReadOnlyCollection<InventoryLocationModel> Locations { get; private set; } = Array.Empty<InventoryLocationModel>();
    public IReadOnlyCollection<ProductCategoryModel> Categories { get; private set; } = Array.Empty<ProductCategoryModel>();
    public IReadOnlyCollection<ProductBrandModel> Brands { get; private set; } = Array.Empty<ProductBrandModel>();
    public decimal EstimatedReorderValue { get; private set; }
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool CanAdjust { get; private set; }

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
            CanAdjust = InventoryUi.HasPermission(User, "INVENTORY_ADJUSTMENT_CREATE");
            Locations = await _locationService.GetAllActiveAsync(cancellationToken);
            Categories = await _categoryService.GetAllActiveAsync(cancellationToken);
            Brands = await _brandService.GetAllActiveAsync(cancellationToken);
            Stocks = StockStatus == "Out of Stock"
                ? await _stockService.GetOutOfStockPagedAsync(RequestModel(), cancellationToken)
                : await _stockService.GetLowStockPagedAsync(RequestModel(), cancellationToken);

            var products = (await _productService.GetAllActiveAsync(cancellationToken)).ToDictionary(product => product.ProductId);
            StockRows = Stocks.Stocks
                .Select(stock => InventoryUi.ToStockRow(stock, products.GetValueOrDefault(stock.ProductId)))
                .Where(row => !BrandId.HasValue || products.GetValueOrDefault(row.ProductId)?.BrandId == BrandId)
                .Where(row => StockStatus != "Out of Stock" || row.CurrentStock <= 0)
                .ToArray();
            EstimatedReorderValue = StockRows.Sum(row => row.ShortageQty * row.CostPrice);
        }
        catch
        {
            ErrorMessage ??= "Failed to load low stock products.";
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
}
