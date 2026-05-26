using AccessEngine.Services;
using Dapper;
using InventoryEngine.Models;

namespace InventoryEngine.Services;

public sealed class InventoryDashboardService : IInventoryDashboardService
{
    private readonly IAccessService _accessService;

    public InventoryDashboardService(IAccessService accessService) => _accessService = accessService;

    public Task<InventoryDashboardSummaryModel> GetSummaryAsync(InventoryDashboardFilterModel filter, CancellationToken cancellationToken = default)
    {
        ValidateFilter(filter);
        return _accessService.QuerySingleAsync<InventoryDashboardSummaryModel, object>("dbo.spInventoryDashboardGetSummary", ToDashboardParameters(filter), cancellationToken);
    }

    public async Task<IReadOnlyCollection<InventoryStockStatusSummaryModel>> GetStockStatusSummaryAsync(InventoryDashboardFilterModel filter, CancellationToken cancellationToken = default)
    {
        ValidateFilter(filter);
        var rows = await _accessService.QueryAsync<InventoryStockStatusSummaryModel, object>("dbo.spInventoryDashboardGetStockStatusSummary", ToDashboardParameters(filter), cancellationToken);
        return rows.ToArray();
    }

    public async Task<IReadOnlyCollection<InventoryValueByCategoryModel>> GetValueByCategoryAsync(InventoryDashboardFilterModel filter, CancellationToken cancellationToken = default)
    {
        ValidateFilter(filter);
        var rows = await _accessService.QueryAsync<InventoryValueByCategoryModel, object>("dbo.spInventoryDashboardGetValueByCategory", ToDashboardParameters(filter), cancellationToken);
        return rows.ToArray();
    }

    public async Task<IReadOnlyCollection<InventoryValueByLocationModel>> GetValueByLocationAsync(InventoryDashboardFilterModel filter, CancellationToken cancellationToken = default)
    {
        ValidateFilter(filter);
        var rows = await _accessService.QueryAsync<InventoryValueByLocationModel, object>("dbo.spInventoryDashboardGetValueByLocation", ToDashboardParameters(filter), cancellationToken);
        return rows.ToArray();
    }

    public async Task<IReadOnlyCollection<InventoryRecentMovementModel>> GetRecentMovementsAsync(InventoryDashboardFilterModel filter, int top = 10, CancellationToken cancellationToken = default)
    {
        ValidateFilter(filter);
        ValidateTop(top);
        var rows = await _accessService.QueryAsync<InventoryRecentMovementModel, object>("dbo.spInventoryDashboardGetRecentMovements", ToDashboardParametersWithTop(filter, top), cancellationToken);
        return rows.ToArray();
    }

    public async Task<IReadOnlyCollection<InventoryMovementTrendModel>> GetMovementTrendAsync(InventoryDashboardFilterModel filter, CancellationToken cancellationToken = default)
    {
        ValidateFilter(filter);
        var rows = await _accessService.QueryAsync<InventoryMovementTrendModel, object>("dbo.spInventoryDashboardGetMovementTrend", ToDashboardParameters(filter), cancellationToken);
        return rows.ToArray();
    }

    public async Task<IReadOnlyCollection<InventoryLowStockProductModel>> GetLowStockProductsAsync(InventoryDashboardFilterModel filter, int top = 10, CancellationToken cancellationToken = default)
    {
        ValidateFilter(filter);
        ValidateTop(top);
        var rows = await _accessService.QueryAsync<InventoryLowStockProductModel, object>("dbo.spInventoryDashboardGetLowStockProducts", ToDashboardParametersWithTop(filter, top), cancellationToken);
        return rows.ToArray();
    }

    public async Task<IReadOnlyCollection<InventoryTopMovingProductModel>> GetTopMovingProductsAsync(InventoryDashboardFilterModel filter, int top = 10, CancellationToken cancellationToken = default)
    {
        ValidateFilter(filter);
        ValidateTop(top);
        var rows = await _accessService.QueryAsync<InventoryTopMovingProductModel, object>("dbo.spInventoryDashboardGetTopMovingProducts", ToDashboardParametersWithTop(filter, top), cancellationToken);
        return rows.ToArray();
    }

