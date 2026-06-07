using CustomerEngine.Models;
using CustomerEngine.Services;
using InventoryEngine.Models;
using InventoryEngine.Services;
using ProductEngine.Services;
using SalesEngine.Models;
using SalesEngine.Services;

namespace AphiwatPOS.Services;

public sealed class ManagerDashboardService : IManagerDashboardService
{
    private readonly ISalesHistoryService _salesHistoryService;
    private readonly ISalesClosingService _salesClosingService;
    private readonly IHeldSaleService _heldSaleService;
    private readonly IInventoryDashboardService _inventoryDashboardService;
    private readonly IProductService _productService;
    private readonly ICustomerReportService _customerReportService;

    public ManagerDashboardService(ISalesHistoryService salesHistoryService, ISalesClosingService salesClosingService, IHeldSaleService heldSaleService, IInventoryDashboardService inventoryDashboardService, IProductService productService, ICustomerReportService customerReportService)
    {
        _salesHistoryService = salesHistoryService;
        _salesClosingService = salesClosingService;
        _heldSaleService = heldSaleService;
        _inventoryDashboardService = inventoryDashboardService;
        _productService = productService;
        _customerReportService = customerReportService;
    }

    public async Task<ManagerDashboardModel> GetDashboardAsync(CancellationToken cancellationToken = default) => new()
    {
        SalesOverview = await GetSalesOverviewAsync(cancellationToken),
        ProfitSummary = await GetProfitSummaryAsync(cancellationToken),
        InventoryOverview = await GetInventoryOverviewAsync(cancellationToken),
        CustomerOverview = await GetCustomerOverviewAsync(cancellationToken),
        CashierPerformance = await GetCashierPerformanceAsync(cancellationToken),
        PaymentSummary = await GetPaymentSummaryAsync(cancellationToken),
        Alerts = await GetDashboardAlertsAsync(cancellationToken)
    };

    public async Task<SalesOverviewModel> GetSalesOverviewAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var weekStart = today.AddDays(-((int)today.DayOfWeek + 6) % 7);
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var trendFrom = today.AddDays(-13);

        var todaySummary = await _salesHistoryService.GetSummaryByDateRangeAsync(today, today, null, cancellationToken);
        var weekSummary = await _salesHistoryService.GetSummaryByDateRangeAsync(weekStart, today, null, cancellationToken);
        var monthSummary = await _salesHistoryService.GetSummaryByDateRangeAsync(monthStart, today, null, cancellationToken);
        var trendSummary = await _salesHistoryService.GetSummaryByDateRangeAsync(trendFrom, today, null, cancellationToken);
        var recentSales = await _salesHistoryService.GetPagedAsync(new SalesPagedRequestModel { PageNumber = 1, PageSize = 25, FromDate = monthStart, ToDate = today }, cancellationToken);

        var trendByDate = trendSummary
            .GroupBy(x => x.SaleDate.Date)
            .ToDictionary(x => x.Key, x => x.Sum(row => row.NetAmount));
        var salesTrend = Enumerable.Range(0, 14)
            .Select(offset =>
            {
                var date = trendFrom.AddDays(offset).Date;
                return new DashboardTrendPointModel
                {
                    Label = date.ToString("MMM dd"),
                    Value = trendByDate.TryGetValue(date, out var value) ? value : 0
                };
            })
            .ToArray();

