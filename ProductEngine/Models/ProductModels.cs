namespace ProductEngine.Models;

public class ProductCategoryModel
{
    public int CategoryId { get; init; }
    public string CategoryCode { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public int? CreatedByUserId { get; init; }
    public int? UpdatedByUserId { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? UpdatedDate { get; init; }
}

public sealed class ProductCategoryCreateModel
{
    public string CategoryCode { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DisplayOrder { get; init; }
    public int CreatedByUserId { get; init; }
}

public sealed class ProductCategoryUpdateModel
{
    public int CategoryId { get; init; }
    public string CategoryCode { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public int UpdatedByUserId { get; init; }
}

public sealed class ProductCategoryPagedRequestModel
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchText { get; init; }
    public bool? IsActive { get; init; }
}

public sealed class ProductCategoryPagedResultModel
{
    public IReadOnlyCollection<ProductCategoryModel> Categories { get; init; } = Array.Empty<ProductCategoryModel>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public class ProductBrandModel
{
    public int BrandId { get; init; }
    public string BrandCode { get; init; } = string.Empty;
    public string BrandName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string LogoUrl { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int? CreatedByUserId { get; init; }
    public int? UpdatedByUserId { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? UpdatedDate { get; init; }
}

public sealed class ProductBrandCreateModel
{
    public string BrandCode { get; init; } = string.Empty;
    public string BrandName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? LogoUrl { get; init; }
    public int CreatedByUserId { get; init; }
}

public sealed class ProductBrandUpdateModel
{
    public int BrandId { get; init; }
    public string BrandCode { get; init; } = string.Empty;
    public string BrandName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? LogoUrl { get; init; }
    public bool IsActive { get; init; }
    public int UpdatedByUserId { get; init; }
}

public sealed class ProductBrandPagedRequestModel
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchText { get; init; }
    public bool? IsActive { get; init; }
}

public sealed class ProductBrandPagedResultModel
{
    public IReadOnlyCollection<ProductBrandModel> Brands { get; init; } = Array.Empty<ProductBrandModel>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public class ProductUnitModel
{
    public int UnitId { get; init; }
    public string UnitCode { get; init; } = string.Empty;
    public string UnitName { get; init; } = string.Empty;
    public string UnitSymbol { get; init; } = string.Empty;
    public bool AllowDecimal { get; init; }
    public bool IsBaseUnit { get; init; }
    public string Description { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int? CreatedByUserId { get; init; }
    public int? UpdatedByUserId { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? UpdatedDate { get; init; }
}

public sealed class ProductUnitCreateModel
{
    public string UnitCode { get; init; } = string.Empty;
    public string UnitName { get; init; } = string.Empty;
    public string? UnitSymbol { get; init; }
    public bool AllowDecimal { get; init; }
    public bool IsBaseUnit { get; init; }
    public string? Description { get; init; }
    public int CreatedByUserId { get; init; }
}

public sealed class ProductUnitUpdateModel
{
    public int UnitId { get; init; }
    public string UnitCode { get; init; } = string.Empty;
    public string UnitName { get; init; } = string.Empty;
    public string? UnitSymbol { get; init; }
    public bool AllowDecimal { get; init; }
    public bool IsBaseUnit { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public int UpdatedByUserId { get; init; }
}

public sealed class ProductUnitPagedRequestModel
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchText { get; init; }
    public bool? IsActive { get; init; }
}

public sealed class ProductUnitPagedResultModel
{
    public IReadOnlyCollection<ProductUnitModel> Units { get; init; } = Array.Empty<ProductUnitModel>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public sealed class ProductUnitConversionModel
{
    public int UnitConversionId { get; init; }
    public int FromUnitId { get; init; }
    public string FromUnitCode { get; init; } = string.Empty;
    public string FromUnitName { get; init; } = string.Empty;
    public int ToUnitId { get; init; }
    public string ToUnitCode { get; init; } = string.Empty;
    public string ToUnitName { get; init; } = string.Empty;
    public decimal ConversionRate { get; init; }
    public string Description { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int? CreatedByUserId { get; init; }
    public int? UpdatedByUserId { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? UpdatedDate { get; init; }
}

public sealed class ProductUnitConversionCreateModel
{
    public int FromUnitId { get; init; }
    public int ToUnitId { get; init; }
    public decimal ConversionRate { get; init; }
    public string? Description { get; init; }
    public int CreatedByUserId { get; init; }
}

public sealed class ProductUnitConversionUpdateModel
{
    public int UnitConversionId { get; init; }
    public int FromUnitId { get; init; }
    public int ToUnitId { get; init; }
    public decimal ConversionRate { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public int UpdatedByUserId { get; init; }
}

public class ProductModel
{
    public int ProductId { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public string Barcode { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public int? BrandId { get; init; }
    public string BrandName { get; init; } = string.Empty;
    public int UnitId { get; init; }
    public string UnitName { get; init; } = string.Empty;
    public decimal CostPrice { get; init; }
    public decimal SellingPrice { get; init; }
    public decimal WholesalePrice { get; init; }
    public decimal WholesaleMinQty { get; init; } = 1;
    public decimal TaxRate { get; init; }
    public bool DiscountAllowed { get; init; }
    public bool IsStockTracked { get; init; }
    public decimal MinimumStockLevel { get; init; }
    public decimal CurrentStock { get; init; }
    public string ProductImageUrl { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Status { get; init; } = "Active";
    public bool IsActive { get; init; }
    public int? CreatedByUserId { get; init; }
    public int? UpdatedByUserId { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? UpdatedDate { get; init; }
}

public sealed class ProductCreateModel
{
    public string ProductCode { get; init; } = string.Empty;
    public string? SKU { get; init; }
    public string? Barcode { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int CategoryId { get; init; }
    public int? BrandId { get; init; }
    public int UnitId { get; init; }
    public decimal CostPrice { get; init; }
    public decimal SellingPrice { get; init; }
    public decimal WholesalePrice { get; init; }
    public decimal WholesaleMinQty { get; init; } = 1;
    public decimal TaxRate { get; init; }
    public bool DiscountAllowed { get; init; }
    public bool IsStockTracked { get; init; }
    public decimal MinimumStockLevel { get; init; }
    public decimal CurrentStock { get; init; }
    public string? ProductImageUrl { get; init; }
    public string? Description { get; init; }
    public string Status { get; init; } = "Active";
    public int CreatedByUserId { get; init; }
}

public sealed class ProductUpdateModel
{
    public int ProductId { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string? SKU { get; init; }
    public string? Barcode { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int CategoryId { get; init; }
    public int? BrandId { get; init; }
    public int UnitId { get; init; }
    public decimal CostPrice { get; init; }
    public decimal SellingPrice { get; init; }
    public decimal WholesalePrice { get; init; }
    public decimal WholesaleMinQty { get; init; } = 1;
    public decimal TaxRate { get; init; }
    public bool DiscountAllowed { get; init; }
    public bool IsStockTracked { get; init; }
    public decimal MinimumStockLevel { get; init; }
    public decimal CurrentStock { get; init; }
    public string? ProductImageUrl { get; init; }
    public string? Description { get; init; }
    public string Status { get; init; } = "Active";
    public bool IsActive { get; init; }
    public int UpdatedByUserId { get; init; }
}

public sealed class ProductPriceUpdateModel
{
    public int ProductId { get; init; }
    public decimal NewCostPrice { get; init; }
    public decimal NewSellingPrice { get; init; }
    public decimal NewWholesalePrice { get; init; }
    public decimal NewWholesaleMinQty { get; init; } = 1;
    public string? ChangeReason { get; init; }
    public int ChangedByUserId { get; init; }
}

public sealed class ProductImageUpdateModel
{
    public int ProductId { get; init; }
    public string ProductImageUrl { get; init; } = string.Empty;
    public int UpdatedByUserId { get; init; }
}

public sealed class ProductBarcodeUpdateModel
{
    public int ProductId { get; init; }
    public string? Barcode { get; init; }
    public int UpdatedByUserId { get; init; }
}

public sealed class ProductPagedRequestModel
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchText { get; init; }
    public bool? IsActive { get; init; }
    public int? CategoryId { get; init; }
    public int? BrandId { get; init; }
    public int? UnitId { get; init; }
    public string? Status { get; init; }
    public bool LowStockOnly { get; init; }
}

public sealed class ProductPagedResultModel
{
    public IReadOnlyCollection<ProductModel> Products { get; init; } = Array.Empty<ProductModel>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public class ProductPriceHistoryModel
{
    public int ProductPriceHistoryId { get; init; }
    public int ProductId { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public decimal OldCostPrice { get; init; }
    public decimal NewCostPrice { get; init; }
    public decimal OldSellingPrice { get; init; }
    public decimal NewSellingPrice { get; init; }
    public decimal ProfitAmount { get; init; }
    public decimal ProfitMargin { get; init; }
    public string ChangeReason { get; init; } = string.Empty;
    public int? ChangedByUserId { get; init; }
    public DateTime ChangedDate { get; init; }
}

public sealed class ProductPriceHistoryPagedRequestModel
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchText { get; init; }
    public int? ProductId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public sealed class ProductPriceHistoryPagedResultModel
{
    public IReadOnlyCollection<ProductPriceHistoryModel> PriceHistory { get; init; } = Array.Empty<ProductPriceHistoryModel>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}