    private static object ToDashboardParameters(InventoryDashboardFilterModel filter) => new
    {
        filter.LocationId,
        filter.CategoryId,
        filter.BrandId,
        filter.DateFrom,
        filter.DateTo,
        GroupBy = string.IsNullOrWhiteSpace(filter.GroupBy) ? "Daily" : filter.GroupBy.Trim()
    };

    private static object ToDashboardParametersWithTop(InventoryDashboardFilterModel filter, int top) => new
    {
        filter.LocationId,
        filter.CategoryId,
        filter.BrandId,
        filter.DateFrom,
        filter.DateTo,
        GroupBy = string.IsNullOrWhiteSpace(filter.GroupBy) ? "Daily" : filter.GroupBy.Trim(),
        Top = top
    };

    private static void ValidateFilter(InventoryDashboardFilterModel filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        if (filter.LocationId.HasValue) InventoryGuard.Positive(filter.LocationId.Value, nameof(filter.LocationId));
        if (filter.CategoryId.HasValue) InventoryGuard.Positive(filter.CategoryId.Value, nameof(filter.CategoryId));
        if (filter.BrandId.HasValue) InventoryGuard.Positive(filter.BrandId.Value, nameof(filter.BrandId));
        if (filter.DateFrom.HasValue && filter.DateTo.HasValue && filter.DateTo.Value.Date < filter.DateFrom.Value.Date)
        {
            throw new ArgumentException("DateTo must be greater than or equal to DateFrom.", nameof(filter.DateTo));
        }

        var groupBy = string.IsNullOrWhiteSpace(filter.GroupBy) ? "Daily" : filter.GroupBy.Trim();
        if (!StringComparer.OrdinalIgnoreCase.Equals(groupBy, "Daily") &&
            !StringComparer.OrdinalIgnoreCase.Equals(groupBy, "Weekly") &&
            !StringComparer.OrdinalIgnoreCase.Equals(groupBy, "Monthly"))
        {
            throw new ArgumentException("GroupBy must be Daily, Weekly, or Monthly.", nameof(filter.GroupBy));
        }
    }

    private static void ValidateTop(int top)
    {
        if (top <= 0 || top > 100) throw new ArgumentException("Top must be between 1 and 100.", nameof(top));
    }
}

public sealed class InventoryLocationService : IInventoryLocationService
{
    private readonly IAccessService _accessService;

    public InventoryLocationService(IAccessService accessService) => _accessService = accessService;

    public async Task<IReadOnlyCollection<InventoryLocationModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _accessService.QueryAsync<InventoryLocationModel, object>("dbo.spInventoryLocationGetAll", new { }, cancellationToken);
        return rows.ToArray();
    }

    public async Task<IReadOnlyCollection<InventoryLocationModel>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _accessService.QueryAsync<InventoryLocationModel, object>("dbo.spInventoryLocationGetAllActive", new { }, cancellationToken);
        return rows.ToArray();
    }

    public Task<InventoryLocationModel?> GetByIdAsync(int locationId, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(locationId, nameof(locationId));
        return _accessService.QuerySingleOrDefaultAsync<InventoryLocationModel, object>("dbo.spInventoryLocationGetById", new { LocationId = locationId }, cancellationToken);
    }

    public Task<int> CreateAsync(InventoryLocationCreateModel model, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Required(model.LocationCode, nameof(model.LocationCode));
        InventoryGuard.Required(model.LocationName, nameof(model.LocationName));
        InventoryGuard.Positive(model.CreatedByUserId, nameof(model.CreatedByUserId));

        return _accessService.QuerySingleAsync<int, object>("dbo.spInventoryLocationCreate", new
        {
            LocationCode = model.LocationCode.Trim(),
            LocationName = model.LocationName.Trim(),
            Description = InventoryGuard.TrimOrEmpty(model.Description),
            model.IsDefault,
            model.CreatedByUserId
        }, cancellationToken);
    }

    public Task UpdateAsync(InventoryLocationUpdateModel model, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(model.LocationId, nameof(model.LocationId));
        InventoryGuard.Required(model.LocationCode, nameof(model.LocationCode));
        InventoryGuard.Required(model.LocationName, nameof(model.LocationName));
        InventoryGuard.Positive(model.UpdatedByUserId, nameof(model.UpdatedByUserId));

        return _accessService.ExecuteAsync("dbo.spInventoryLocationUpdate", new
        {
            model.LocationId,
            LocationCode = model.LocationCode.Trim(),
            LocationName = model.LocationName.Trim(),
            Description = InventoryGuard.TrimOrEmpty(model.Description),
            model.IsDefault,
            model.IsActive,
            model.UpdatedByUserId
        }, cancellationToken);
    }

    public Task ToggleActiveAsync(int locationId, bool isActive, int updatedByUserId, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(locationId, nameof(locationId));
        InventoryGuard.Positive(updatedByUserId, nameof(updatedByUserId));
        return _accessService.ExecuteAsync("dbo.spInventoryLocationToggleActive", new { LocationId = locationId, IsActive = isActive, UpdatedByUserId = updatedByUserId }, cancellationToken);
    }
}