        var monthTransactions = monthSummary.Sum(x => x.TransactionCount);
        return new SalesOverviewModel
        {
            TodaySales = todaySummary.Sum(x => x.NetAmount),
            WeekSales = weekSummary.Sum(x => x.NetAmount),
            MonthSales = monthSummary.Sum(x => x.NetAmount),
            TotalTransactions = monthTransactions,
            AverageBillAmount = monthTransactions == 0 ? 0 : monthSummary.Sum(x => x.NetAmount) / monthTransactions,
            SalesTrend = salesTrend,
            BestSellingProducts = await BestSellingProductsAsync(recentSales.Sales, cancellationToken)
        };
    }

    public async Task<ProfitSummaryModel> GetProfitSummaryAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var summary = await _salesHistoryService.GetSummaryByDateRangeAsync(monthStart, today, null, cancellationToken);
        var gross = summary.Sum(x => x.GrossAmount);
        var discount = summary.Sum(x => x.DiscountAmount);
        var net = summary.Sum(x => x.NetAmount);
        var cogs = summary.Sum(x => x.CostOfGoodsSold);
        var profit = summary.Sum(x => x.GrossProfitAmount);
        var vatIn = summary.Sum(x => x.VatInAmount);
        var vatOut = summary.Sum(x => x.VatOutAmount);
        return new ProfitSummaryModel { GrossSales = gross, DiscountAmount = discount, NetSales = net, CostOfGoodsSold = cogs, VatInAmount = vatIn, VatOutAmount = vatOut, EstimatedProfit = profit, ProfitMarginPercent = net == 0 ? 0 : profit / net * 100 };
    }

    public async Task<InventoryOverviewModel> GetInventoryOverviewAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var filter = new InventoryDashboardFilterModel { DateFrom = monthStart, DateTo = today };
        var summary = await _inventoryDashboardService.GetSummaryAsync(filter, cancellationToken);
        var lowStock = await _inventoryDashboardService.GetLowStockProductsAsync(filter, 8, cancellationToken);
        var moving = (await _inventoryDashboardService.GetTopMovingProductsAsync(filter, 8, cancellationToken)).ToArray();

        return new InventoryOverviewModel
        {
            TotalProducts = summary.TotalProducts,
            LowStockProducts = summary.LowStockProducts,
            OutOfStockProducts = summary.OutOfStockProducts,
            StockValue = summary.TotalStockValue,
            StockInQty = summary.StockInQty,
            StockOutQty = summary.StockOutQty,
            LowStockList = lowStock,
            FastMovingProducts = moving,
            SlowMovingProducts = moving.OrderBy(x => x.TotalMovedQty).Take(5).ToArray()
        };
    }

    public async Task<CustomerOverviewModel> GetCustomerOverviewAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var summary = await _customerReportService.GetSummaryAsync(new CustomerReportRequestModel { DateFrom = monthStart, DateTo = today, Top = 10 }, cancellationToken);
        var topCustomers = await _customerReportService.GetTopCustomersBySpendingAsync(new CustomerReportRequestModel { DateFrom = monthStart, DateTo = today, Top = 8 }, cancellationToken);
        var memberLevels = await _customerReportService.GetMemberLevelSummaryAsync(new CustomerReportRequestModel { Top = 20 }, cancellationToken);

        return new CustomerOverviewModel
        {
            TotalCustomers = summary.TotalCustomers,
            NewCustomersThisMonth = summary.NewCustomers,
            MemberCustomers = memberLevels.Sum(x => x.CustomerCount),
            CustomersUsingCredit = summary.TotalCreditCustomers,
            OutstandingCustomerCredit = summary.TotalOutstandingCredit,
            TopCustomers = topCustomers
        };
    }

    public async Task<IReadOnlyCollection<CashierPerformanceModel>> GetCashierPerformanceAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var sales = await _salesHistoryService.GetPagedAsync(new SalesPagedRequestModel { PageNumber = 1, PageSize = 200, FromDate = monthStart, ToDate = today }, cancellationToken);

        return sales.Sales.GroupBy(x => new { x.CashierUserId, x.CashierName })
            .Select(group => new CashierPerformanceModel
            {
                CashierId = group.Key.CashierUserId,
                CashierName = string.IsNullOrWhiteSpace(group.Key.CashierName) ? $"User {group.Key.CashierUserId}" : group.Key.CashierName,
                SalesAmount = group.Sum(x => x.NetAmount),
                TransactionCount = group.Count(),
                DiscountAmount = group.Sum(x => x.TotalDiscountAmount),
                RefundAmount = 0,
                CashDifferenceAmount = 0
            })
            .OrderByDescending(x => x.SalesAmount)
            .Take(8)
            .ToArray();
    }

    public async Task<PaymentSummaryModel> GetPaymentSummaryAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var sales = await _salesHistoryService.GetPagedAsync(new SalesPagedRequestModel { PageNumber = 1, PageSize = 100, FromDate = monthStart, ToDate = today }, cancellationToken);
        var payments = new List<SalesPaymentModel>();
        foreach (var sale in sales.Sales.Take(60))
        {
            payments.AddRange(await _salesHistoryService.GetPaymentsAsync(sale.SalesHeaderId, cancellationToken));
        }

        decimal SumBy(params string[] tokens) => payments.Where(p => tokens.Any(token => p.PaymentMethodName.Contains(token, StringComparison.OrdinalIgnoreCase))).Sum(p => p.PaymentAmount);
        var mixedPaymentTotal = sales.Sales
            .Where(sale => payments.Count(p => p.SalesHeaderId == sale.SalesHeaderId) > 1)
            .Sum(sale => sale.NetAmount);

        return new PaymentSummaryModel
        {
            CashSales = SumBy("cash"),
            QrPaymentSales = SumBy("qr", "promptpay"),
            BankTransferSales = SumBy("bank", "transfer"),
            CreditSales = SumBy("credit"),
            MixedPaymentSales = mixedPaymentTotal,
            OutstandingCreditPayment = SumBy("credit")
        };
    }

    public async Task<IReadOnlyCollection<DashboardAlertModel>> GetDashboardAlertsAsync(CancellationToken cancellationToken = default)
    {
        var inventory = await GetInventoryOverviewAsync(cancellationToken);
        var credit = await _customerReportService.GetCreditSummaryAsync(new CustomerReportRequestModel(), cancellationToken);
        var held = await _heldSaleService.GetPagedAsync(new HeldSalePagedRequestModel { PageNumber = 1, PageSize = 1, Status = "Held" }, cancellationToken);
        var alerts = new List<DashboardAlertModel>();
        if (inventory.LowStockProducts > 0) alerts.Add(new DashboardAlertModel { Title = "Low stock warning", Message = $"{inventory.LowStockProducts} products need stock attention.", Severity = "warning", Url = "/Inventory/LowStock" });
        if (inventory.OutOfStockProducts > 0) alerts.Add(new DashboardAlertModel { Title = "Out of stock warning", Message = $"{inventory.OutOfStockProducts} products are out of stock.", Severity = "danger", Url = "/Inventory/CurrentStock" });
        if (credit.TotalOutstandingAmount > 0) alerts.Add(new DashboardAlertModel { Title = "Unpaid customer credit", Message = $"{credit.TotalOutstandingAmount:N2} outstanding credit.", Severity = "warning", Url = "/Customers/Credit" });
        if (held.TotalCount > 0) alerts.Add(new DashboardAlertModel { Title = "Held sales waiting", Message = $"{held.TotalCount} held sales are open.", Severity = "info", Url = "/Sales/HeldSales" });
        alerts.Add(new DashboardAlertModel { Title = "Cashier shift closing", Message = "Review daily closing before end of day.", Severity = "info", Url = "/Sales/DailyClosing" });
        return alerts;
    }

    private async Task<IReadOnlyCollection<DashboardProductMetricModel>> BestSellingProductsAsync(IEnumerable<SalesHeaderModel> sales, CancellationToken cancellationToken)
    {
        var items = new List<SalesItemModel>();
        foreach (var sale in sales.Take(40))
        {
            items.AddRange(await _salesHistoryService.GetItemsAsync(sale.SalesHeaderId, cancellationToken));
        }

        return items.GroupBy(x => x.ProductNameSnapshot)
            .Select(group => new DashboardProductMetricModel { ProductName = group.Key, Quantity = group.Sum(x => x.Quantity), Amount = group.Sum(x => x.LineTotal) })
            .OrderByDescending(x => x.Quantity)
            .Take(8)
            .ToArray();
    }
}

