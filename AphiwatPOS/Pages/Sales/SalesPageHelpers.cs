using System.Security.Claims;
using SalesEngine.Models;

namespace AphiwatPOS.Pages.Sales;

public static class SalesPageHelpers
{
    public static int CurrentUserId(ClaimsPrincipal user) =>
        int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ? userId : 0;

    public static bool HasSalesAccess(ClaimsPrincipal user) =>
        user.IsInRole("Admin") || user.Claims.Any(claim => claim.Type == "Permission" && claim.Value.StartsWith("SALES_", StringComparison.OrdinalIgnoreCase));

    public static bool HasPermission(ClaimsPrincipal user, string permissionCode) =>
        user.IsInRole("Admin") || user.Claims.Any(claim => claim.Type == "Permission" && claim.Value.Equals(permissionCode, StringComparison.OrdinalIgnoreCase));

    public static string StatusBadge(string status) => status switch
    {
        "Completed" => "badge-success",
        "PartiallyRefunded" => "badge-warning",
        "Refunded" => "badge-danger",
        "Voided" => "badge-muted",
        "Held" => "badge-warning",
        "Resumed" => "badge-success",
        "Cancelled" => "badge-danger",
        "Expired" => "badge-muted",
        "Draft" => "badge-muted",
        "Approved" => "badge-success",
        "Rejected" => "badge-danger",
        _ => "badge-muted"
    };

    public static int TotalPages(int totalCount, int pageSize) => Math.Max(1, (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize)));

    public static SalesCartItemModel ToCartItem(SalesCartItemInput input) => new()
    {
        ProductId = input.ProductId,
        LocationId = input.LocationId,
        Quantity = input.Quantity,
        UnitPrice = input.UnitPrice,
        ItemDiscountAmount = input.ItemDiscountAmount,
        TaxAmount = input.TaxAmount
    };

    public static SalesPaymentInputModel ToPayment(SalesPaymentInput input) => new()
    {
        PaymentMethodId = input.PaymentMethodId,
        PaymentAmount = input.PaymentAmount,
        ReferenceNo = input.ReferenceNo
    };

    public static SalesReturnPaymentInputModel ToReturnPayment(SalesReturnPaymentInput input) => new()
    {
        PaymentMethodId = input.PaymentMethodId,
        RefundAmount = input.RefundAmount,
        ReferenceNo = input.ReferenceNo
    };
}

public sealed class SalesCartItemInput
{
    public int ProductId { get; set; }
    public int LocationId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public string? UnitSymbol { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal ItemDiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
}

public sealed class SalesPaymentInput
{
    public int PaymentMethodId { get; set; }
    public string? Name { get; set; }
    public decimal PaymentAmount { get; set; }
    public string? ReferenceNo { get; set; }
}

public sealed class SalesReturnPaymentInput
{
    public int PaymentMethodId { get; set; }
    public decimal RefundAmount { get; set; }
    public string? ReferenceNo { get; set; }
}