public sealed class InventoryStockService : IInventoryStockService
{
    private readonly IAccessService _accessService;

    public InventoryStockService(IAccessService accessService) => _accessService = accessService;

    public async Task<InventoryStockPagedResultModel> GetPagedAsync(InventoryStockPagedRequestModel request, CancellationToken cancellationToken = default)
    {
        InventoryGuard.ValidPage(request.PageNumber, request.PageSize);
        var rows = (await _accessService.QueryAsync<InventoryStockPagedRow, object>("dbo.spInventoryStockGetPaged", ToStockParameters(request), cancellationToken)).ToArray();
        return ToStockPagedResult(rows, request);
    }

    public async Task<IReadOnlyCollection<InventoryStockModel>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(productId, nameof(productId));
        var rows = await _accessService.QueryAsync<InventoryStockModel, object>("dbo.spInventoryStockGetByProductId", new { ProductId = productId }, cancellationToken);
        return rows.ToArray();
    }

    public async Task<InventoryStockPagedResultModel> GetLowStockPagedAsync(InventoryStockPagedRequestModel request, CancellationToken cancellationToken = default)
    {
        InventoryGuard.ValidPage(request.PageNumber, request.PageSize);
        var rows = (await _accessService.QueryAsync<InventoryStockPagedRow, object>("dbo.spInventoryStockGetLowStockPaged", ToStockParameters(request), cancellationToken)).ToArray();
        return ToStockPagedResult(rows, request);
    }

    public async Task<InventoryStockPagedResultModel> GetOutOfStockPagedAsync(InventoryStockPagedRequestModel request, CancellationToken cancellationToken = default)
    {
        InventoryGuard.ValidPage(request.PageNumber, request.PageSize);
        var rows = (await _accessService.QueryAsync<InventoryStockPagedRow, object>("dbo.spInventoryStockGetOutOfStockPaged", ToStockParameters(request), cancellationToken)).ToArray();
        return ToStockPagedResult(rows, request);
    }

    public Task<InventoryStockSummaryModel> GetSummaryAsync(int? locationId = null, CancellationToken cancellationToken = default)
    {
        if (locationId.HasValue) InventoryGuard.Positive(locationId.Value, nameof(locationId));
        return _accessService.QuerySingleAsync<InventoryStockSummaryModel, object>("dbo.spInventoryStockGetSummary", new { LocationId = locationId }, cancellationToken);
    }

    private static object ToStockParameters(InventoryStockPagedRequestModel request) => new
    {
        request.PageNumber,
        request.PageSize,
        SearchText = InventoryGuard.TrimOrNull(request.SearchText),
        request.LocationId,
        request.CategoryId,
        request.ActiveProductsOnly
    };

    private static InventoryStockPagedResultModel ToStockPagedResult(InventoryStockPagedRow[] rows, InventoryStockPagedRequestModel request) => new()
    {
        Stocks = rows,
        TotalCount = rows.FirstOrDefault()?.TotalCount ?? 0,
        PageNumber = request.PageNumber,
        PageSize = request.PageSize
    };
}