public sealed class CashierDashboardService : ICashierDashboardService
{
    private readonly ISalesHistoryService _salesHistoryService;
    private readonly ISalesClosingService _salesClosingService;
    private readonly IHeldSaleService _heldSaleService;

    public CashierDashboardService(ISalesHistoryService salesHistoryService, ISalesClosingService salesClosingService, IHeldSaleService heldSaleService)
    {
        _salesHistoryService = salesHistoryService;
        _salesClosingService = salesClosingService;
        _heldSaleService = heldSaleService;
    }

    public async Task<CashierDashboardModel> GetDashboardAsync(int cashierId, string cashierName, string? terminalId, CancellationToken cancellationToken = default)
    {
        var shift = await GetTodayShiftSummaryAsync(cashierId, cancellationToken);
        return new CashierDashboardModel
        {
            ShiftSummary = new TodayShiftSummaryModel
            {
                CashierName = cashierName,
                ShiftStartTime = shift.ShiftStartTime,
                TodaySales = shift.TodaySales,
                TransactionCount = shift.TransactionCount,
                CashReceived = shift.CashReceived,
                QrPaymentReceived = shift.QrPaymentReceived,
                RefundAmount = shift.RefundAmount,
                HeldSalesCount = shift.HeldSalesCount
            },
            RecentSales = await GetRecentSalesAsync(cashierId, cancellationToken),
            HeldSales = await GetHeldSalesAsync(cashierId, cancellationToken),
            PendingTasks = await GetPendingTasksAsync(cashierId, cancellationToken),
            CustomerCreditToday = await GetCustomerCreditTodayAsync(cashierId, cancellationToken),
            CustomerDisplayStatus = await GetCustomerDisplayStatusAsync(terminalId, cancellationToken),
            Notifications = await GetCashierNotificationsAsync(cashierId, cancellationToken)
        };
    }

