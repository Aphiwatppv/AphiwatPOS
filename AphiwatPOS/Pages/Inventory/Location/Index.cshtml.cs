using System.ComponentModel.DataAnnotations;
using AphiwatPOS.Pages.Products;
using InventoryEngine.Models;
using InventoryEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AphiwatPOS.Pages.Inventory.Location;

public sealed class IndexModel : PageModel
{
    private readonly IInventoryLocationService _locationService;
    private readonly IInventoryStockService _stockService;

    public IndexModel(IInventoryLocationService locationService, IInventoryStockService stockService)
    {
        _locationService = locationService;
        _stockService = stockService;
    }

    [BindProperty(SupportsGet = true)] public string? SearchText { get; set; }
    [BindProperty(SupportsGet = true)] public bool? IsActive { get; set; }
    [BindProperty] public LocationInput Input { get; set; } = new();

    public IReadOnlyCollection<InventoryLocationModel> Locations { get; private set; } = Array.Empty<InventoryLocationModel>();
    public Dictionary<int, InventoryStockSummaryModel> SummaryByLocationId { get; private set; } = new();
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool CanManage { get; private set; }
    public string GeneratedLocationCode { get; private set; } = string.Empty;

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
            if (!CanManageLocation()) return Forbid();

            var locationCode = await GenerateLocationCodeAsync(cancellationToken);
            await _locationService.CreateAsync(new InventoryLocationCreateModel
            {
                LocationCode = locationCode,
                LocationName = Input.LocationName,
                Description = Input.Description,
                IsDefault = Input.IsDefault,
                CreatedByUserId = InventoryUi.CurrentUserId(User)
            }, cancellationToken);

            TempData["StatusMessage"] = "Inventory location saved successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostUpdateAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!CanManageLocation()) return Forbid();

            await _locationService.UpdateAsync(new InventoryLocationUpdateModel
            {
                LocationId = Input.LocationId,
                LocationCode = Input.LocationCode,
                LocationName = Input.LocationName,
                Description = Input.Description,
                IsDefault = Input.IsDefault,
                IsActive = Input.IsActive,
                UpdatedByUserId = InventoryUi.CurrentUserId(User)
            }, cancellationToken);

            TempData["StatusMessage"] = "Inventory location updated successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(int locationId, bool isActive, CancellationToken cancellationToken)
    {
        try
        {
            if (!CanManageLocation()) return Forbid();

            await _locationService.ToggleActiveAsync(locationId, isActive, InventoryUi.CurrentUserId(User), cancellationToken);
            TempData["StatusMessage"] = isActive ? "Location activated." : "Location deactivated.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToCurrentFilters();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            CanManage = CanManageLocation();
            GeneratedLocationCode = await GenerateLocationCodeAsync(cancellationToken);
            var locations = await _locationService.GetAllAsync(cancellationToken);
            Locations = locations
                .Where(location => !IsActive.HasValue || location.IsActive == IsActive.Value)
                .Where(location => string.IsNullOrWhiteSpace(SearchText) ||
                    location.LocationCode.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    location.LocationName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    location.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(location => location.IsDefault)
                .ThenBy(location => location.LocationName)
                .ToArray();

            foreach (var location in Locations)
            {
                SummaryByLocationId[location.LocationId] = await _stockService.GetSummaryAsync(location.LocationId, cancellationToken);
            }
        }
        catch
        {
            ErrorMessage ??= "Failed to load inventory locations.";
        }
    }

    private IActionResult RedirectToCurrentFilters() => RedirectToPage(new { SearchText, IsActive });

    private async Task<string> GenerateLocationCodeAsync(CancellationToken cancellationToken)
    {
        var locations = await _locationService.GetAllAsync(cancellationToken);
        var existingCodes = locations
            .Select(location => location.LocationCode)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return await ProductCodeGenerator.NextAsync(
            "LOCATION",
            2,
            code => Task.FromResult(existingCodes.Contains(code)));
    }

    private bool CanManageLocation()
    {
        return InventoryUi.HasPermission(User, "INVENTORY_LOCATION_MANAGE") ||
            InventoryUi.HasPermission(User, "INVENTORY_LOCATION_UPDATE");
    }

    public sealed class LocationInput
    {
        public int LocationId { get; set; }

        [Required, StringLength(50)]
        public string LocationCode { get; set; } = string.Empty;

        [Required, StringLength(150)]
        public string LocationName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsDefault { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
