using InventoryEngine.Models;

namespace InventoryEngine.Services;

public interface IInventoryDashboardService
{
    Task<InventoryDashboardSummaryModel> GetSummaryAsync(InventoryDashboardFilterModel filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryStockStatusSummaryModel>> GetStockStatusSummaryAsync(InventoryDashboardFilterModel filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryValueByCategoryModel>> GetValueByCategoryAsync(InventoryDashboardFilterModel filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryValueByLocationModel>> GetValueByLocationAsync(InventoryDashboardFilterModel filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryRecentMovementModel>> GetRecentMovementsAsync(InventoryDashboardFilterModel filter, int top = 10, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryMovementTrendModel>> GetMovementTrendAsync(InventoryDashboardFilterModel filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryLowStockProductModel>> GetLowStockProductsAsync(InventoryDashboardFilterModel filter, int top = 10, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryTopMovingProductModel>> GetTopMovingProductsAsync(InventoryDashboardFilterModel filter, int top = 10, CancellationToken cancellationToken = default);
}

public interface IInventoryLocationService
{
    Task<IReadOnlyCollection<InventoryLocationModel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryLocationModel>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<InventoryLocationModel?> GetByIdAsync(int locationId, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(InventoryLocationCreateModel model, CancellationToken cancellationToken = default);
    Task UpdateAsync(InventoryLocationUpdateModel model, CancellationToken cancellationToken = default);
    Task ToggleActiveAsync(int locationId, bool isActive, int updatedByUserId, CancellationToken cancellationToken = default);
}

public interface IInventoryStockService
{
    Task<InventoryStockPagedResultModel> GetPagedAsync(InventoryStockPagedRequestModel request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryStockModel>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<InventoryStockPagedResultModel> GetLowStockPagedAsync(InventoryStockPagedRequestModel request, CancellationToken cancellationToken = default);
    Task<InventoryStockPagedResultModel> GetOutOfStockPagedAsync(InventoryStockPagedRequestModel request, CancellationToken cancellationToken = default);
    Task<InventoryStockSummaryModel> GetSummaryAsync(int? locationId = null, CancellationToken cancellationToken = default);
}

public interface IInventoryMovementService
{
    Task<InventoryMovementPagedResultModel> GetPagedAsync(InventoryMovementPagedRequestModel request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryMovementModel>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<long> CreateAsync(InventoryMovementCreateModel model, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryMovementModel>> GetByReferenceNoAsync(string referenceNo, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryMovementSummaryModel>> GetSummaryByDateRangeAsync(DateTime fromDate, DateTime toDate, int? locationId = null, CancellationToken cancellationToken = default);
}

public interface IStockAdjustmentService
{
    Task<PagedResultModel<StockAdjustmentModel>> GetPagedAsync(int pageNumber, int pageSize, string? searchText = null, string? status = null, int? locationId = null, CancellationToken cancellationToken = default);
    Task<StockAdjustmentModel?> GetByIdAsync(long stockAdjustmentId, CancellationToken cancellationToken = default);
    Task<long> CreateAsync(StockAdjustmentCreateModel model, CancellationToken cancellationToken = default);
    Task<long> AddItemAsync(long stockAdjustmentId, int productId, decimal quantity, string adjustmentType, decimal unitCost, string reason, CancellationToken cancellationToken = default);
    Task ApproveAsync(StockAdjustmentApproveModel model, CancellationToken cancellationToken = default);
    Task RejectAsync(long stockAdjustmentId, string reason, int rejectedByUserId, CancellationToken cancellationToken = default);
    Task CancelAsync(long stockAdjustmentId, int cancelledByUserId, CancellationToken cancellationToken = default);
}

public interface IStockCountService
{
    Task<PagedResultModel<StockCountModel>> GetPagedAsync(int pageNumber, int pageSize, string? searchText = null, string? status = null, int? locationId = null, CancellationToken cancellationToken = default);
    Task<StockCountModel?> GetByIdAsync(long stockCountId, CancellationToken cancellationToken = default);
    Task<long> CreateAsync(StockCountCreateModel model, CancellationToken cancellationToken = default);
    Task<long> AddItemAsync(long stockCountId, int productId, decimal countedQty, string? remarks, CancellationToken cancellationToken = default);
    Task UpdateCountedQtyAsync(long stockCountItemId, decimal countedQty, string? remarks, int updatedByUserId, CancellationToken cancellationToken = default);
    Task ApproveAsync(StockCountApproveModel model, CancellationToken cancellationToken = default);
    Task CancelAsync(long stockCountId, int cancelledByUserId, CancellationToken cancellationToken = default);
}

public interface IStockTransferService
{
    Task<PagedResultModel<StockTransferModel>> GetPagedAsync(int pageNumber, int pageSize, string? searchText = null, string? status = null, int? sourceLocationId = null, int? destinationLocationId = null, CancellationToken cancellationToken = default);
    Task<StockTransferModel?> GetByIdAsync(long stockTransferId, CancellationToken cancellationToken = default);
    Task<long> CreateAsync(StockTransferCreateModel model, CancellationToken cancellationToken = default);
    Task<long> AddItemAsync(long stockTransferId, int productId, decimal quantity, decimal unitCost, CancellationToken cancellationToken = default);
    Task SendAsync(StockTransferSendModel model, CancellationToken cancellationToken = default);
    Task ReceiveAsync(StockTransferReceiveModel model, CancellationToken cancellationToken = default);
    Task CancelAsync(long stockTransferId, int cancelledByUserId, CancellationToken cancellationToken = default);
}
