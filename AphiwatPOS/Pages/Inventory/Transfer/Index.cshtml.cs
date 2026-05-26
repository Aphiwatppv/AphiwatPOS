using System.ComponentModel.DataAnnotations;
using InventoryEngine.Models;
using InventoryEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductEngine.Models;
using ProductEngine.Services;

namespace AphiwatPOS.Pages.Inventory.Transfer;

public sealed class IndexModel : PageModel
{
    private readonly IInventoryLocationService _locationService;
    private readonly IInventoryStockService _stockService;
    private readonly IStockTransferService _transferService;
    private readonly IProductService _productService;

    public IndexModel(IInventoryLocationService locationService, IInventoryStockService stockService, IStockTransferService transferService, IProductService productService)
    {
        _locationService = locationService;
        _stockService = stockService;
        _transferService = transferService;
        _productService = productService;
    }

    [BindProperty(SupportsGet = true)] public string? SearchText { get; set; }
    [BindProperty(SupportsGet = true)] public int? SourceLocationId { get; set; }
    [BindProperty(SupportsGet = true)] public int? DestinationLocationId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? FromDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? ToDate { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

    [BindProperty] public TransferInput Input { get; set; } = new();
    [BindProperty] public ActionInput Action { get; set; } = new();

    public PagedResultModel<StockTransferModel> Transfers { get; private set; } = new();
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
            if (Input.SourceLocationId <= 0 || Input.DestinationLocationId <= 0) throw new InvalidOperationException("From and to locations are required.");
            if (Input.SourceLocationId == Input.DestinationLocationId) throw new InvalidOperationException("From and to locations cannot be the same.");
            if (!Input.Items.Any(i => i.ProductId > 0 && i.Quantity > 0)) throw new InvalidOperationException("At least one transfer item is required.");
            var transferId = await _transferService.CreateAsync(new StockTransferCreateModel { SourceLocationId = Input.SourceLocationId, DestinationLocationId = Input.DestinationLocationId, Remarks = Input.Remarks, CreatedByUserId = InventoryUi.CurrentUserId(User) }, cancellationToken);
            foreach (var item in Input.Items.Where(i => i.ProductId > 0 && i.Quantity > 0))
            {
                await _transferService.AddItemAsync(transferId, item.ProductId, item.Quantity, item.UnitCost, cancellationToken);
            }
            TempData["StatusMessage"] = "Stock transfer draft saved.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostSendAsync(CancellationToken cancellationToken)
    {
        await _transferService.SendAsync(new StockTransferSendModel { StockTransferId = Action.Id, SentByUserId = InventoryUi.CurrentUserId(User) }, cancellationToken);
        TempData["StatusMessage"] = "Transfer sent and TransferOut movements created.";
        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostReceiveAsync(CancellationToken cancellationToken)
    {
        await _transferService.ReceiveAsync(new StockTransferReceiveModel { StockTransferId = Action.Id, ReceivedByUserId = InventoryUi.CurrentUserId(User) }, cancellationToken);
        TempData["StatusMessage"] = "Transfer received and TransferIn movements created.";
        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostCancelAsync(CancellationToken cancellationToken)
    {
        await _transferService.CancelAsync(Action.Id, InventoryUi.CurrentUserId(User), cancellationToken);
        TempData["StatusMessage"] = "Transfer cancelled.";
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
                CanCreate = InventoryUi.HasPermission(User, "INVENTORY_TRANSFER_CREATE"),
                CanUpdate = InventoryUi.HasPermission(User, "INVENTORY_TRANSFER_UPDATE"),
                CanApprove = InventoryUi.HasPermission(User, "INVENTORY_TRANSFER_SEND"),
                CanCancel = InventoryUi.HasPermission(User, "INVENTORY_TRANSFER_CANCEL")
            };
            Locations = await _locationService.GetAllActiveAsync(cancellationToken);
            Products = await _productService.GetAllActiveAsync(cancellationToken);
            Transfers = await _transferService.GetPagedAsync(PageNumber, 10, SearchText, Status, SourceLocationId, DestinationLocationId, cancellationToken);
            if (SourceLocationId.HasValue)
            {
                foreach (var product in Products)
                {
                    var stock = await _stockService.GetByProductIdAsync(product.ProductId, cancellationToken);
                    StockByProduct[product.ProductId] = stock.FirstOrDefault(s => s.LocationId == SourceLocationId)?.CurrentStock ?? 0;
                }
            }
        }
        catch
        {
            ErrorMessage ??= "Failed to load stock transfers.";
        }
    }

    private IActionResult RedirectToCurrentFilters() => RedirectToPage(new { SearchText, SourceLocationId, DestinationLocationId, Status, FromDate, ToDate, PageNumber });

    public sealed class TransferInput
    {
        [Range(1, int.MaxValue)] public int SourceLocationId { get; set; }
        [Range(1, int.MaxValue)] public int DestinationLocationId { get; set; }
        public string? Remarks { get; set; }
        public List<TransferItemInput> Items { get; set; } = new();
    }

    public sealed class TransferItemInput
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
    }

    public sealed class ActionInput
    {
        public long Id { get; set; }
        public string? Reason { get; set; }
    }
}
