namespace InventoryEngine.Models;

public sealed class PagedResultModel<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public sealed class TotalCountModel
{
    public int TotalCount { get; init; }
}

public sealed class InventoryDashboardFilterModel
{
    public int? LocationId { get; init; }
    public int? CategoryId { get; init; }
    public int? BrandId { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public string GroupBy { get; init; } = "Daily";
}

public sealed class InventoryDashboardSummaryModel
{
    public int TotalProducts { get; init; }
    public int TrackedProducts { get; init; }
    public int ActiveLocations { get; init; }
    public decimal TotalStockQty { get; init; }
    public decimal TotalStockValue { get; init; }
    public int LowStockProducts { get; init; }
    public int OutOfStockProducts { get; init; }
    public int MovementCount { get; init; }
    public decimal StockInQty { get; init; }
    public decimal StockOutQty { get; init; }
    public decimal NetMovementQty { get; init; }
    public int DraftAdjustments { get; init; }
    public int OpenStockCounts { get; init; }
    public int OpenTransfers { get; init; }
}

public sealed class InventoryStockStatusSummaryModel
{
    public string StockStatus { get; init; } = string.Empty;
    public int ProductCount { get; init; }
    public decimal TotalQty { get; init; }
    public decimal TotalValue { get; init; }
}

public sealed class InventoryValueByCategoryModel
{
    public int CategoryId { get; init; }
    public string CategoryCode { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public int ProductCount { get; init; }
    public decimal TotalQty { get; init; }
    public decimal TotalValue { get; init; }
}

public sealed class InventoryValueByLocationModel
{
    public int LocationId { get; init; }
    public string LocationCode { get; init; } = string.Empty;
    public string LocationName { get; init; } = string.Empty;
    public int ProductCount { get; init; }
    public decimal TotalQty { get; init; }
    public decimal TotalValue { get; init; }
}

public sealed class InventoryRecentMovementModel
{
    public long InventoryMovementId { get; init; }
    public DateTime MovementDate { get; init; }
    public int ProductId { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public string BrandName { get; init; } = string.Empty;
    public int LocationId { get; init; }
    public string LocationName { get; init; } = string.Empty;
    public string MovementType { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal QuantitySigned { get; init; }
    public decimal UnitCost { get; init; }
    public decimal MovementValue { get; init; }
    public string ReferenceType { get; init; } = string.Empty;
    public string ReferenceNo { get; init; } = string.Empty;
    public string CreatedByName { get; init; } = string.Empty;
}

public sealed class InventoryMovementTrendModel
{
    public DateTime PeriodStart { get; init; }
    public string PeriodLabel { get; init; } = string.Empty;
    public decimal StockInQty { get; init; }
    public decimal StockOutQty { get; init; }
    public decimal NetQty { get; init; }
    public int MovementCount { get; init; }
}

public sealed class InventoryLowStockProductModel
{
    public int ProductId { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public string Barcode { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public string BrandName { get; init; } = string.Empty;
    public string UnitName { get; init; } = string.Empty;
    public string UnitSymbol { get; init; } = string.Empty;
    public int LocationId { get; init; }
    public string LocationName { get; init; } = string.Empty;
    public decimal CurrentQty { get; init; }
    public decimal MinimumStockLevel { get; init; }
    public decimal ShortageQty { get; init; }
    public decimal EstimatedReorderValue { get; init; }
    public string StockStatus { get; init; } = string.Empty;
    public DateTime? LastMovementDate { get; init; }
}

public sealed class InventoryTopMovingProductModel
{
    public int ProductId { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public string BrandName { get; init; } = string.Empty;
    public decimal StockInQty { get; init; }
    public decimal StockOutQty { get; init; }
    public decimal TotalMovedQty { get; init; }
    public decimal NetQty { get; init; }
    public int MovementCount { get; init; }
}

public sealed class InventoryLocationModel
{
    public int LocationId { get; init; }
    public string LocationCode { get; init; } = string.Empty;
    public string LocationName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; }
    public int? CreatedByUserId { get; init; }
    public DateTime CreatedDate { get; init; }
    public int? UpdatedByUserId { get; init; }
    public DateTime? UpdatedDate { get; init; }
}

public sealed class InventoryLocationCreateModel
{
    public string LocationCode { get; init; } = string.Empty;
    public string LocationName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsDefault { get; init; }
    public int CreatedByUserId { get; init; }
}

public sealed class InventoryLocationUpdateModel
{
    public int LocationId { get; init; }
    public string LocationCode { get; init; } = string.Empty;
    public string LocationName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; }
    public int UpdatedByUserId { get; init; }
}

public class InventoryStockModel
{
    public long InventoryStockId { get; init; }
    public int ProductId { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public int LocationId { get; init; }
    public string LocationCode { get; init; } = string.Empty;
    public string LocationName { get; init; } = string.Empty;
    public decimal CurrentStock { get; init; }
    public decimal MinimumStockLevel { get; init; }
    public bool IsLowStock { get; init; }
    public bool IsOutOfStock { get; init; }
    public DateTime LastMovementDate { get; init; }
    public DateTime UpdatedDate { get; init; }
}

public sealed class InventoryStockPagedRequestModel
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchText { get; init; }
    public int? LocationId { get; init; }
    public int? CategoryId { get; init; }
    public bool ActiveProductsOnly { get; init; } = true;
}

public sealed class InventoryStockPagedResultModel
{
    public IReadOnlyCollection<InventoryStockModel> Stocks { get; init; } = Array.Empty<InventoryStockModel>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public sealed class InventoryStockSummaryModel
{
    public int ProductCount { get; init; }
    public int LowStockCount { get; init; }
    public int OutOfStockCount { get; init; }
    public decimal TotalStockQty { get; init; }
}

public class InventoryMovementModel
{
    public long InventoryMovementId { get; init; }
    public int ProductId { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public int LocationId { get; init; }
    public string LocationCode { get; init; } = string.Empty;
    public string LocationName { get; init; } = string.Empty;
    public string MovementType { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal QuantitySigned { get; init; }
    public decimal UnitCost { get; init; }
    public decimal StockBefore { get; init; }
    public decimal StockAfter { get; init; }
    public string ReferenceType { get; init; } = string.Empty;
    public long? ReferenceId { get; init; }
    public string ReferenceNo { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public string Remarks { get; init; } = string.Empty;
    public int? CreatedByUserId { get; init; }
    public DateTime CreatedDate { get; init; }
}

public sealed class InventoryMovementCreateModel
{
    public int ProductId { get; init; }
    public int LocationId { get; init; }
    public string MovementType { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal UnitCost { get; init; }
    public string? ReferenceType { get; init; }
    public long? ReferenceId { get; init; }
    public string? ReferenceNo { get; init; }
    public string? Reason { get; init; }
    public string? Remarks { get; init; }
    public bool AllowNegativeStock { get; init; }
    public int CreatedByUserId { get; init; }
}

public sealed class InventoryMovementPagedRequestModel
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchText { get; init; }
    public int? ProductId { get; init; }
    public int? LocationId { get; init; }
    public string? MovementType { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public sealed class InventoryMovementPagedResultModel
{
    public IReadOnlyCollection<InventoryMovementModel> Movements { get; init; } = Array.Empty<InventoryMovementModel>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public sealed class InventoryMovementSummaryModel
{
    public DateTime MovementDate { get; init; }
    public string MovementType { get; init; } = string.Empty;
    public decimal TotalQuantity { get; init; }
    public int MovementCount { get; init; }
}

public class StockAdjustmentModel
{
    public long StockAdjustmentId { get; init; }
    public string AdjustmentNo { get; init; } = string.Empty;
    public int LocationId { get; init; }
    public string LocationName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public string Remarks { get; init; } = string.Empty;
    public int? CreatedByUserId { get; init; }
    public DateTime CreatedDate { get; init; }
    public int? UpdatedByUserId { get; init; }
    public DateTime? UpdatedDate { get; init; }
    public IReadOnlyCollection<StockAdjustmentItemModel> Items { get; init; } = Array.Empty<StockAdjustmentItemModel>();
}

public sealed class StockAdjustmentItemModel
{
    public long StockAdjustmentItemId { get; init; }
    public long StockAdjustmentId { get; init; }
    public int ProductId { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public string AdjustmentType { get; init; } = string.Empty;
    public decimal UnitCost { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public sealed class StockAdjustmentCreateModel
{
    public int LocationId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? Remarks { get; init; }
    public int CreatedByUserId { get; init; }
}

public sealed class StockAdjustmentApproveModel
{
    public long StockAdjustmentId { get; init; }
    public int ApprovedByUserId { get; init; }
}

public class StockCountModel
{
    public long StockCountId { get; init; }
    public string StockCountNo { get; init; } = string.Empty;
    public int LocationId { get; init; }
    public string LocationName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Remarks { get; init; } = string.Empty;
    public int? CreatedByUserId { get; init; }
    public DateTime CreatedDate { get; init; }
    public int? UpdatedByUserId { get; init; }
    public DateTime? UpdatedDate { get; init; }
    public IReadOnlyCollection<StockCountItemModel> Items { get; init; } = Array.Empty<StockCountItemModel>();
}

public sealed class StockCountItemModel
{
    public long StockCountItemId { get; init; }
    public long StockCountId { get; init; }
    public int ProductId { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public decimal SystemQty { get; init; }
    public decimal CountedQty { get; init; }
    public decimal VarianceQty { get; init; }
    public string Remarks { get; init; } = string.Empty;
}

public sealed class StockCountCreateModel
{
    public int LocationId { get; init; }
    public string? Remarks { get; init; }
    public int CreatedByUserId { get; init; }
}

public sealed class StockCountApproveModel
{
    public long StockCountId { get; init; }
    public int ApprovedByUserId { get; init; }
}

public class StockTransferModel
{
    public long StockTransferId { get; init; }
    public string TransferNo { get; init; } = string.Empty;
    public int SourceLocationId { get; init; }
    public string SourceLocationName { get; init; } = string.Empty;
    public int DestinationLocationId { get; init; }
    public string DestinationLocationName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Remarks { get; init; } = string.Empty;
    public DateTime? SentDate { get; init; }
    public DateTime? ReceivedDate { get; init; }
    public int? CreatedByUserId { get; init; }
    public DateTime CreatedDate { get; init; }
    public int? UpdatedByUserId { get; init; }
    public DateTime? UpdatedDate { get; init; }
    public IReadOnlyCollection<StockTransferItemModel> Items { get; init; } = Array.Empty<StockTransferItemModel>();
}

public sealed class StockTransferItemModel
{
    public long StockTransferItemId { get; init; }
    public long StockTransferId { get; init; }
    public int ProductId { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal UnitCost { get; init; }
}

public sealed class StockTransferCreateModel
{
    public int SourceLocationId { get; init; }
    public int DestinationLocationId { get; init; }
    public string? Remarks { get; init; }
    public int CreatedByUserId { get; init; }
}

public sealed class StockTransferSendModel
{
    public long StockTransferId { get; init; }
    public int SentByUserId { get; init; }
}

public sealed class StockTransferReceiveModel
{
    public long StockTransferId { get; init; }
    public int ReceivedByUserId { get; init; }
}