public sealed class InventoryMovementService : IInventoryMovementService
{
    private readonly IAccessService _accessService;

    public InventoryMovementService(IAccessService accessService) => _accessService = accessService;

    public async Task<InventoryMovementPagedResultModel> GetPagedAsync(InventoryMovementPagedRequestModel request, CancellationToken cancellationToken = default)
    {
        InventoryGuard.ValidPage(request.PageNumber, request.PageSize);
        var rows = (await _accessService.QueryAsync<InventoryMovementPagedRow, object>("dbo.spInventoryMovementGetPaged", new
        {
            request.PageNumber,
            request.PageSize,
            SearchText = InventoryGuard.TrimOrNull(request.SearchText),
            request.ProductId,
            request.LocationId,
            MovementType = InventoryGuard.TrimOrNull(request.MovementType),
            request.FromDate,
            request.ToDate
        }, cancellationToken)).ToArray();

        return new InventoryMovementPagedResultModel { Movements = rows, TotalCount = rows.FirstOrDefault()?.TotalCount ?? 0, PageNumber = request.PageNumber, PageSize = request.PageSize };
    }

    public async Task<IReadOnlyCollection<InventoryMovementModel>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(productId, nameof(productId));
        var rows = await _accessService.QueryAsync<InventoryMovementModel, object>("dbo.spInventoryMovementGetByProductId", new { ProductId = productId }, cancellationToken);
        return rows.ToArray();
    }

    public Task<long> CreateAsync(InventoryMovementCreateModel model, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(model.ProductId, nameof(model.ProductId));
        InventoryGuard.Positive(model.LocationId, nameof(model.LocationId));
        InventoryGuard.Required(model.MovementType, nameof(model.MovementType));
        InventoryGuard.Positive(model.Quantity, nameof(model.Quantity));
        InventoryGuard.NonNegative(model.UnitCost, nameof(model.UnitCost));
        InventoryGuard.Positive(model.CreatedByUserId, nameof(model.CreatedByUserId));

        return _accessService.QuerySingleAsync<long, object>("dbo.spInventoryMovementCreate", new
        {
            model.ProductId,
            model.LocationId,
            MovementType = model.MovementType.Trim(),
            model.Quantity,
            model.UnitCost,
            ReferenceType = InventoryGuard.TrimOrEmpty(model.ReferenceType),
            model.ReferenceId,
            ReferenceNo = InventoryGuard.TrimOrEmpty(model.ReferenceNo),
            Reason = InventoryGuard.TrimOrEmpty(model.Reason),
            Remarks = InventoryGuard.TrimOrEmpty(model.Remarks),
            model.AllowNegativeStock,
            model.CreatedByUserId
        }, cancellationToken);
    }

    public async Task<IReadOnlyCollection<InventoryMovementModel>> GetByReferenceNoAsync(string referenceNo, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Required(referenceNo, nameof(referenceNo));
        var rows = await _accessService.QueryAsync<InventoryMovementModel, object>("dbo.spInventoryMovementGetByReferenceNo", new { ReferenceNo = referenceNo.Trim() }, cancellationToken);
        return rows.ToArray();
    }

    public async Task<IReadOnlyCollection<InventoryMovementSummaryModel>> GetSummaryByDateRangeAsync(DateTime fromDate, DateTime toDate, int? locationId = null, CancellationToken cancellationToken = default)
    {
        if (toDate < fromDate) throw new ArgumentException("To date must be greater than or equal to from date.", nameof(toDate));
        if (locationId.HasValue) InventoryGuard.Positive(locationId.Value, nameof(locationId));
        var rows = await _accessService.QueryAsync<InventoryMovementSummaryModel, object>("dbo.spInventoryMovementGetSummaryByDateRange", new { FromDate = fromDate, ToDate = toDate, LocationId = locationId }, cancellationToken);
        return rows.ToArray();
    }
}

public sealed class StockAdjustmentService : IStockAdjustmentService
{
    private readonly IAccessService _accessService;

