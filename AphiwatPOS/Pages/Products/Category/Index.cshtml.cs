using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AphiwatPOS.Pages.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductEngine.Models;
using ProductEngine.Services;

namespace AphiwatPOS.Pages.Products.Category;

public sealed class IndexModel : PageModel
{
    private readonly IProductCategoryService _categoryService;
    private readonly IProductService _productService;

    public IndexModel(IProductCategoryService categoryService, IProductService productService)
    {
        _categoryService = categoryService;
        _productService = productService;
    }

    [BindProperty(SupportsGet = true)] public string? SearchText { get; set; }
    [BindProperty(SupportsGet = true)] public bool? IsActive { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    [BindProperty] public CategoryInput Input { get; set; } = new();

    public ProductCategoryPagedResultModel Categories { get; private set; } = new();
    public IReadOnlyCollection<ProductModel> Products { get; private set; } = Array.Empty<ProductModel>();
    public string NextCategoryCode { get; private set; } = string.Empty;
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
        var categoryCode = await NextCategoryCodeAsync(cancellationToken);
        if (await _categoryService.IsCodeExistsAsync(categoryCode, null, cancellationToken) || await _categoryService.IsNameExistsAsync(Input.CategoryName, null, cancellationToken))
        {
            TempData["ErrorMessage"] = "Duplicate category code or name.";
            return RedirectToCurrentFilters();
        }

        await _categoryService.CreateAsync(new ProductCategoryCreateModel { CategoryCode = categoryCode, CategoryName = Input.CategoryName, Description = Input.Description, DisplayOrder = Input.DisplayOrder, CreatedByUserId = CurrentUserId() }, cancellationToken);
        TempData["StatusMessage"] = "Category saved successfully.";
        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostUpdateAsync(CancellationToken cancellationToken)
    {
        if (await _categoryService.IsCodeExistsAsync(Input.CategoryCode, Input.CategoryId, cancellationToken) || await _categoryService.IsNameExistsAsync(Input.CategoryName, Input.CategoryId, cancellationToken))
        {
            TempData["ErrorMessage"] = "Duplicate category code or name.";
            return RedirectToCurrentFilters();
        }

        await _categoryService.UpdateAsync(new ProductCategoryUpdateModel { CategoryId = Input.CategoryId, CategoryCode = Input.CategoryCode, CategoryName = Input.CategoryName, Description = Input.Description, DisplayOrder = Input.DisplayOrder, IsActive = Input.IsActive, UpdatedByUserId = CurrentUserId() }, cancellationToken);
        TempData["StatusMessage"] = "Category updated successfully.";
        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int categoryId, CancellationToken cancellationToken)
    {
        await _categoryService.DeactivateAsync(categoryId, CurrentUserId(), cancellationToken);
        TempData["StatusMessage"] = "Category deactivated.";
        return RedirectToCurrentFilters();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            Categories = await _categoryService.GetPagedAsync(new ProductCategoryPagedRequestModel { PageNumber = PageNumber, PageSize = 10, SearchText = SearchText, IsActive = IsActive }, cancellationToken);
            Products = await _productService.GetAllActiveAsync(cancellationToken);
            NextCategoryCode = await NextCategoryCodeAsync(cancellationToken);
        }
        catch
        {
            ErrorMessage ??= "Failed to load category data. Confirm the Product Management SQL has been deployed.";
        }
    }

    private IActionResult RedirectToCurrentFilters() => RedirectToPage(new { SearchText, IsActive, PageNumber });
    private int CurrentUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    private Task<string> NextCategoryCodeAsync(CancellationToken cancellationToken) =>
        ProductCodeGenerator.NextAsync("CATEGORY", 2, code => _categoryService.IsCodeExistsAsync(code, null, cancellationToken));

    public sealed class CategoryInput
    {
        public int CategoryId { get; set; }
        [Required, StringLength(50)] public string CategoryCode { get; set; } = string.Empty;
        [Required, StringLength(100)] public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
