using SalesEngine.Models;

namespace AphiwatPOS.Services;

public interface IReceiptPrinterService
{
    Task PrintReceiptAsync(long salesHeaderId, int printedByUserId, CancellationToken cancellationToken = default);
    Task PrintRawBytesAsync(byte[] bytes, CancellationToken cancellationToken = default);
    Task<bool> CheckPrinterAvailableAsync(CancellationToken cancellationToken = default);
}

public interface ICashDrawerService
{
    Task<CashDrawerOpenResult> OpenDrawerAsync(string openType, string reason, int openedByUserId, long? sessionId = null, long? saleId = null, CancellationToken cancellationToken = default);
    Task<CashDrawerOpenResult> OpenDrawerForSaleAsync(SalesCompleteResultModel sale, IReadOnlyCollection<SalesPaymentInputModel> payments, IReadOnlyCollection<PaymentMethodModel> paymentMethods, int openedByUserId, CancellationToken cancellationToken = default);
    Task<CashDrawerOpenResult> OpenDrawerManuallyAsync(string reason, int openedByUserId, CancellationToken cancellationToken = default);
    Task<CashDrawerOpenResult> TestOpenDrawerAsync(int openedByUserId, CancellationToken cancellationToken = default);
    Task<CashDrawerStatusModel> GetStatusAsync(int cashierUserId, CancellationToken cancellationToken = default);
    Task<CashDrawerSessionModel?> GetActiveSessionAsync(int cashierUserId, CancellationToken cancellationToken = default);
    Task<CashDrawerSessionModel> OpenShiftAsync(decimal startingCash, int cashierUserId, int openedByUserId, CancellationToken cancellationToken = default);
    Task<CashDrawerShiftCloseResult> CloseShiftAsync(decimal actualCash, string? note, int cashierUserId, int closedByUserId, CancellationToken cancellationToken = default);
    Task RecordCashInAsync(decimal amount, string reason, int cashierUserId, int createdByUserId, CancellationToken cancellationToken = default);
    Task RecordCashOutAsync(decimal amount, string reason, int cashierUserId, int createdByUserId, int? approvedByUserId = null, CancellationToken cancellationToken = default);
}
