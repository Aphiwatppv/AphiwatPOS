using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AphiwatPOS.Pages.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductEngine.Models;
using ProductEngine.Services;

namespace AphiwatPOS.Pages.Products.Brand;

public sealed class IndexModel : PageModel
{
    private readonly IProductBrandService _brandService;
    private readonly IProductService _productService;
    private readonly IWebHostEnvironment _environment;

    public IndexModel(IProductBrandService brandService, IProductService productService, IWebHostEnvironment environment)
    {
        _brandService = brandService;
        _productService = productService;
        _environment = environment;
    }

    [BindProperty(SupportsGet = true)] public string? SearchText { get; set; }
    [BindProperty(SupportsGet = true)] public bool? IsActive { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    [BindProperty] public BrandInput Input { get; set; } = new();

    public ProductBrandPagedResultModel Brands { get; private set; } = new();
    public IReadOnlyCollection<ProductModel> Products { get; private set; } = Array.Empty<ProductModel>();
    public string NextBrandCode { get; private set; } = string.Empty;
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
        var brandCode = await NextBrandCodeAsync(cancellationToken);
        if (await _brandService.IsCodeExistsAsync(brandCode, null, cancellationToken) || await _brandService.IsNameExistsAsync(Input.BrandName, null, cancellationToken))
        {
            TempData["ErrorMessage"] = "Duplicate brand code or name.";
            return RedirectToCurrentFilters();
        }

        var logoUrl = await SaveImageAsync(Input.LogoFile, cancellationToken) ?? Input.ExistingLogoUrl ?? string.Empty;
        await _brandService.CreateAsync(new ProductBrandCreateModel { BrandCode = brandCode, BrandName = Input.BrandName, Description = Input.Description, LogoUrl = logoUrl, CreatedByUserId = CurrentUserId() }, cancellationToken);
        TempData["StatusMessage"] = "Brand saved successfully.";
        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostUpdateAsync(CancellationToken cancellationToken)
    {
        if (await _brandService.IsCodeExistsAsync(Input.BrandCode, Input.BrandId, cancellationToken) || await _brandService.IsNameExistsAsync(Input.BrandName, Input.BrandId, cancellationToken))
        {
            TempData["ErrorMessage"] = "Duplicate brand code or name.";
            return RedirectToCurrentFilters();
        }

        var logoUrl = await SaveImageAsync(Input.LogoFile, cancellationToken) ?? Input.ExistingLogoUrl ?? string.Empty;
        await _brandService.UpdateAsync(new ProductBrandUpdateModel { BrandId = Input.BrandId, BrandCode = Input.BrandCode, BrandName = Input.BrandName, Description = Input.Description, LogoUrl = logoUrl, IsActive = Input.IsActive, UpdatedByUserId = CurrentUserId() }, cancellationToken);
        TempData["StatusMessage"] = "Brand updated successfully.";
        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int brandId, CancellationToken cancellationToken)
    {
        await _brandService.DeactivateAsync(brandId, CurrentUserId(), cancellationToken);
        TempData["StatusMessage"] = "Brand deactivated.";
        return RedirectToCurrentFilters();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            Brands = await _brandService.GetPagedAsync(new ProductBrandPagedRequestModel { PageNumber = PageNumber, PageSize = 10, SearchText = SearchText, IsActive = IsActive }, cancellationToken);
            Products = await _productService.GetAllActiveAsync(cancellationToken);
            NextBrandCode = await NextBrandCodeAsync(cancellationToken);
        }
        catch
        {
            ErrorMessage ??= "Failed to load brand data. Confirm the Product Management SQL has been deployed.";
        }
    }

    private IActionResult RedirectToCurrentFilters() => RedirectToPage(new { SearchText, IsActive, PageNumber });
    private int CurrentUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    private Task<string> NextBrandCodeAsync(CancellationToken cancellationToken) =>
        ProductCodeGenerator.NextAsync("BRAND", 2, code => _brandService.IsCodeExistsAsync(code, null, cancellationToken));

    private async Task<string?> SaveImageAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0) return null;
        var folder = Path.Combine(_environment.WebRootPath, "uploads", "brands");
        Directory.CreateDirectory(folder);
        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(folder, fileName);
        await using var stream = System.IO.File.Create(path);
        await file.CopyToAsync(stream, cancellationToken);
        return $"/uploads/brands/{fileName}";
    }

    public sealed class BrandInput
    {
        public int BrandId { get; set; }
        [Required, StringLength(50)] public string BrandCode { get; set; } = string.Empty;
        [Required, StringLength(100)] public string BrandName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public IFormFile? LogoFile { get; set; }
        public string? ExistingLogoUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