    public async Task<TodayShiftSummaryModel> GetTodayShiftSummaryAsync(int cashierId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var sales = await _salesHistoryService.GetPagedAsync(new SalesPagedRequestModel { PageNumber = 1, PageSize = 100, CashierUserId = cashierId, FromDate = today, ToDate = today }, cancellationToken);
        var held = await GetHeldSalesAsync(cashierId, cancellationToken);
        var payments = new List<SalesPaymentModel>();
        foreach (var sale in sales.Sales.Take(40))
        {
            payments.AddRange(await _salesHistoryService.GetPaymentsAsync(sale.SalesHeaderId, cancellationToken));
        }

        decimal SumBy(params string[] tokens) => payments.Where(p => tokens.Any(token => p.PaymentMethodName.Contains(token, StringComparison.OrdinalIgnoreCase))).Sum(p => p.PaymentAmount);
        return new TodayShiftSummaryModel
        {
            ShiftStartTime = today.AddHours(8),
            TodaySales = sales.Sales.Sum(x => x.NetAmount),
            TransactionCount = sales.TotalCount,
            CashReceived = SumBy("cash"),
            QrPaymentReceived = SumBy("qr", "promptpay", "bank", "transfer"),
            RefundAmount = 0,
            HeldSalesCount = held.Count
        };
    }

    public async Task<IReadOnlyCollection<SalesHeaderModel>> GetRecentSalesAsync(int cashierId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var sales = await _salesHistoryService.GetPagedAsync(new SalesPagedRequestModel { PageNumber = 1, PageSize = 8, CashierUserId = cashierId, FromDate = today, ToDate = today }, cancellationToken);
        return sales.Sales;
    }

    public async Task<IReadOnlyCollection<HeldSaleHeaderModel>> GetHeldSalesAsync(int cashierId, CancellationToken cancellationToken = default)
    {
        var held = await _heldSaleService.GetPagedAsync(new HeldSalePagedRequestModel { PageNumber = 1, PageSize = 6, CashierUserId = cashierId, Status = "Held" }, cancellationToken);
        return held.HeldSales;
    }

    public async Task<IReadOnlyCollection<PendingTaskModel>> GetPendingTasksAsync(int cashierId, CancellationToken cancellationToken = default)
    {
        var held = await GetHeldSalesAsync(cashierId, cancellationToken);
        var tasks = new List<PendingTaskModel>();
        if (held.Count > 0) tasks.Add(new PendingTaskModel { Title = "Held sales waiting", Detail = $"{held.Count} held sales can be continued.", Status = "warning", Url = "/Sales/HeldSales" });
        tasks.Add(new PendingTaskModel { Title = "Sales waiting for print", Detail = "Print last receipt after payment if customer requests it.", Status = "info", Url = "/Sales/SalesHistory" });
        tasks.Add(new PendingTaskModel { Title = "Failed payments", Detail = "Review payment failures from checkout notifications.", Status = "info", Url = "/Sales/POSCheckout" });
        return tasks;
    }

    public async Task<IReadOnlyCollection<CustomerCreditTodayModel>> GetCustomerCreditTodayAsync(int cashierId, CancellationToken cancellationToken = default)
    {
        var recent = await GetRecentSalesAsync(cashierId, cancellationToken);
        var creditRows = new List<CustomerCreditTodayModel>();
        foreach (var sale in recent.Where(x => x.CustomerId.HasValue).Take(10))
        {
            var payments = await _salesHistoryService.GetPaymentsAsync(sale.SalesHeaderId, cancellationToken);
            var credit = payments.Where(p => p.PaymentMethodName.Contains("credit", StringComparison.OrdinalIgnoreCase)).Sum(p => p.PaymentAmount);
            if (credit > 0) creditRows.Add(new CustomerCreditTodayModel { CustomerName = sale.CustomerName ?? "Customer", CreditAmount = credit, DueAmount = credit, PaymentStatus = "Pending" });
        }
        return creditRows;
    }

    public Task<CustomerDisplayStatusModel> GetCustomerDisplayStatusAsync(string? terminalId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new CustomerDisplayStatusModel { IsConnected = false, TerminalId = string.IsNullOrWhiteSpace(terminalId) ? "default" : terminalId, CurrentSaleStatus = "Ready" });
    }

    public Task<IReadOnlyCollection<DashboardNotificationModel>> GetCashierNotificationsAsync(int cashierId, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<DashboardNotificationModel> notifications = new[]
        {
            new DashboardNotificationModel { Message = "Ready for new sale.", Severity = "success" },
            new DashboardNotificationModel { Message = "Customer display opens from checkout or dashboard.", Severity = "info" }
        };
        return Task.FromResult(notifications);
    }
}