    public StockAdjustmentService(IAccessService accessService) => _accessService = accessService;

    public async Task<PagedResultModel<StockAdjustmentModel>> GetPagedAsync(int pageNumber, int pageSize, string? searchText = null, string? status = null, int? locationId = null, CancellationToken cancellationToken = default)
    {
        InventoryGuard.ValidPage(pageNumber, pageSize);
        var rows = (await _accessService.QueryAsync<StockAdjustmentPagedRow, object>("dbo.spStockAdjustmentGetPaged", new { PageNumber = pageNumber, PageSize = pageSize, SearchText = InventoryGuard.TrimOrNull(searchText), Status = InventoryGuard.TrimOrNull(status), LocationId = locationId }, cancellationToken)).ToArray();
        return new PagedResultModel<StockAdjustmentModel> { Items = rows, TotalCount = rows.FirstOrDefault()?.TotalCount ?? 0, PageNumber = pageNumber, PageSize = pageSize };
    }

    public Task<StockAdjustmentModel?> GetByIdAsync(long stockAdjustmentId, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(stockAdjustmentId, nameof(stockAdjustmentId));
        return _accessService.QueryMultipleAsync<object, StockAdjustmentModel?>(
            "dbo.spStockAdjustmentGetById",
            new { StockAdjustmentId = stockAdjustmentId },
            async reader =>
            {
                var adjustment = await reader.ReadSingleOrDefaultAsync<StockAdjustmentModel>();
                if (adjustment is null) return null;

                var items = (await reader.ReadAsync<StockAdjustmentItemModel>()).ToArray();
                return new StockAdjustmentModel
                {
                    StockAdjustmentId = adjustment.StockAdjustmentId,
                    AdjustmentNo = adjustment.AdjustmentNo,
                    LocationId = adjustment.LocationId,
                    LocationName = adjustment.LocationName,
                    Status = adjustment.Status,
                    Reason = adjustment.Reason,
                    Remarks = adjustment.Remarks,
                    CreatedByUserId = adjustment.CreatedByUserId,
                    CreatedDate = adjustment.CreatedDate,
                    UpdatedByUserId = adjustment.UpdatedByUserId,
                    UpdatedDate = adjustment.UpdatedDate,
                    Items = items
                };
            },
            cancellationToken);
    }

    public Task<long> CreateAsync(StockAdjustmentCreateModel model, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(model.LocationId, nameof(model.LocationId));
        InventoryGuard.Required(model.Reason, nameof(model.Reason));
        InventoryGuard.Positive(model.CreatedByUserId, nameof(model.CreatedByUserId));
        return _accessService.QuerySingleAsync<long, object>("dbo.spStockAdjustmentCreate", new { model.LocationId, Reason = model.Reason.Trim(), Remarks = InventoryGuard.TrimOrEmpty(model.Remarks), model.CreatedByUserId }, cancellationToken);
    }

    public Task<long> AddItemAsync(long stockAdjustmentId, int productId, decimal quantity, string adjustmentType, decimal unitCost, string reason, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(stockAdjustmentId, nameof(stockAdjustmentId));
        InventoryGuard.Positive(productId, nameof(productId));
        InventoryGuard.Positive(quantity, nameof(quantity));
        InventoryGuard.Required(adjustmentType, nameof(adjustmentType));
        InventoryGuard.NonNegative(unitCost, nameof(unitCost));
        InventoryGuard.Required(reason, nameof(reason));
        return _accessService.QuerySingleAsync<long, object>("dbo.spStockAdjustmentAddItem", new { StockAdjustmentId = stockAdjustmentId, ProductId = productId, Quantity = quantity, AdjustmentType = adjustmentType.Trim(), UnitCost = unitCost, Reason = reason.Trim() }, cancellationToken);
    }

    public Task ApproveAsync(StockAdjustmentApproveModel model, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(model.StockAdjustmentId, nameof(model.StockAdjustmentId));
        InventoryGuard.Positive(model.ApprovedByUserId, nameof(model.ApprovedByUserId));
        return _accessService.ExecuteAsync("dbo.spStockAdjustmentApprove", new { model.StockAdjustmentId, model.ApprovedByUserId }, cancellationToken);
    }

