using System.Security.Claims;
using InventoryEngine.Models;
using ProductEngine.Models;

namespace AphiwatPOS.Pages.Inventory;

public sealed class InventoryStockRow
{
    public long InventoryStockId { get; init; }
    public int ProductId { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public string Barcode { get; init; } = string.Empty;
    public string ProductImageUrl { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public string BrandName { get; init; } = string.Empty;
    public string UnitName { get; init; } = string.Empty;
    public string UnitSymbol { get; init; } = string.Empty;
    public int LocationId { get; init; }
    public string LocationCode { get; init; } = string.Empty;
    public string LocationName { get; init; } = string.Empty;
    public decimal CurrentStock { get; init; }
    public decimal MinimumStockLevel { get; init; }
    public decimal CostPrice { get; init; }
    public DateTime LastMovementDate { get; init; }
    public bool IsStockTracked { get; init; } = true;
    public decimal ShortageQty => Math.Max(0, MinimumStockLevel - CurrentStock);
    public decimal StockValue => Math.Max(0, CurrentStock) * CostPrice;
    public string StockStatus => !IsStockTracked ? "Not Tracked" : CurrentStock <= 0 ? "Out of Stock" : CurrentStock <= MinimumStockLevel ? "Low Stock" : "Normal";
}

public sealed class InventoryActionPermissions
{
    public bool CanView { get; init; }
    public bool CanCreate { get; init; }
    public bool CanUpdate { get; init; }
    public bool CanApprove { get; init; }
    public bool CanCancel { get; init; }
}

public static class InventoryUi
{
    public static bool HasPermission(ClaimsPrincipal user, string permissionCode)
    {
        return user.IsInRole("Admin") ||
            user.Claims.Any(claim =>
                claim.Type == "Permission" &&
                string.Equals(claim.Value, permissionCode, StringComparison.OrdinalIgnoreCase));
    }

    public static bool HasAnyInventoryAccess(ClaimsPrincipal user)
    {
        return user.IsInRole("Admin") ||
            user.Claims.Any(claim => claim.Type == "Permission" && claim.Value.StartsWith("INVENTORY_", StringComparison.OrdinalIgnoreCase));
    }

    public static int CurrentUserId(ClaimsPrincipal user)
    {
        return int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : 0;
    }

    public static int TotalPages(int totalCount, int pageSize)
    {
        return Math.Max(1, (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)));
    }

    public static string StatusBadge(string status)
    {
        return status switch
        {
            "Normal" or "Approved" or "Received" or "StockIn" or "PurchaseReceive" or "Return" or "TransferIn" or "AdjustmentIn" => "badge-success",
            "Low Stock" or "Draft" or "Sent" or "StockCountCorrection" or "AdjustmentOut" or "Pending" or "Counting" or "Pending Approval" => "badge-warning",
            "Out of Stock" or "Rejected" or "Sale" or "StockOut" or "TransferOut" => "badge-danger",
            _ => "badge-muted"
        };
    }

    public static bool IsOutMovement(string movementType, decimal signedQty)
    {
        return signedQty < 0 ||
            movementType is "StockOut" or "Sale" or "TransferOut" or "AdjustmentOut";
    }

    public static InventoryStockRow ToStockRow(InventoryStockModel stock, ProductModel? product)
    {
        return new InventoryStockRow
        {
            InventoryStockId = stock.InventoryStockId,
            ProductId = stock.ProductId,
            ProductCode = stock.ProductCode,
            ProductName = stock.ProductName,
            SKU = product?.SKU ?? string.Empty,
            Barcode = product?.Barcode ?? string.Empty,
            ProductImageUrl = product?.ProductImageUrl ?? string.Empty,
            CategoryName = product?.CategoryName ?? string.Empty,
            BrandName = product?.BrandName ?? string.Empty,
            UnitName = product?.UnitName ?? string.Empty,
            UnitSymbol = product?.UnitName ?? string.Empty,
            LocationId = stock.LocationId,
            LocationCode = stock.LocationCode,
            LocationName = stock.LocationName,
            CurrentStock = stock.CurrentStock,
            MinimumStockLevel = stock.MinimumStockLevel,
            CostPrice = product?.CostPrice ?? 0,
            LastMovementDate = stock.LastMovementDate,
            IsStockTracked = product?.IsStockTracked ?? true
        };
    }
}
