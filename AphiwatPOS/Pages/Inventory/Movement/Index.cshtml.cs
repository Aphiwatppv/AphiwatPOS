using InventoryEngine.Models;
using InventoryEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductEngine.Models;
using ProductEngine.Services;

namespace AphiwatPOS.Pages.Inventory.Movement;

public sealed class IndexModel : PageModel
{
    private readonly IInventoryLocationService _locationService;
    private readonly IInventoryMovementService _movementService;
    private readonly IProductService _productService;

    public IndexModel(IInventoryLocationService locationService, IInventoryMovementService movementService, IProductService productService)
    {
        _locationService = locationService;
        _movementService = movementService;
        _productService = productService;
    }

    [BindProperty(SupportsGet = true)] public string? SearchText { get; set; }
    [BindProperty(SupportsGet = true)] public int? ProductId { get; set; }
    [BindProperty(SupportsGet = true)] public int? LocationId { get; set; }
    [BindProperty(SupportsGet = true)] public string? MovementType { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? FromDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? ToDate { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

    public InventoryMovementPagedResultModel Movements { get; private set; } = new();
    public IReadOnlyCollection<InventoryLocationModel> Locations { get; private set; } = Array.Empty<InventoryLocationModel>();
    public IReadOnlyCollection<ProductModel> Products { get; private set; } = Array.Empty<ProductModel>();
    public IReadOnlyCollection<InventoryMovementSummaryModel> SummaryRows { get; private set; } = Array.Empty<InventoryMovementSummaryModel>();
    public decimal StockInQty { get; private set; }
    public decimal StockOutQty { get; private set; }
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        StatusMessage = TempData["StatusMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        try
        {
            Locations = await _locationService.GetAllActiveAsync(cancellationToken);
            Products = await _productService.GetAllActiveAsync(cancellationToken);
            Movements = await _movementService.GetPagedAsync(new InventoryMovementPagedRequestModel
            {
                PageNumber = PageNumber,
                PageSize = 10,
                SearchText = SearchText,
                ProductId = ProductId,
                LocationId = LocationId,
                MovementType = MovementType,
                FromDate = FromDate,
                ToDate = ToDate
            }, cancellationToken);
            SummaryRows = await _movementService.GetSummaryByDateRangeAsync(FromDate ?? DateTime.UtcNow.AddDays(-30), ToDate ?? DateTime.UtcNow, LocationId, cancellationToken);
            StockInQty = Movements.Movements.Where(m => !InventoryUi.IsOutMovement(m.MovementType, m.QuantitySigned)).Sum(m => Math.Abs(m.QuantitySigned));
            StockOutQty = Movements.Movements.Where(m => InventoryUi.IsOutMovement(m.MovementType, m.QuantitySigned)).Sum(m => Math.Abs(m.QuantitySigned));
        }
        catch
        {
            ErrorMessage ??= "Failed to load stock movements.";
        }
    }
}
