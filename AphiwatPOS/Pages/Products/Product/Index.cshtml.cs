using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AphiwatPOS.Pages.Products;
using InventoryEngine.Models;
using InventoryEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductEngine.Models;
using ProductEngine.Services;

namespace AphiwatPOS.Pages.Products.Product;

public sealed class IndexModel : PageModel
{
    private readonly IProductService _productService;
    private readonly IProductCategoryService _categoryService;
    private readonly IProductBrandService _brandService;
    private readonly IProductUnitService _unitService;
    private readonly IProductPriceHistoryService _priceHistoryService;
    private readonly IInventoryStockService _inventoryStockService;
    private readonly IWebHostEnvironment _environment;

    public IndexModel(
        IProductService productService,
        IProductCategoryService categoryService,
        IProductBrandService brandService,
        IProductUnitService unitService,
        IProductPriceHistoryService priceHistoryService,
        IInventoryStockService inventoryStockService,
        IWebHostEnvironment environment)
    {
        _productService = productService;
        _categoryService = categoryService;
        _brandService = brandService;
        _unitService = unitService;
        _priceHistoryService = priceHistoryService;
        _inventoryStockService = inventoryStockService;
        _environment = environment;
    }

    [BindProperty(SupportsGet = true)] public string? SearchText { get; set; }
    [BindProperty(SupportsGet = true)] public int? CategoryId { get; set; }
    [BindProperty(SupportsGet = true)] public int? BrandId { get; set; }
    [BindProperty(SupportsGet = true)] public int? UnitId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public bool? IsActive { get; set; }
    [BindProperty(SupportsGet = true)] public bool LowStockOnly { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public string? Mode { get; set; }
    [BindProperty(SupportsGet = true)] public string? Barcode { get; set; }

    [BindProperty] public ProductFormInput ProductInput { get; set; } = new();
    [BindProperty] public PriceUpdateInput PriceInput { get; set; } = new();
    [BindProperty] public BarcodeUpdateInput BarcodeInput { get; set; } = new();
    [BindProperty] public ImageUpdateInput ImageInput { get; set; } = new();

    public ProductPagedResultModel Products { get; private set; } = new();
    public IReadOnlyCollection<ProductCategoryModel> Categories { get; private set; } = Array.Empty<ProductCategoryModel>();
    public IReadOnlyCollection<ProductBrandModel> Brands { get; private set; } = Array.Empty<ProductBrandModel>();
    public IReadOnlyCollection<ProductUnitModel> Units { get; private set; } = Array.Empty<ProductUnitModel>();
    public IReadOnlyCollection<ProductModel> LowStockProducts { get; private set; } = Array.Empty<ProductModel>();
    public Dictionary<int, IReadOnlyCollection<ProductPriceHistoryModel>> PriceHistoryByProductId { get; private set; } = new();
    public Dictionary<int, IReadOnlyCollection<InventoryStockModel>> InventoryStockByProductId { get; private set; } = new();
    public int InventoryLowStockCount { get; private set; }
    public string NextProductCode { get; private set; } = string.Empty;
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        StatusMessage = TempData["StatusMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        if (Mode?.Equals("create", StringComparison.OrdinalIgnoreCase) == true && !string.IsNullOrWhiteSpace(Barcode))
        {
            ProductInput.Barcode = Barcode.Trim();
        }
        await LoadAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        if (!ValidatePricingInput())
        {
            TempData["ErrorMessage"] = "Selling price cannot be lower than minimum selling price.";
            return RedirectToPage();
        }

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please review the product form and try again.";
            return RedirectToPage();
        }

        var productCode = await NextProductCodeAsync(cancellationToken);
        var isBarcodeGenerated = string.IsNullOrWhiteSpace(ProductInput.Barcode);
        var barcode = isBarcodeGenerated ? await NextBarcodeAsync(cancellationToken) : ProductInput.Barcode!.Trim();
        if (!isBarcodeGenerated && await _productService.IsBarcodeExistsAsync(barcode, null, cancellationToken))
        {
            TempData["ErrorMessage"] = "This barcode is already used by another product.";
            return RedirectToPage();
        }

        var imageUrl = await SaveImageAsync(ProductInput.ProductImage, cancellationToken) ?? ProductInput.ExistingImageUrl ?? string.Empty;
        try
        {
            await _productService.CreateAsync(new ProductCreateModel
            {
                ProductCode = productCode,
                SKU = ProductInput.SKU,
                Barcode = barcode,
                ProductName = ProductInput.ProductName,
                CategoryId = ProductInput.CategoryId,
                BrandId = ProductInput.BrandId,
                UnitId = ProductInput.UnitId,
                CostPrice = ProductInput.CostPrice,
                MinimumCost = ProductInput.MinimumCost,
                VatMode = ProductInput.VatMode,
                VatPercentage = ProductInput.VatPercentage,
                VatAmount = ProductInput.VatAmount,
                MinimumSellingPrice = ProductInput.MinimumSellingPrice,
                SellingPrice = ProductInput.SellingPrice,
                WholesalePrice = ProductInput.WholesalePrice,
                WholesaleMinQty = ProductInput.WholesaleMinQty,
                TaxRate = ProductInput.VatPercentage,
                DiscountAllowed = ProductInput.DiscountAllowed,
                IsStockTracked = ProductInput.IsStockTracked,
                MinimumStockLevel = ProductInput.MinimumStockLevel,
                CurrentStock = 0,
                ProductImageUrl = imageUrl,
                Description = ProductInput.Description,
                Status = ProductInput.Status,
                CreatedByUserId = CurrentUserId()
            }, cancellationToken);
        }
        catch (Exception ex) when (ex.Message.Contains("minimum selling price", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Selling price cannot be lower than minimum selling price.";
            return RedirectToPage();
        }

        TempData["StatusMessage"] = isBarcodeGenerated
            ? $"Product saved successfully. Generated barcode {barcode}."
            : "Product saved successfully.";
        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostUpdateAsync(CancellationToken cancellationToken)
    {
        if (!ValidatePricingInput())
        {
            TempData["ErrorMessage"] = "Selling price cannot be lower than minimum selling price.";
            return RedirectToCurrentFilters();
        }

        var barcode = string.IsNullOrWhiteSpace(ProductInput.Barcode) ? null : ProductInput.Barcode.Trim();
        if (!string.IsNullOrWhiteSpace(barcode) &&
            await _productService.IsBarcodeExistsAsync(barcode, ProductInput.ProductId, cancellationToken))
        {
            TempData["ErrorMessage"] = "This barcode is already used by another product.";
            return RedirectToCurrentFilters();
        }

        var imageUrl = await SaveImageAsync(ProductInput.ProductImage, cancellationToken) ?? ProductInput.ExistingImageUrl ?? string.Empty;
        var existingProduct = await _productService.GetByIdAsync(ProductInput.ProductId, cancellationToken);
        try
        {
            await _productService.UpdateAsync(new ProductUpdateModel
            {
                ProductId = ProductInput.ProductId,
                ProductCode = ProductInput.ProductCode,
                SKU = ProductInput.SKU,
                Barcode = barcode,
                ProductName = ProductInput.ProductName,
                CategoryId = ProductInput.CategoryId,
                BrandId = ProductInput.BrandId,
                UnitId = ProductInput.UnitId,
                CostPrice = ProductInput.CostPrice,
                MinimumCost = ProductInput.MinimumCost,
                VatMode = ProductInput.VatMode,
                VatPercentage = ProductInput.VatPercentage,
                VatAmount = ProductInput.VatAmount,
                MinimumSellingPrice = ProductInput.MinimumSellingPrice,
                SellingPrice = ProductInput.SellingPrice,
                WholesalePrice = ProductInput.WholesalePrice,
                WholesaleMinQty = ProductInput.WholesaleMinQty,
                TaxRate = ProductInput.VatPercentage,
                DiscountAllowed = ProductInput.DiscountAllowed,
                IsStockTracked = ProductInput.IsStockTracked,
                MinimumStockLevel = ProductInput.MinimumStockLevel,
                CurrentStock = existingProduct?.CurrentStock ?? 0,
                ProductImageUrl = imageUrl,
                Description = ProductInput.Description,
                Status = ProductInput.Status,
                IsActive = ProductInput.IsActive,
                UpdatedByUserId = CurrentUserId()
            }, cancellationToken);
        }
        catch (Exception ex) when (ex.Message.Contains("minimum selling price", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Selling price cannot be lower than minimum selling price.";
            return RedirectToCurrentFilters();
        }

        TempData["StatusMessage"] = "Product updated successfully.";
        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostUpdatePriceAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _productService.UpdatePriceAsync(new ProductPriceUpdateModel
            {
                ProductId = PriceInput.ProductId,
                NewCostPrice = PriceInput.NewCostPrice,
                NewSellingPrice = PriceInput.NewSellingPrice,
                NewWholesalePrice = PriceInput.NewWholesalePrice,
                NewWholesaleMinQty = PriceInput.NewWholesaleMinQty,
                ChangeReason = PriceInput.ChangeReason,
                ChangedByUserId = CurrentUserId()
            }, cancellationToken);
        }
        catch (Exception ex) when (ex.Message.Contains("minimum selling price", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Selling price cannot be lower than minimum selling price.";
            return RedirectToCurrentFilters();
        }

        TempData["StatusMessage"] = "Product price updated and history saved.";
        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostUpdateBarcodeAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(BarcodeInput.NewBarcode) &&
            await _productService.IsBarcodeExistsAsync(BarcodeInput.NewBarcode, BarcodeInput.ProductId, cancellationToken))
        {
            TempData["ErrorMessage"] = "This barcode is already used by another product.";
            return RedirectToCurrentFilters();
        }

        await _productService.UpdateBarcodeAsync(new ProductBarcodeUpdateModel { ProductId = BarcodeInput.ProductId, Barcode = BarcodeInput.NewBarcode, UpdatedByUserId = CurrentUserId() }, cancellationToken);
        TempData["StatusMessage"] = "Barcode updated successfully.";
        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostUpdateImageAsync(CancellationToken cancellationToken)
    {
        var imageUrl = await SaveImageAsync(ImageInput.ProductImage, cancellationToken) ?? ImageInput.ExistingImageUrl ?? string.Empty;
        await _productService.UpdateImageAsync(new ProductImageUpdateModel { ProductId = ImageInput.ProductId, ProductImageUrl = imageUrl, UpdatedByUserId = CurrentUserId() }, cancellationToken);
        TempData["StatusMessage"] = "Product image updated successfully.";
        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int productId, CancellationToken cancellationToken)
    {
        await _productService.DeactivateAsync(productId, CurrentUserId(), cancellationToken);
        TempData["StatusMessage"] = "Product deactivated.";
        return RedirectToCurrentFilters();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            Categories = await _categoryService.GetAllActiveAsync(cancellationToken);
            Brands = await _brandService.GetAllActiveAsync(cancellationToken);
            Units = await _unitService.GetAllActiveAsync(cancellationToken);
            NextProductCode = await NextProductCodeAsync(cancellationToken);
            Products = await _productService.GetPagedAsync(new ProductPagedRequestModel
            {
                PageNumber = PageNumber,
                PageSize = 10,
                SearchText = SearchText,
                CategoryId = CategoryId,
                BrandId = BrandId,
                UnitId = UnitId,
                Status = Status,
                IsActive = IsActive,
                LowStockOnly = false
            }, cancellationToken);

            foreach (var product in Products.Products)
            {
                PriceHistoryByProductId[product.ProductId] = await _priceHistoryService.GetByProductIdAsync(product.ProductId, cancellationToken);
                InventoryStockByProductId[product.ProductId] = await _inventoryStockService.GetByProductIdAsync(product.ProductId, cancellationToken);
            }

            var summary = await _inventoryStockService.GetSummaryAsync(null, cancellationToken);
            InventoryLowStockCount = summary.LowStockCount;
        }
        catch
        {
            ErrorMessage ??= "Failed to load product data. Confirm the Product and Inventory SQL has been deployed.";
        }
    }

    private IActionResult RedirectToCurrentFilters() => RedirectToPage(new { SearchText, CategoryId, BrandId, UnitId, Status, IsActive, LowStockOnly, PageNumber });

    private int CurrentUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    private Task<string> NextProductCodeAsync(CancellationToken cancellationToken)
    {
        return ProductCodeGenerator.NextAsync(
            "PRO",
            2,
            code => _productService.IsCodeExistsAsync(code, null, cancellationToken));
    }

    private async Task<string> NextBarcodeAsync(CancellationToken cancellationToken)
    {
        for (var sequence = 1L; sequence <= 9_999_999_999L; sequence++)
        {
            var body = $"20{sequence:0000000000}";
            var barcode = $"{body}{CalculateEan13CheckDigit(body)}";
            if (!await _productService.IsBarcodeExistsAsync(barcode, null, cancellationToken))
            {
                return barcode;
            }
        }

        throw new InvalidOperationException("Unable to generate the next barcode.");
    }

    private static int CalculateEan13CheckDigit(string firstTwelveDigits)
    {
        var sum = 0;
        for (var index = 0; index < firstTwelveDigits.Length; index++)
        {
            var digit = firstTwelveDigits[index] - '0';
            sum += index % 2 == 0 ? digit : digit * 3;
        }

        return (10 - sum % 10) % 10;
    }

    private async Task<string?> SaveImageAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0) return null;
        var folder = Path.Combine(_environment.WebRootPath, "uploads", "products");
        Directory.CreateDirectory(folder);
        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(folder, fileName);
        await using var stream = System.IO.File.Create(path);
        await file.CopyToAsync(stream, cancellationToken);
        return $"/uploads/products/{fileName}";
    }

