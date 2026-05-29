using AccessEngine.Services;
using Microsoft.Extensions.Options;
using SalesEngine.Models;
using System.Collections.Concurrent;

namespace AphiwatPOS.Services;

public sealed class CashDrawerService : ICashDrawerService
{
    private static readonly ConcurrentDictionary<int, DateTimeOffset> RecentOpenRequests = new();
    private readonly IAccessService _accessService;
    private readonly IReceiptPrinterService _printerService;
    private readonly IOptionsMonitor<ReceiptPrinterOptions> _options;
    private readonly ILogger<CashDrawerService> _logger;

    public CashDrawerService(IAccessService accessService, IReceiptPrinterService printerService, IOptionsMonitor<ReceiptPrinterOptions> options, ILogger<CashDrawerService> logger)
    {
        _accessService = accessService;
        _printerService = printerService;
        _options = options;
        _logger = logger;
    }

    public async Task<CashDrawerOpenResult> OpenDrawerAsync(string openType, string reason, int openedByUserId, long? sessionId = null, long? saleId = null, CancellationToken cancellationToken = default)
    {
        if (!_options.CurrentValue.CashDrawerEnabled) return CashDrawerOpenResult.Fail("Cash drawer is disabled.");
        if (openedByUserId <= 0) throw new ArgumentException("Opened by user is required.", nameof(openedByUserId));
        if (string.IsNullOrWhiteSpace(openType)) throw new ArgumentException("Open type is required.", nameof(openType));
        if (IsDuplicateRequest(openedByUserId))
        {
            var duplicateLogId = await CreateOpenLogAsync(sessionId, saleId, openType, reason, false, "Duplicate drawer open request.", openedByUserId, cancellationToken);
            return CashDrawerOpenResult.Fail("Duplicate drawer open request ignored.", "Duplicate drawer open request.", sessionId, duplicateLogId);
        }

        var logId = 0L;
        try
        {
            var command = BuildDrawerKickCommand();
            await _printerService.PrintRawBytesAsync(command, cancellationToken);
            logId = await CreateOpenLogAsync(sessionId, saleId, openType, reason, true, null, openedByUserId, cancellationToken);
            return CashDrawerOpenResult.Success("Cash drawer opened successfully.", sessionId, logId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cash drawer open failed. Type: {OpenType}, SaleId: {SaleId}", openType, saleId);
            try
            {
                logId = await CreateOpenLogAsync(sessionId, saleId, openType, reason, false, ex.Message, openedByUserId, cancellationToken);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Could not save cash drawer failure log.");
            }

            return CashDrawerOpenResult.Fail("Cash drawer could not be opened. Please check printer connection.", ex.Message, sessionId, logId == 0 ? null : logId);
        }
    }

    public async Task<CashDrawerOpenResult> OpenDrawerForSaleAsync(SalesCompleteResultModel sale, IReadOnlyCollection<SalesPaymentInputModel> payments, IReadOnlyCollection<PaymentMethodModel> paymentMethods, int openedByUserId, CancellationToken cancellationToken = default)
    {
        var cashPaymentIds = paymentMethods.Where(method => method.IsCash).Select(method => method.PaymentMethodId).ToHashSet();
        var cashAmount = payments.Where(payment => cashPaymentIds.Contains(payment.PaymentMethodId)).Sum(payment => payment.PaymentAmount);
        if (cashAmount <= 0) return CashDrawerOpenResult.Success("Cash drawer was not opened because payment did not include cash.");

        var session = await GetActiveSessionAsync(openedByUserId, cancellationToken);
        if (session is null)
        {
            var logId = await CreateOpenLogAsync(null, sale.SalesHeaderId, "CashSale", "Cash sale drawer open blocked because no active shift exists.", false, "No active cash drawer session.", openedByUserId, cancellationToken);
            return CashDrawerOpenResult.Fail("No active cash drawer session.", "No active cash drawer session.", null, logId);
        }

        var retainedCash = Math.Max(0, cashAmount - Math.Min(cashAmount, sale.ChangeAmount));
        await _accessService.ExecuteAsync("dbo.spCashDrawerTransactionCreate", new
        {
            session.SessionId,
            TransactionType = "CashSale",
            Amount = retainedCash,
            Reason = "Cash sale",
            ReferenceNo = sale.SaleNo,
            SaleId = sale.SalesHeaderId > 0 ? sale.SalesHeaderId : (long?)null,
            CreatedByUserId = openedByUserId
        }, cancellationToken);

        return await OpenDrawerAsync("CashSale", $"Cash sale {sale.SaleNo}", openedByUserId, session.SessionId, sale.SalesHeaderId > 0 ? sale.SalesHeaderId : (long?)null, cancellationToken);
    }

    public async Task<CashDrawerOpenResult> OpenDrawerManuallyAsync(string reason, int openedByUserId, CancellationToken cancellationToken = default)
    {
        if (!_options.CurrentValue.AllowManualOpenDrawer) return CashDrawerOpenResult.Fail("Manual cash drawer opening is disabled.");
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Reason is required.", nameof(reason));

        var session = await GetActiveSessionAsync(openedByUserId, cancellationToken);
        if (session is null) return CashDrawerOpenResult.Fail("No active cash drawer session.", "No active cash drawer session.");

        return await OpenDrawerAsync("Manual", reason.Trim(), openedByUserId, session.SessionId, null, cancellationToken);
    }

    public Task<CashDrawerOpenResult> TestOpenDrawerAsync(int openedByUserId, CancellationToken cancellationToken = default) =>
        OpenDrawerAsync("Test", "Admin or manager drawer test", openedByUserId, null, null, cancellationToken);

    public async Task<CashDrawerStatusModel> GetStatusAsync(int cashierUserId, CancellationToken cancellationToken = default)
    {
        var session = await GetActiveSessionAsync(cashierUserId, cancellationToken);
        return new CashDrawerStatusModel
        {
            Session = session,
            CashDrawerEnabled = _options.CurrentValue.CashDrawerEnabled,
            AllowManualOpenDrawer = _options.CurrentValue.AllowManualOpenDrawer,
            PrinterName = _options.CurrentValue.PrinterName
        };
    }

    public Task<CashDrawerSessionModel?> GetActiveSessionAsync(int cashierUserId, CancellationToken cancellationToken = default) =>
        _accessService.QuerySingleOrDefaultAsync<CashDrawerSessionModel, object>("dbo.spCashDrawerSessionGetActive", new { CashierUserId = cashierUserId }, cancellationToken);

    public Task<CashDrawerSessionModel> OpenShiftAsync(decimal startingCash, int cashierUserId, int openedByUserId, CancellationToken cancellationToken = default)
    {
        if (startingCash < 0) throw new ArgumentException("Starting cash cannot be negative.", nameof(startingCash));
        return _accessService.QuerySingleAsync<CashDrawerSessionModel, object>("dbo.spCashDrawerSessionOpen", new { CashierUserId = cashierUserId, StartingCash = startingCash, OpenedByUserId = openedByUserId }, cancellationToken);
    }

    public async Task<CashDrawerShiftCloseResult> CloseShiftAsync(decimal actualCash, string? note, int cashierUserId, int closedByUserId, CancellationToken cancellationToken = default)
    {
        if (actualCash < 0) throw new ArgumentException("Actual cash cannot be negative.", nameof(actualCash));
        var session = await _accessService.QuerySingleAsync<CashDrawerSessionModel, object>("dbo.spCashDrawerSessionClose", new { CashierUserId = cashierUserId, ActualCash = actualCash, Note = note ?? string.Empty, ClosedByUserId = closedByUserId }, cancellationToken);
        return new CashDrawerShiftCloseResult { Session = session };
    }

    public async Task RecordCashInAsync(decimal amount, string reason, int cashierUserId, int createdByUserId, CancellationToken cancellationToken = default)
    {
        if (amount <= 0) throw new ArgumentException("Cash in amount must be greater than zero.", nameof(amount));
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Reason is required.", nameof(reason));
        var session = await RequireActiveSessionAsync(cashierUserId, cancellationToken);
        await _accessService.ExecuteAsync("dbo.spCashDrawerTransactionCreate", new { session.SessionId, TransactionType = "CashIn", Amount = amount, Reason = reason.Trim(), ReferenceNo = string.Empty, SaleId = (long?)null, CreatedByUserId = createdByUserId }, cancellationToken);
    }

    public async Task RecordCashOutAsync(decimal amount, string reason, int cashierUserId, int createdByUserId, int? approvedByUserId = null, CancellationToken cancellationToken = default)
    {
        if (amount <= 0) throw new ArgumentException("Cash out amount must be greater than zero.", nameof(amount));
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Reason is required.", nameof(reason));
        var session = await RequireActiveSessionAsync(cashierUserId, cancellationToken);
        await _accessService.ExecuteAsync("dbo.spCashDrawerTransactionCreate", new { session.SessionId, TransactionType = "CashOut", Amount = amount, Reason = reason.Trim(), ReferenceNo = approvedByUserId.HasValue ? $"Approved by {approvedByUserId}" : string.Empty, SaleId = (long?)null, CreatedByUserId = createdByUserId }, cancellationToken);
    }

    private async Task<CashDrawerSessionModel> RequireActiveSessionAsync(int cashierUserId, CancellationToken cancellationToken)
    {
        var session = await GetActiveSessionAsync(cashierUserId, cancellationToken);
        return session ?? throw new InvalidOperationException("No active cash drawer session.");
    }

    private byte[] BuildDrawerKickCommand()
    {
        var options = _options.CurrentValue;
        if (!string.IsNullOrWhiteSpace(options.DrawerKickCommand))
        {
            var bytes = options.DrawerKickCommand.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(value => byte.Parse(value))
                .ToArray();
            if (bytes.Length > 0)
            {
                if (bytes.Length >= 3 && bytes[0] == 27 && bytes[1] == 112)
                {
                    bytes[2] = options.DrawerPin == 5 ? (byte)1 : (byte)0;
                }

                return bytes;
            }
        }

        var pin = options.DrawerPin == 5 ? (byte)1 : (byte)0;
        return new byte[] { 27, 112, pin, 25, 250 };
    }

    private Task<long> CreateOpenLogAsync(long? sessionId, long? saleId, string openType, string reason, bool isSuccess, string? errorMessage, int openedByUserId, CancellationToken cancellationToken) =>
        _accessService.QuerySingleAsync<long, object>("dbo.spCashDrawerOpenLogCreate", new
        {
            SessionId = sessionId,
            SaleId = saleId,
            OpenType = openType,
            Reason = reason ?? string.Empty,
            IsSuccess = isSuccess,
            ErrorMessage = errorMessage ?? string.Empty,
            OpenedByUserId = openedByUserId
        }, cancellationToken);

    private static bool IsDuplicateRequest(int openedByUserId)
    {
        var now = DateTimeOffset.UtcNow;
        if (RecentOpenRequests.TryGetValue(openedByUserId, out var previous) && now - previous < TimeSpan.FromSeconds(2))
        {
            RecentOpenRequests[openedByUserId] = now;
            return true;
        }

        RecentOpenRequests[openedByUserId] = now;
        return false;
    }
}