    public Task RejectAsync(long stockAdjustmentId, string reason, int rejectedByUserId, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(stockAdjustmentId, nameof(stockAdjustmentId));
        InventoryGuard.Required(reason, nameof(reason));
        InventoryGuard.Positive(rejectedByUserId, nameof(rejectedByUserId));
        return _accessService.ExecuteAsync("dbo.spStockAdjustmentReject", new { StockAdjustmentId = stockAdjustmentId, Reason = reason.Trim(), RejectedByUserId = rejectedByUserId }, cancellationToken);
    }

    public Task CancelAsync(long stockAdjustmentId, int cancelledByUserId, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(stockAdjustmentId, nameof(stockAdjustmentId));
        InventoryGuard.Positive(cancelledByUserId, nameof(cancelledByUserId));
        return _accessService.ExecuteAsync("dbo.spStockAdjustmentCancel", new { StockAdjustmentId = stockAdjustmentId, CancelledByUserId = cancelledByUserId }, cancellationToken);
    }
}

public sealed class StockCountService : IStockCountService
{
    private readonly IAccessService _accessService;

    public StockCountService(IAccessService accessService) => _accessService = accessService;

    public async Task<PagedResultModel<StockCountModel>> GetPagedAsync(int pageNumber, int pageSize, string? searchText = null, string? status = null, int? locationId = null, CancellationToken cancellationToken = default)
    {
        InventoryGuard.ValidPage(pageNumber, pageSize);
        var rows = (await _accessService.QueryAsync<StockCountPagedRow, object>("dbo.spStockCountGetPaged", new { PageNumber = pageNumber, PageSize = pageSize, SearchText = InventoryGuard.TrimOrNull(searchText), Status = InventoryGuard.TrimOrNull(status), LocationId = locationId }, cancellationToken)).ToArray();
        return new PagedResultModel<StockCountModel> { Items = rows, TotalCount = rows.FirstOrDefault()?.TotalCount ?? 0, PageNumber = pageNumber, PageSize = pageSize };
    }

    public Task<StockCountModel?> GetByIdAsync(long stockCountId, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(stockCountId, nameof(stockCountId));
        return _accessService.QueryMultipleAsync<object, StockCountModel?>(
            "dbo.spStockCountGetById",
            new { StockCountId = stockCountId },
            async reader =>
            {
                var count = await reader.ReadSingleOrDefaultAsync<StockCountModel>();
                if (count is null) return null;

                var items = (await reader.ReadAsync<StockCountItemModel>()).ToArray();
                return new StockCountModel
                {
                    StockCountId = count.StockCountId,
                    StockCountNo = count.StockCountNo,
                    LocationId = count.LocationId,
                    LocationName = count.LocationName,
                    Status = count.Status,
                    Remarks = count.Remarks,
                    CreatedByUserId = count.CreatedByUserId,
                    CreatedDate = count.CreatedDate,
                    UpdatedByUserId = count.UpdatedByUserId,
                    UpdatedDate = count.UpdatedDate,
                    Items = items
                };
            },
            cancellationToken);
    }

    public Task<long> CreateAsync(StockCountCreateModel model, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(model.LocationId, nameof(model.LocationId));
        InventoryGuard.Positive(model.CreatedByUserId, nameof(model.CreatedByUserId));
        return _accessService.QuerySingleAsync<long, object>("dbo.spStockCountCreate", new { model.LocationId, Remarks = InventoryGuard.TrimOrEmpty(model.Remarks), model.CreatedByUserId }, cancellationToken);
    }

    public Task<long> AddItemAsync(long stockCountId, int productId, decimal countedQty, string? remarks, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(stockCountId, nameof(stockCountId));
        InventoryGuard.Positive(productId, nameof(productId));
        InventoryGuard.NonNegative(countedQty, nameof(countedQty));
        return _accessService.QuerySingleAsync<long, object>("dbo.spStockCountAddItem", new { StockCountId = stockCountId, ProductId = productId, CountedQty = countedQty, Remarks = InventoryGuard.TrimOrEmpty(remarks) }, cancellationToken);
    }