    private bool ValidatePricingInput()
    {
        ProductInput.VatMode = ProductInput.VatMode switch
        {
            "NoVat" or "VatIncluded" or "VatExcluded" => ProductInput.VatMode,
            _ => "VatExcluded"
        };

        if (ProductInput.VatMode == "NoVat")
        {
            ProductInput.VatPercentage = 0;
        }

        var vatAmount = ProductInput.VatMode switch
        {
            "NoVat" => 0,
            "VatIncluded" when ProductInput.VatPercentage > 0 => ProductInput.MinimumCost * ProductInput.VatPercentage / (100 + ProductInput.VatPercentage),
            "VatIncluded" => 0,
            _ => ProductInput.MinimumCost * ProductInput.VatPercentage / 100
        };
        var minimumSellingPrice = ProductInput.VatMode == "VatExcluded" ? ProductInput.MinimumCost + vatAmount : ProductInput.MinimumCost;
        ProductInput.VatAmount = vatAmount;
        ProductInput.MinimumSellingPrice = minimumSellingPrice;
        ProductInput.TaxRate = ProductInput.VatMode == "NoVat" ? 0 : ProductInput.VatPercentage;
        return ProductInput.SellingPrice >= minimumSellingPrice;
    }

    public sealed class ProductFormInput
    {
        public int ProductId { get; set; }
        [Required, StringLength(50)] public string ProductCode { get; set; } = string.Empty;
        [StringLength(100)] public string? SKU { get; set; }
        [StringLength(100)] public string? Barcode { get; set; }
        [Required, StringLength(200)] public string ProductName { get; set; } = string.Empty;
        [Range(1, int.MaxValue)] public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        [Range(1, int.MaxValue)] public int UnitId { get; set; }
        [Range(0, double.MaxValue)] public decimal CostPrice { get; set; }
        [Range(0, double.MaxValue)] public decimal MinimumCost { get; set; }
        public string VatMode { get; set; } = "VatExcluded";
        [Range(0, 100)] public decimal VatPercentage { get; set; }
        [Range(0, double.MaxValue)] public decimal VatAmount { get; set; }
        [Range(0, double.MaxValue)] public decimal MinimumSellingPrice { get; set; }
        [Range(0, double.MaxValue)] public decimal SellingPrice { get; set; }
        [Range(0, double.MaxValue)] public decimal WholesalePrice { get; set; }
        [Range(0, double.MaxValue)] public decimal WholesaleMinQty { get; set; } = 1;
        [Range(0, 100)] public decimal TaxRate { get; set; }
        public bool DiscountAllowed { get; set; } = true;
        public bool IsStockTracked { get; set; } = true;
        [Range(0, double.MaxValue)] public decimal MinimumStockLevel { get; set; }
        public IFormFile? ProductImage { get; set; }
        public string? ExistingImageUrl { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = "Active";
        public bool IsActive { get; set; } = true;
    }

    public sealed class PriceUpdateInput
    {
        public int ProductId { get; set; }
        [Range(0, double.MaxValue)] public decimal NewCostPrice { get; set; }
        [Range(0, double.MaxValue)] public decimal NewSellingPrice { get; set; }
        [Range(0, double.MaxValue)] public decimal NewWholesalePrice { get; set; }
        [Range(0, double.MaxValue)] public decimal NewWholesaleMinQty { get; set; } = 1;
        public string? ChangeReason { get; set; }
    }

    public sealed class BarcodeUpdateInput
    {
        public int ProductId { get; set; }
        public string? NewBarcode { get; set; }
    }

    public sealed class ImageUpdateInput
    {
        public int ProductId { get; set; }
        public string? ExistingImageUrl { get; set; }
        public IFormFile? ProductImage { get; set; }
    }
}
