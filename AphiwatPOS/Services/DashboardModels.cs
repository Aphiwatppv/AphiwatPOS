using InventoryEngine.Models;
using SalesEngine.Models;

namespace AphiwatPOS.Services;

public sealed class ManagerDashboardModel
{
    public SalesOverviewModel SalesOverview { get; init; } = new();
    public ProfitSummaryModel ProfitSummary { get; init; } = new();
    public InventoryOverviewModel InventoryOverview { get; init; } = new();
    public CustomerOverviewModel CustomerOverview { get; init; } = new();
    public IReadOnlyCollection<CashierPerformanceModel> CashierPerformance { get; init; } = Array.Empty<CashierPerformanceModel>();
    public PaymentSummaryModel PaymentSummary { get; init; } = new();
    public IReadOnlyCollection<DashboardAlertModel> Alerts { get; init; } = Array.Empty<DashboardAlertModel>();
}

public sealed class CashierDashboardModel
{
    public TodayShiftSummaryModel ShiftSummary { get; init; } = new();
    public IReadOnlyCollection<SalesHeaderModel> RecentSales { get; init; } = Array.Empty<SalesHeaderModel>();
    public IReadOnlyCollection<HeldSaleHeaderModel> HeldSales { get; init; } = Array.Empty<HeldSaleHeaderModel>();
    public IReadOnlyCollection<PendingTaskModel> PendingTasks { get; init; } = Array.Empty<PendingTaskModel>();
    public IReadOnlyCollection<CustomerCreditTodayModel> CustomerCreditToday { get; init; } = Array.Empty<CustomerCreditTodayModel>();
    public CustomerDisplayStatusModel CustomerDisplayStatus { get; init; } = new();
    public IReadOnlyCollection<DashboardNotificationModel> Notifications { get; init; } = Array.Empty<DashboardNotificationModel>();
}

public sealed class SalesOverviewModel
{
    public decimal TodaySales { get; init; }
    public decimal WeekSales { get; init; }
    public decimal MonthSales { get; init; }
    public int TotalTransactions { get; init; }
    public decimal AverageBillAmount { get; init; }
    public IReadOnlyCollection<DashboardTrendPointModel> SalesTrend { get; init; } = Array.Empty<DashboardTrendPointModel>();
    public IReadOnlyCollection<DashboardProductMetricModel> BestSellingProducts { get; init; } = Array.Empty<DashboardProductMetricModel>();
}

public sealed class ProfitSummaryModel
{
    public decimal GrossSales { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal NetSales { get; init; }
    public decimal CostOfGoodsSold { get; init; }
    public decimal EstimatedProfit { get; init; }
    public decimal ProfitMarginPercent { get; init; }
}

public sealed class InventoryOverviewModel
{
    public int TotalProducts { get; init; }
    public int LowStockProducts { get; init; }
    public int OutOfStockProducts { get; init; }
    public decimal StockValue { get; init; }
    public decimal StockInQty { get; init; }
    public decimal StockOutQty { get; init; }
    public IReadOnlyCollection<InventoryLowStockProductModel> LowStockList { get; init; } = Array.Empty<InventoryLowStockProductModel>();
    public IReadOnlyCollection<InventoryTopMovingProductModel> FastMovingProducts { get; init; } = Array.Empty<InventoryTopMovingProductModel>();
    public IReadOnlyCollection<InventoryTopMovingProductModel> SlowMovingProducts { get; init; } = Array.Empty<InventoryTopMovingProductModel>();
}

public sealed class CustomerOverviewModel
{
    public int TotalCustomers { get; init; }
    public int NewCustomersThisMonth { get; init; }
    public int MemberCustomers { get; init; }
    public int CustomersUsingCredit { get; init; }
    public decimal OutstandingCustomerCredit { get; init; }
    public IReadOnlyCollection<CustomerEngine.Models.TopCustomerModel> TopCustomers { get; init; } = Array.Empty<CustomerEngine.Models.TopCustomerModel>();
}

public sealed class CashierPerformanceModel
{
    public int CashierId { get; init; }
    public string CashierName { get; init; } = string.Empty;
    public decimal SalesAmount { get; init; }
    public int TransactionCount { get; init; }
    public decimal RefundAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal CashDifferenceAmount { get; init; }
}

public sealed class PaymentSummaryModel
{
    public decimal CashSales { get; init; }
    public decimal QrPaymentSales { get; init; }
    public decimal BankTransferSales { get; init; }
    public decimal CreditSales { get; init; }
    public decimal MixedPaymentSales { get; init; }
    public decimal OutstandingCreditPayment { get; init; }
}

public sealed class TodayShiftSummaryModel
{
    public string CashierName { get; init; } = string.Empty;
    public DateTime ShiftStartTime { get; init; }
    public decimal TodaySales { get; init; }
    public int TransactionCount { get; init; }
    public decimal CashReceived { get; init; }
    public decimal QrPaymentReceived { get; init; }
    public decimal RefundAmount { get; init; }
    public int HeldSalesCount { get; init; }
}

public sealed class PendingTaskModel
{
    public string Title { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
    public string Status { get; init; } = "info";
    public string Url { get; init; } = string.Empty;
}

public sealed class CustomerCreditTodayModel
{
    public string CustomerName { get; init; } = string.Empty;
    public decimal CreditAmount { get; init; }
    public decimal DueAmount { get; init; }
    public string PaymentStatus { get; init; } = "Pending";
}

public sealed class CustomerDisplayStatusModel
{
    public bool IsConnected { get; init; }
    public string TerminalId { get; init; } = "default";
    public string CurrentSaleStatus { get; init; } = "Ready";
}

public sealed class DashboardAlertModel
{
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Severity { get; init; } = "info";
    public string Url { get; init; } = string.Empty;
}

public sealed class DashboardNotificationModel
{
    public string Message { get; init; } = string.Empty;
    public string Severity { get; init; } = "info";
}

public sealed class DashboardTrendPointModel
{
    public string Label { get; init; } = string.Empty;
    public decimal Value { get; init; }
}

public sealed class DashboardProductMetricModel
{
    public string ProductName { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal Amount { get; init; }
}
