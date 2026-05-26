using AccessEngine.Services;
using ProductEngine.Models;

namespace ProductEngine.Services;

public sealed class ProductCategoryService : IProductCategoryService
{
    private readonly IAccessService _accessService;

    public ProductCategoryService(IAccessService accessService) => _accessService = accessService;

    public async Task<ProductCategoryPagedResultModel> GetPagedAsync(ProductCategoryPagedRequestModel request, CancellationToken cancellationToken = default)
    {
        var rows = (await _accessService.QueryAsync<ProductCategoryPagedRow, object>(
            "dbo.spProductCategoryGetPaged",
            new { request.PageNumber, request.PageSize, SearchText = ProductServiceHelpers.TrimOrNull(request.SearchText), request.IsActive },
            cancellationToken)).ToArray();

        return new ProductCategoryPagedResultModel
        {
            Categories = rows,
            TotalCount = rows.FirstOrDefault()?.TotalCount ?? 0,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<IReadOnlyCollection<ProductCategoryModel>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _accessService.QueryAsync<ProductCategoryModel, object>("dbo.spProductCategoryGetAllActive", new { }, cancellationToken);
        return rows.ToArray();
    }

    public Task<ProductCategoryModel?> GetByIdAsync(int categoryId, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleOrDefaultAsync<ProductCategoryModel, object>("dbo.spProductCategoryGetById", new { CategoryId = categoryId }, cancellationToken);

    public Task<int> CreateAsync(ProductCategoryCreateModel model, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleAsync<int, object>("dbo.spProductCategoryCreate", new
        {
            CategoryCode = model.CategoryCode.Trim(),
            CategoryName = model.CategoryName.Trim(),
            Description = model.Description?.Trim() ?? string.Empty,
            model.DisplayOrder,
            model.CreatedByUserId
        }, cancellationToken);

    public Task UpdateAsync(ProductCategoryUpdateModel model, CancellationToken cancellationToken = default) =>
        _accessService.ExecuteAsync("dbo.spProductCategoryUpdate", new
        {
            model.CategoryId,
            CategoryCode = model.CategoryCode.Trim(),
            CategoryName = model.CategoryName.Trim(),
            Description = model.Description?.Trim() ?? string.Empty,
            model.DisplayOrder,
            model.IsActive,
            model.UpdatedByUserId
        }, cancellationToken);

    public Task DeactivateAsync(int categoryId, int updatedByUserId, CancellationToken cancellationToken = default) =>
        _accessService.ExecuteAsync("dbo.spProductCategoryDeactivate", new { CategoryId = categoryId, UpdatedByUserId = updatedByUserId }, cancellationToken);

    public Task<bool> IsCodeExistsAsync(string categoryCode, int? excludeCategoryId = null, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleAsync<bool, object>("dbo.spProductCategoryCheckCodeExists", new { CategoryCode = categoryCode.Trim(), ExcludeCategoryId = excludeCategoryId }, cancellationToken);

    public Task<bool> IsNameExistsAsync(string categoryName, int? excludeCategoryId = null, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleAsync<bool, object>("dbo.spProductCategoryCheckNameExists", new { CategoryName = categoryName.Trim(), ExcludeCategoryId = excludeCategoryId }, cancellationToken);
}

public sealed class ProductBrandService : IProductBrandService
{
    private readonly IAccessService _accessService;

    public ProductBrandService(IAccessService accessService) => _accessService = accessService;

    public async Task<ProductBrandPagedResultModel> GetPagedAsync(ProductBrandPagedRequestModel request, CancellationToken cancellationToken = default)
    {
        var rows = (await _accessService.QueryAsync<ProductBrandPagedRow, object>(
            "dbo.spProductBrandGetPaged",
            new { request.PageNumber, request.PageSize, SearchText = ProductServiceHelpers.TrimOrNull(request.SearchText), request.IsActive },
            cancellationToken)).ToArray();

        return new ProductBrandPagedResultModel { Brands = rows, TotalCount = rows.FirstOrDefault()?.TotalCount ?? 0, PageNumber = request.PageNumber, PageSize = request.PageSize };
    }

    public async Task<IReadOnlyCollection<ProductBrandModel>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _accessService.QueryAsync<ProductBrandModel, object>("dbo.spProductBrandGetAllActive", new { }, cancellationToken);
        return rows.ToArray();
    }

    public Task<ProductBrandModel?> GetByIdAsync(int brandId, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleOrDefaultAsync<ProductBrandModel, object>("dbo.spProductBrandGetById", new { BrandId = brandId }, cancellationToken);

    public Task<int> CreateAsync(ProductBrandCreateModel model, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleAsync<int, object>("dbo.spProductBrandCreate", new
        {
            BrandCode = model.BrandCode.Trim(),
            BrandName = model.BrandName.Trim(),
            Description = model.Description?.Trim() ?? string.Empty,
            LogoUrl = model.LogoUrl?.Trim() ?? string.Empty,
            model.CreatedByUserId
        }, cancellationToken);

    public Task UpdateAsync(ProductBrandUpdateModel model, CancellationToken cancellationToken = default) =>
        _accessService.ExecuteAsync("dbo.spProductBrandUpdate", new
        {
            model.BrandId,
            BrandCode = model.BrandCode.Trim(),
            BrandName = model.BrandName.Trim(),
            Description = model.Description?.Trim() ?? string.Empty,
            LogoUrl = model.LogoUrl?.Trim() ?? string.Empty,
            model.IsActive,
            model.UpdatedByUserId
        }, cancellationToken);

    public Task DeactivateAsync(int brandId, int updatedByUserId, CancellationToken cancellationToken = default) =>
        _accessService.ExecuteAsync("dbo.spProductBrandDeactivate", new { BrandId = brandId, UpdatedByUserId = updatedByUserId }, cancellationToken);

    public Task<bool> IsCodeExistsAsync(string brandCode, int? excludeBrandId = null, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleAsync<bool, object>("dbo.spProductBrandCheckCodeExists", new { BrandCode = brandCode.Trim(), ExcludeBrandId = excludeBrandId }, cancellationToken);

    public Task<bool> IsNameExistsAsync(string brandName, int? excludeBrandId = null, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleAsync<bool, object>("dbo.spProductBrandCheckNameExists", new { BrandName = brandName.Trim(), ExcludeBrandId = excludeBrandId }, cancellationToken);
}

public sealed class ProductUnitService : IProductUnitService
{
    private readonly IAccessService _accessService;

    public ProductUnitService(IAccessService accessService) => _accessService = accessService;

    public async Task<ProductUnitPagedResultModel> GetPagedAsync(ProductUnitPagedRequestModel request, CancellationToken cancellationToken = default)
    {
        var rows = (await _accessService.QueryAsync<ProductUnitPagedRow, object>(
            "dbo.spProductUnitGetPaged",
            new { request.PageNumber, request.PageSize, SearchText = ProductServiceHelpers.TrimOrNull(request.SearchText), request.IsActive },
            cancellationToken)).ToArray();

        return new ProductUnitPagedResultModel { Units = rows, TotalCount = rows.FirstOrDefault()?.TotalCount ?? 0, PageNumber = request.PageNumber, PageSize = request.PageSize };
    }

    public async Task<IReadOnlyCollection<ProductUnitModel>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _accessService.QueryAsync<ProductUnitModel, object>("dbo.spProductUnitGetAllActive", new { }, cancellationToken);
        return rows.ToArray();
    }

    public Task<ProductUnitModel?> GetByIdAsync(int unitId, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleOrDefaultAsync<ProductUnitModel, object>("dbo.spProductUnitGetById", new { UnitId = unitId }, cancellationToken);

    public Task<int> CreateAsync(ProductUnitCreateModel model, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleAsync<int, object>("dbo.spProductUnitCreate", new
        {
            UnitCode = model.UnitCode.Trim(),
            UnitName = model.UnitName.Trim(),
            UnitSymbol = model.UnitSymbol?.Trim() ?? string.Empty,
            model.AllowDecimal,
            model.IsBaseUnit,
            Description = model.Description?.Trim() ?? string.Empty,
            model.CreatedByUserId
        }, cancellationToken);

    public Task UpdateAsync(ProductUnitUpdateModel model, CancellationToken cancellationToken = default) =>
        _accessService.ExecuteAsync("dbo.spProductUnitUpdate", new
        {
            model.UnitId,
            UnitCode = model.UnitCode.Trim(),
            UnitName = model.UnitName.Trim(),
            UnitSymbol = model.UnitSymbol?.Trim() ?? string.Empty,
            model.AllowDecimal,
            model.IsBaseUnit,
            Description = model.Description?.Trim() ?? string.Empty,
            model.IsActive,
            model.UpdatedByUserId
        }, cancellationToken);

    public Task DeactivateAsync(int unitId, int updatedByUserId, CancellationToken cancellationToken = default) =>
        _accessService.ExecuteAsync("dbo.spProductUnitDeactivate", new { UnitId = unitId, UpdatedByUserId = updatedByUserId }, cancellationToken);

    public Task<bool> IsCodeExistsAsync(string unitCode, int? excludeUnitId = null, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleAsync<bool, object>("dbo.spProductUnitCheckCodeExists", new { UnitCode = unitCode.Trim(), ExcludeUnitId = excludeUnitId }, cancellationToken);

    public Task<bool> IsNameExistsAsync(string unitName, int? excludeUnitId = null, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleAsync<bool, object>("dbo.spProductUnitCheckNameExists", new { UnitName = unitName.Trim(), ExcludeUnitId = excludeUnitId }, cancellationToken);
}

public sealed class ProductUnitConversionService : IProductUnitConversionService
{
    private readonly IAccessService _accessService;

    public ProductUnitConversionService(IAccessService accessService) => _accessService = accessService;

    public async Task<IReadOnlyCollection<ProductUnitConversionModel>> GetByUnitIdAsync(int unitId, CancellationToken cancellationToken = default)
    {
        var rows = await _accessService.QueryAsync<ProductUnitConversionModel, object>("dbo.spProductUnitConversionGetByUnitId", new { UnitId = unitId }, cancellationToken);
        return rows.ToArray();
    }

    public Task<ProductUnitConversionModel?> GetByIdAsync(int unitConversionId, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleOrDefaultAsync<ProductUnitConversionModel, object>("dbo.spProductUnitConversionGetById", new { UnitConversionId = unitConversionId }, cancellationToken);

    public Task<int> CreateAsync(ProductUnitConversionCreateModel model, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleAsync<int, object>("dbo.spProductUnitConversionCreate", new
        {
            model.FromUnitId,
            model.ToUnitId,
            model.ConversionRate,
            Description = model.Description?.Trim() ?? string.Empty,
            model.CreatedByUserId
        }, cancellationToken);

    public Task UpdateAsync(ProductUnitConversionUpdateModel model, CancellationToken cancellationToken = default) =>
        _accessService.ExecuteAsync("dbo.spProductUnitConversionUpdate", new
        {
            model.UnitConversionId,
            model.FromUnitId,
            model.ToUnitId,
            model.ConversionRate,
            Description = model.Description?.Trim() ?? string.Empty,
            model.IsActive,
            model.UpdatedByUserId
        }, cancellationToken);

    public Task DeactivateAsync(int unitConversionId, int updatedByUserId, CancellationToken cancellationToken = default) =>
        _accessService.ExecuteAsync("dbo.spProductUnitConversionDeactivate", new { UnitConversionId = unitConversionId, UpdatedByUserId = updatedByUserId }, cancellationToken);

    public Task<bool> IsDuplicateAsync(int fromUnitId, int toUnitId, int? excludeUnitConversionId = null, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleAsync<bool, object>("dbo.spProductUnitConversionCheckDuplicate", new { FromUnitId = fromUnitId, ToUnitId = toUnitId, ExcludeUnitConversionId = excludeUnitConversionId }, cancellationToken);
}

public sealed class ProductService : IProductService
{
    private readonly IAccessService _accessService;

    public ProductService(IAccessService accessService) => _accessService = accessService;

    public async Task<ProductPagedResultModel> GetPagedAsync(ProductPagedRequestModel request, CancellationToken cancellationToken = default)
    {
        var rows = (await _accessService.QueryAsync<ProductPagedRow, object>(
            "dbo.spProductGetPaged",
            new
            {
                request.PageNumber,
                request.PageSize,
                SearchText = ProductServiceHelpers.TrimOrNull(request.SearchText),
                request.IsActive,
                request.CategoryId,
                request.BrandId,
                request.UnitId,
                Status = ProductServiceHelpers.TrimOrNull(request.Status),
                request.LowStockOnly
            },
            cancellationToken)).ToArray();

        return new ProductPagedResultModel { Products = rows, TotalCount = rows.FirstOrDefault()?.TotalCount ?? 0, PageNumber = request.PageNumber, PageSize = request.PageSize };
    }

    public async Task<IReadOnlyCollection<ProductModel>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _accessService.QueryAsync<ProductModel, object>("dbo.spProductGetAllActive", new { }, cancellationToken);
        return rows.ToArray();
    }

    public Task<ProductModel?> GetByIdAsync(int productId, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleOrDefaultAsync<ProductModel, object>("dbo.spProductGetById", new { ProductId = productId }, cancellationToken);

    public Task<int> CreateAsync(ProductCreateModel model, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleAsync<int, object>("dbo.spProductCreate", new
        {
            ProductCode = model.ProductCode.Trim(),
            SKU = model.SKU?.Trim(),
            Barcode = ProductServiceHelpers.TrimOrNull(model.Barcode),
            ProductName = model.ProductName.Trim(),
            model.CategoryId,
            model.BrandId,
            model.UnitId,
            model.CostPrice,
            model.SellingPrice,
            model.WholesalePrice,
            model.WholesaleMinQty,
            model.TaxRate,
            model.DiscountAllowed,
            model.IsStockTracked,
            model.MinimumStockLevel,
            model.CurrentStock,
            ProductImageUrl = model.ProductImageUrl?.Trim() ?? string.Empty,
            Description = model.Description?.Trim() ?? string.Empty,
            Status = model.Status.Trim(),
            model.CreatedByUserId
        }, cancellationToken);

    public Task UpdateAsync(ProductUpdateModel model, CancellationToken cancellationToken = default) =>
        _accessService.ExecuteAsync("dbo.spProductUpdate", new
        {
            model.ProductId,
            ProductCode = model.ProductCode.Trim(),
            SKU = model.SKU?.Trim(),
            Barcode = ProductServiceHelpers.TrimOrNull(model.Barcode),
            ProductName = model.ProductName.Trim(),
            model.CategoryId,
            model.BrandId,
            model.UnitId,
            model.CostPrice,
            model.SellingPrice,
            model.WholesalePrice,
            model.WholesaleMinQty,
            model.TaxRate,
            model.DiscountAllowed,
            model.IsStockTracked,
            model.MinimumStockLevel,
            model.CurrentStock,
            ProductImageUrl = model.ProductImageUrl?.Trim() ?? string.Empty,
            Description = model.Description?.Trim() ?? string.Empty,
            Status = model.Status.Trim(),
            model.IsActive,
            model.UpdatedByUserId
        }, cancellationToken);

    public Task DeactivateAsync(int productId, int updatedByUserId, CancellationToken cancellationToken = default) =>
        _accessService.ExecuteAsync("dbo.spProductDeactivate", new { ProductId = productId, UpdatedByUserId = updatedByUserId }, cancellationToken);

    public Task UpdatePriceAsync(ProductPriceUpdateModel model, CancellationToken cancellationToken = default) =>
        _accessService.ExecuteAsync("dbo.spProductUpdatePrice", new
        {
            model.ProductId,
            model.NewCostPrice,
            model.NewSellingPrice,
            model.NewWholesalePrice,
            model.NewWholesaleMinQty,
            ChangeReason = model.ChangeReason?.Trim() ?? string.Empty,
            model.ChangedByUserId
        }, cancellationToken);

    public Task UpdateImageAsync(ProductImageUpdateModel model, CancellationToken cancellationToken = default) =>
        _accessService.ExecuteAsync("dbo.spProductUpdateImage", new { model.ProductId, ProductImageUrl = model.ProductImageUrl.Trim(), model.UpdatedByUserId }, cancellationToken);

    public Task UpdateBarcodeAsync(ProductBarcodeUpdateModel model, CancellationToken cancellationToken = default) =>
        _accessService.ExecuteAsync("dbo.spProductUpdateBarcode", new { model.ProductId, Barcode = ProductServiceHelpers.TrimOrNull(model.Barcode), model.UpdatedByUserId }, cancellationToken);

    public Task<bool> IsCodeExistsAsync(string productCode, int? excludeProductId = null, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleAsync<bool, object>("dbo.spProductCheckCodeExists", new { ProductCode = productCode.Trim(), ExcludeProductId = excludeProductId }, cancellationToken);

    public Task<bool> IsBarcodeExistsAsync(string barcode, int? excludeProductId = null, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleAsync<bool, object>("dbo.spProductCheckBarcodeExists", new { Barcode = barcode.Trim(), ExcludeProductId = excludeProductId }, cancellationToken);

    public async Task<IReadOnlyCollection<ProductModel>> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _accessService.QueryAsync<ProductModel, object>("dbo.spProductGetLowStock", new { }, cancellationToken);
        return rows.ToArray();
    }

    public Task<ProductModel?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleOrDefaultAsync<ProductModel, object>("dbo.spProductGetByBarcode", new { Barcode = barcode.Trim() }, cancellationToken);
}

public sealed class ProductPriceHistoryService : IProductPriceHistoryService
{
    private readonly IAccessService _accessService;

    public ProductPriceHistoryService(IAccessService accessService) => _accessService = accessService;

    public async Task<IReadOnlyCollection<ProductPriceHistoryModel>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        var rows = await _accessService.QueryAsync<ProductPriceHistoryModel, object>("dbo.spProductPriceHistoryGetByProductId", new { ProductId = productId }, cancellationToken);
        return rows.ToArray();
    }

    public async Task<ProductPriceHistoryPagedResultModel> GetPagedAsync(ProductPriceHistoryPagedRequestModel request, CancellationToken cancellationToken = default)
    {
        var rows = (await _accessService.QueryAsync<ProductPriceHistoryPagedRow, object>(
            "dbo.spProductPriceHistoryGetPaged",
            new
            {
                request.PageNumber,
                request.PageSize,
                SearchText = ProductServiceHelpers.TrimOrNull(request.SearchText),
                request.ProductId,
                request.FromDate,
                request.ToDate
            },
            cancellationToken)).ToArray();

        return new ProductPriceHistoryPagedResultModel { PriceHistory = rows, TotalCount = rows.FirstOrDefault()?.TotalCount ?? 0, PageNumber = request.PageNumber, PageSize = request.PageSize };
    }

    public Task<int> CreateAsync(ProductPriceHistoryModel model, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleAsync<int, object>("dbo.spProductPriceHistoryCreate", new
        {
            model.ProductId,
            model.OldCostPrice,
            model.NewCostPrice,
            model.OldSellingPrice,
            model.NewSellingPrice,
            model.ProfitAmount,
            model.ProfitMargin,
            ChangeReason = model.ChangeReason.Trim(),
            ChangedByUserId = model.ChangedByUserId ?? 0
        }, cancellationToken);
}

internal sealed class ProductCategoryPagedRow : ProductCategoryModel { public int TotalCount { get; init; } }
internal sealed class ProductBrandPagedRow : ProductBrandModel { public int TotalCount { get; init; } }
internal sealed class ProductUnitPagedRow : ProductUnitModel { public int TotalCount { get; init; } }
internal sealed class ProductPagedRow : ProductModel { public int TotalCount { get; init; } }
internal sealed class ProductPriceHistoryPagedRow : ProductPriceHistoryModel { public int TotalCount { get; init; } }

internal static class ProductServiceHelpers
{
    public static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
