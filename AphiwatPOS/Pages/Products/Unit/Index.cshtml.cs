using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AphiwatPOS.Pages.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductEngine.Models;
using ProductEngine.Services;

namespace AphiwatPOS.Pages.Products.Unit;

public sealed class IndexModel : PageModel
{
    private readonly IProductUnitService _unitService;
    private readonly IProductUnitConversionService _conversionService;

    public IndexModel(IProductUnitService unitService, IProductUnitConversionService conversionService)
    {
        _unitService = unitService;
        _conversionService = conversionService;
    }

    [BindProperty(SupportsGet = true)] public string? SearchText { get; set; }
    [BindProperty(SupportsGet = true)] public bool? IsActive { get; set; }
    [BindProperty(SupportsGet = true)] public bool? AllowDecimal { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    [BindProperty] public UnitInput Input { get; set; } = new();
    [BindProperty] public ConversionFormInput ConversionInput { get; set; } = new();

    public ProductUnitPagedResultModel Units { get; private set; } = new();
    public IReadOnlyCollection<ProductUnitModel> ActiveUnits { get; private set; } = Array.Empty<ProductUnitModel>();
    public Dictionary<int, IReadOnlyCollection<ProductUnitConversionModel>> ConversionsByUnitId { get; private set; } = new();
    public string NextUnitCode { get; private set; } = string.Empty;
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        StatusMessage = TempData["StatusMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        await LoadAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        var unitCode = await NextUnitCodeAsync(cancellationToken);
        if (await _unitService.IsCodeExistsAsync(unitCode, null, cancellationToken) || await _unitService.IsNameExistsAsync(Input.UnitName, null, cancellationToken))
        {
            TempData["ErrorMessage"] = "Duplicate unit code or name.";
            return RedirectToCurrentFilters();
        }

        await _unitService.CreateAsync(new ProductUnitCreateModel { UnitCode = unitCode, UnitName = Input.UnitName, UnitSymbol = Input.UnitSymbol, AllowDecimal = Input.AllowDecimal, IsBaseUnit = Input.IsBaseUnit, Description = Input.Description, CreatedByUserId = CurrentUserId() }, cancellationToken);
        TempData["StatusMessage"] = "Unit saved successfully.";
        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostUpdateAsync(CancellationToken cancellationToken)
    {
        if (await _unitService.IsCodeExistsAsync(Input.UnitCode, Input.UnitId, cancellationToken) || await _unitService.IsNameExistsAsync(Input.UnitName, Input.UnitId, cancellationToken))
        {
            TempData["ErrorMessage"] = "Duplicate unit code or name.";
            return RedirectToCurrentFilters();
        }

        await _unitService.UpdateAsync(new ProductUnitUpdateModel { UnitId = Input.UnitId, UnitCode = Input.UnitCode, UnitName = Input.UnitName, UnitSymbol = Input.UnitSymbol, AllowDecimal = Input.AllowDecimal, IsBaseUnit = Input.IsBaseUnit, Description = Input.Description, IsActive = Input.IsActive, UpdatedByUserId = CurrentUserId() }, cancellationToken);
        TempData["StatusMessage"] = "Unit updated successfully.";
        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int unitId, CancellationToken cancellationToken)
    {
        await _unitService.DeactivateAsync(unitId, CurrentUserId(), cancellationToken);
        TempData["StatusMessage"] = "Unit deactivated.";
        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostCreateConversionAsync(CancellationToken cancellationToken)
    {
        if (await _conversionService.IsDuplicateAsync(ConversionInput.FromUnitId, ConversionInput.ToUnitId, null, cancellationToken))
        {
            TempData["ErrorMessage"] = "This unit conversion already exists.";
            return RedirectToCurrentFilters();
        }

        await _conversionService.CreateAsync(new ProductUnitConversionCreateModel { FromUnitId = ConversionInput.FromUnitId, ToUnitId = ConversionInput.ToUnitId, ConversionRate = ConversionInput.ConversionRate, Description = ConversionInput.Description, CreatedByUserId = CurrentUserId() }, cancellationToken);
        TempData["StatusMessage"] = "Unit conversion saved successfully.";
        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostUpdateConversionAsync(CancellationToken cancellationToken)
    {
        if (await _conversionService.IsDuplicateAsync(ConversionInput.FromUnitId, ConversionInput.ToUnitId, ConversionInput.UnitConversionId, cancellationToken))
        {
            TempData["ErrorMessage"] = "This unit conversion already exists.";
            return RedirectToCurrentFilters();
        }

        await _conversionService.UpdateAsync(new ProductUnitConversionUpdateModel { UnitConversionId = ConversionInput.UnitConversionId, FromUnitId = ConversionInput.FromUnitId, ToUnitId = ConversionInput.ToUnitId, ConversionRate = ConversionInput.ConversionRate, Description = ConversionInput.Description, IsActive = ConversionInput.IsActive, UpdatedByUserId = CurrentUserId() }, cancellationToken);
        TempData["StatusMessage"] = "Unit conversion updated successfully.";
        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostDeactivateConversionAsync(int unitConversionId, CancellationToken cancellationToken)
    {
        await _conversionService.DeactivateAsync(unitConversionId, CurrentUserId(), cancellationToken);
        TempData["StatusMessage"] = "Unit conversion deactivated.";
        return RedirectToCurrentFilters();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _unitService.GetPagedAsync(new ProductUnitPagedRequestModel { PageNumber = PageNumber, PageSize = 10, SearchText = SearchText, IsActive = IsActive }, cancellationToken);
            if (AllowDecimal.HasValue)
            {
                result = new ProductUnitPagedResultModel { Units = result.Units.Where(unit => unit.AllowDecimal == AllowDecimal.Value).ToArray(), TotalCount = result.Units.Count(unit => unit.AllowDecimal == AllowDecimal.Value), PageNumber = result.PageNumber, PageSize = result.PageSize };
            }

            Units = result;
            ActiveUnits = await _unitService.GetAllActiveAsync(cancellationToken);
            NextUnitCode = await NextUnitCodeAsync(cancellationToken);
            foreach (var unit in Units.Units)
            {
                ConversionsByUnitId[unit.UnitId] = await _conversionService.GetByUnitIdAsync(unit.UnitId, cancellationToken);
            }
        }
        catch
        {
            ErrorMessage ??= "Failed to load unit data. Confirm the Product Management SQL has been deployed.";
        }
    }

    private IActionResult RedirectToCurrentFilters() => RedirectToPage(new { SearchText, IsActive, AllowDecimal, PageNumber });
    private int CurrentUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    private Task<string> NextUnitCodeAsync(CancellationToken cancellationToken) =>
        ProductCodeGenerator.NextAsync("UNIT", 1, code => _unitService.IsCodeExistsAsync(code, null, cancellationToken));

    public sealed class UnitInput
    {
        public int UnitId { get; set; }
        [Required, StringLength(50)] public string UnitCode { get; set; } = string.Empty;
        [Required, StringLength(100)] public string UnitName { get; set; } = string.Empty;
        public string? UnitSymbol { get; set; }
        public bool AllowDecimal { get; set; }
        public bool IsBaseUnit { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public sealed class ConversionFormInput
    {
        public int UnitConversionId { get; set; }
        [Range(1, int.MaxValue)] public int FromUnitId { get; set; }
        [Range(1, int.MaxValue)] public int ToUnitId { get; set; }
        [Range(0.000001, double.MaxValue)] public decimal ConversionRate { get; set; } = 1;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
