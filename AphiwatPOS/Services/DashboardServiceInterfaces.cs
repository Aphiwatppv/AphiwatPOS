namespace AphiwatPOS.Services;

public interface IManagerDashboardService
{
    Task<SalesOverviewModel> GetSalesOverviewAsync(CancellationToken cancellationToken = default);
    Task<ProfitSummaryModel> GetProfitSummaryAsync(CancellationToken cancellationToken = default);
    Task<InventoryOverviewModel> GetInventoryOverviewAsync(CancellationToken cancellationToken = default);
    Task<CustomerOverviewModel> GetCustomerOverviewAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CashierPerformanceModel>> GetCashierPerformanceAsync(CancellationToken cancellationToken = default);
    Task<PaymentSummaryModel> GetPaymentSummaryAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DashboardAlertModel>> GetDashboardAlertsAsync(CancellationToken cancellationToken = default);
    Task<ManagerDashboardModel> GetDashboardAsync(CancellationToken cancellationToken = default);
}

public interface ICashierDashboardService
{
    Task<TodayShiftSummaryModel> GetTodayShiftSummaryAsync(int cashierId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SalesEngine.Models.SalesHeaderModel>> GetRecentSalesAsync(int cashierId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SalesEngine.Models.HeldSaleHeaderModel>> GetHeldSalesAsync(int cashierId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PendingTaskModel>> GetPendingTasksAsync(int cashierId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CustomerCreditTodayModel>> GetCustomerCreditTodayAsync(int cashierId, CancellationToken cancellationToken = default);
    Task<CustomerDisplayStatusModel> GetCustomerDisplayStatusAsync(string? terminalId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DashboardNotificationModel>> GetCashierNotificationsAsync(int cashierId, CancellationToken cancellationToken = default);
    Task<CashierDashboardModel> GetDashboardAsync(int cashierId, string cashierName, string? terminalId, CancellationToken cancellationToken = default);
}