    public Task UpdateCountedQtyAsync(long stockCountItemId, decimal countedQty, string? remarks, int updatedByUserId, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(stockCountItemId, nameof(stockCountItemId));
        InventoryGuard.NonNegative(countedQty, nameof(countedQty));
        InventoryGuard.Positive(updatedByUserId, nameof(updatedByUserId));
        return _accessService.ExecuteAsync("dbo.spStockCountUpdateCountedQty", new { StockCountItemId = stockCountItemId, CountedQty = countedQty, Remarks = InventoryGuard.TrimOrEmpty(remarks), UpdatedByUserId = updatedByUserId }, cancellationToken);
    }

    public Task ApproveAsync(StockCountApproveModel model, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(model.StockCountId, nameof(model.StockCountId));
        InventoryGuard.Positive(model.ApprovedByUserId, nameof(model.ApprovedByUserId));
        return _accessService.ExecuteAsync("dbo.spStockCountApprove", new { model.StockCountId, model.ApprovedByUserId }, cancellationToken);
    }

    public Task CancelAsync(long stockCountId, int cancelledByUserId, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(stockCountId, nameof(stockCountId));
        InventoryGuard.Positive(cancelledByUserId, nameof(cancelledByUserId));
        return _accessService.ExecuteAsync("dbo.spStockCountCancel", new { StockCountId = stockCountId, CancelledByUserId = cancelledByUserId }, cancellationToken);
    }
}

public sealed class StockTransferService : IStockTransferService
{
    private readonly IAccessService _accessService;

    public StockTransferService(IAccessService accessService) => _accessService = accessService;

    public async Task<PagedResultModel<StockTransferModel>> GetPagedAsync(int pageNumber, int pageSize, string? searchText = null, string? status = null, int? sourceLocationId = null, int? destinationLocationId = null, CancellationToken cancellationToken = default)
    {
        InventoryGuard.ValidPage(pageNumber, pageSize);
        var rows = (await _accessService.QueryAsync<StockTransferPagedRow, object>("dbo.spStockTransferGetPaged", new { PageNumber = pageNumber, PageSize = pageSize, SearchText = InventoryGuard.TrimOrNull(searchText), Status = InventoryGuard.TrimOrNull(status), SourceLocationId = sourceLocationId, DestinationLocationId = destinationLocationId }, cancellationToken)).ToArray();
        return new PagedResultModel<StockTransferModel> { Items = rows, TotalCount = rows.FirstOrDefault()?.TotalCount ?? 0, PageNumber = pageNumber, PageSize = pageSize };
    }

    public Task<StockTransferModel?> GetByIdAsync(long stockTransferId, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(stockTransferId, nameof(stockTransferId));
        return _accessService.QueryMultipleAsync<object, StockTransferModel?>(
            "dbo.spStockTransferGetById",
            new { StockTransferId = stockTransferId },
            async reader =>
            {
                var transfer = await reader.ReadSingleOrDefaultAsync<StockTransferModel>();
                if (transfer is null) return null;

                var items = (await reader.ReadAsync<StockTransferItemModel>()).ToArray();
                return new StockTransferModel
                {
                    StockTransferId = transfer.StockTransferId,
                    TransferNo = transfer.TransferNo,
                    SourceLocationId = transfer.SourceLocationId,
                    SourceLocationName = transfer.SourceLocationName,
                    DestinationLocationId = transfer.DestinationLocationId,
                    DestinationLocationName = transfer.DestinationLocationName,
                    Status = transfer.Status,
                    Remarks = transfer.Remarks,
                    SentDate = transfer.SentDate,
                    ReceivedDate = transfer.ReceivedDate,
                    CreatedByUserId = transfer.CreatedByUserId,
                    CreatedDate = transfer.CreatedDate,
                    UpdatedByUserId = transfer.UpdatedByUserId,
                    UpdatedDate = transfer.UpdatedDate,
                    Items = items
                };
            },
            cancellationToken);
    }

