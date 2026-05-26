using ProductEngine.Models;

namespace ProductEngine.Services;

public interface IProductCategoryService
{
    Task<ProductCategoryPagedResultModel> GetPagedAsync(ProductCategoryPagedRequestModel request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductCategoryModel>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<ProductCategoryModel?> GetByIdAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(ProductCategoryCreateModel model, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProductCategoryUpdateModel model, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int categoryId, int updatedByUserId, CancellationToken cancellationToken = default);
    Task<bool> IsCodeExistsAsync(string categoryCode, int? excludeCategoryId = null, CancellationToken cancellationToken = default);
    Task<bool> IsNameExistsAsync(string categoryName, int? excludeCategoryId = null, CancellationToken cancellationToken = default);
}

public interface IProductBrandService
{
    Task<ProductBrandPagedResultModel> GetPagedAsync(ProductBrandPagedRequestModel request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductBrandModel>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<ProductBrandModel?> GetByIdAsync(int brandId, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(ProductBrandCreateModel model, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProductBrandUpdateModel model, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int brandId, int updatedByUserId, CancellationToken cancellationToken = default);
    Task<bool> IsCodeExistsAsync(string brandCode, int? excludeBrandId = null, CancellationToken cancellationToken = default);
    Task<bool> IsNameExistsAsync(string brandName, int? excludeBrandId = null, CancellationToken cancellationToken = default);
}

public interface IProductUnitService
{
    Task<ProductUnitPagedResultModel> GetPagedAsync(ProductUnitPagedRequestModel request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductUnitModel>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<ProductUnitModel?> GetByIdAsync(int unitId, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(ProductUnitCreateModel model, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProductUnitUpdateModel model, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int unitId, int updatedByUserId, CancellationToken cancellationToken = default);
    Task<bool> IsCodeExistsAsync(string unitCode, int? excludeUnitId = null, CancellationToken cancellationToken = default);
    Task<bool> IsNameExistsAsync(string unitName, int? excludeUnitId = null, CancellationToken cancellationToken = default);
}

public interface IProductUnitConversionService
{
    Task<IReadOnlyCollection<ProductUnitConversionModel>> GetByUnitIdAsync(int unitId, CancellationToken cancellationToken = default);
    Task<ProductUnitConversionModel?> GetByIdAsync(int unitConversionId, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(ProductUnitConversionCreateModel model, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProductUnitConversionUpdateModel model, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int unitConversionId, int updatedByUserId, CancellationToken cancellationToken = default);
    Task<bool> IsDuplicateAsync(int fromUnitId, int toUnitId, int? excludeUnitConversionId = null, CancellationToken cancellationToken = default);
}

public interface IProductService
{
    Task<ProductPagedResultModel> GetPagedAsync(ProductPagedRequestModel request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductModel>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<ProductModel?> GetByIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(ProductCreateModel model, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProductUpdateModel model, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int productId, int updatedByUserId, CancellationToken cancellationToken = default);
    Task UpdatePriceAsync(ProductPriceUpdateModel model, CancellationToken cancellationToken = default);
    Task UpdateImageAsync(ProductImageUpdateModel model, CancellationToken cancellationToken = default);
    Task UpdateBarcodeAsync(ProductBarcodeUpdateModel model, CancellationToken cancellationToken = default);
    Task<bool> IsCodeExistsAsync(string productCode, int? excludeProductId = null, CancellationToken cancellationToken = default);
    Task<bool> IsBarcodeExistsAsync(string barcode, int? excludeProductId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductModel>> GetLowStockAsync(CancellationToken cancellationToken = default);
    Task<ProductModel?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
}

public interface IProductPriceHistoryService
{
    Task<IReadOnlyCollection<ProductPriceHistoryModel>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<ProductPriceHistoryPagedResultModel> GetPagedAsync(ProductPriceHistoryPagedRequestModel request, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(ProductPriceHistoryModel model, CancellationToken cancellationToken = default);
}