    public Task<long> CreateAsync(StockTransferCreateModel model, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(model.SourceLocationId, nameof(model.SourceLocationId));
        InventoryGuard.Positive(model.DestinationLocationId, nameof(model.DestinationLocationId));
        if (model.SourceLocationId == model.DestinationLocationId) throw new InvalidOperationException("Source and destination locations must be different.");
        InventoryGuard.Positive(model.CreatedByUserId, nameof(model.CreatedByUserId));
        return _accessService.QuerySingleAsync<long, object>("dbo.spStockTransferCreate", new { model.SourceLocationId, model.DestinationLocationId, Remarks = InventoryGuard.TrimOrEmpty(model.Remarks), model.CreatedByUserId }, cancellationToken);
    }

    public Task<long> AddItemAsync(long stockTransferId, int productId, decimal quantity, decimal unitCost, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(stockTransferId, nameof(stockTransferId));
        InventoryGuard.Positive(productId, nameof(productId));
        InventoryGuard.Positive(quantity, nameof(quantity));
        InventoryGuard.NonNegative(unitCost, nameof(unitCost));
        return _accessService.QuerySingleAsync<long, object>("dbo.spStockTransferAddItem", new { StockTransferId = stockTransferId, ProductId = productId, Quantity = quantity, UnitCost = unitCost }, cancellationToken);
    }

    public Task SendAsync(StockTransferSendModel model, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(model.StockTransferId, nameof(model.StockTransferId));
        InventoryGuard.Positive(model.SentByUserId, nameof(model.SentByUserId));
        return _accessService.ExecuteAsync("dbo.spStockTransferSend", new { model.StockTransferId, model.SentByUserId }, cancellationToken);
    }

    public Task ReceiveAsync(StockTransferReceiveModel model, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(model.StockTransferId, nameof(model.StockTransferId));
        InventoryGuard.Positive(model.ReceivedByUserId, nameof(model.ReceivedByUserId));
        return _accessService.ExecuteAsync("dbo.spStockTransferReceive", new { model.StockTransferId, model.ReceivedByUserId }, cancellationToken);
    }

    public Task CancelAsync(long stockTransferId, int cancelledByUserId, CancellationToken cancellationToken = default)
    {
        InventoryGuard.Positive(stockTransferId, nameof(stockTransferId));
        InventoryGuard.Positive(cancelledByUserId, nameof(cancelledByUserId));
        return _accessService.ExecuteAsync("dbo.spStockTransferCancel", new { StockTransferId = stockTransferId, CancelledByUserId = cancelledByUserId }, cancellationToken);
    }
}

internal sealed class InventoryStockPagedRow : InventoryStockModel { public int TotalCount { get; init; } }
internal sealed class InventoryMovementPagedRow : InventoryMovementModel { public int TotalCount { get; init; } }
internal sealed class StockAdjustmentPagedRow : StockAdjustmentModel { public int TotalCount { get; init; } }
internal sealed class StockCountPagedRow : StockCountModel { public int TotalCount { get; init; } }
internal sealed class StockTransferPagedRow : StockTransferModel { public int TotalCount { get; init; } }

internal static class InventoryGuard
{
    public static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    public static string TrimOrEmpty(string? value) => value?.Trim() ?? string.Empty;

    public static void Required(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{name} is required.", name);
    }

    public static void Positive(int value, string name)
    {
        if (value <= 0) throw new ArgumentException($"{name} must be greater than zero.", name);
    }

    public static void Positive(long value, string name)
    {
        if (value <= 0) throw new ArgumentException($"{name} must be greater than zero.", name);
    }

    public static void Positive(decimal value, string name)
    {
        if (value <= 0) throw new ArgumentException($"{name} must be greater than zero.", name);
    }

    public static void NonNegative(decimal value, string name)
    {
        if (value < 0) throw new ArgumentException($"{name} must be zero or greater.", name);
    }

    public static void ValidPage(int pageNumber, int pageSize)
    {
        if (pageNumber <= 0) throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));
        if (pageSize <= 0 || pageSize > 500) throw new ArgumentException("Page size must be between 1 and 500.", nameof(pageSize));
    }
}
