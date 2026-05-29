namespace AphiwatPOS.Services;

public sealed class ReceiptPrinterOptions
{
    public string PrinterName { get; set; } = "XP-80C";
    public bool CashDrawerEnabled { get; set; } = true;
    public string DrawerKickCommand { get; set; } = "27,112,0,25,250";
    public int DrawerPin { get; set; } = 2;
    public bool OpenDrawerAfterReceiptPrint { get; set; } = true;
    public bool AllowManualOpenDrawer { get; set; } = true;
}

public sealed class CashDrawerSessionModel
{
    public long SessionId { get; init; }
    public int CashierUserId { get; init; }
    public string CashierName { get; init; } = string.Empty;
    public decimal StartingCash { get; init; }
    public decimal CashSales { get; init; }
    public decimal CashIn { get; init; }
    public decimal CashOut { get; init; }
    public decimal CashRefund { get; init; }
    public decimal ExpectedCash { get; init; }
    public decimal? ActualCash { get; init; }
    public decimal? Difference { get; init; }
    public int OpenedByUserId { get; init; }
    public int? ClosedByUserId { get; init; }
    public int? ApprovedByUserId { get; init; }
    public DateTime OpenedDate { get; init; }
    public DateTime? ClosedDate { get; init; }
    public string Status { get; init; } = string.Empty;
}

public sealed class CashDrawerOpenResult
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public long? SessionId { get; init; }
    public long? OpenLogId { get; init; }
    public string? ErrorMessage { get; init; }

    public static CashDrawerOpenResult Success(string message, long? sessionId = null, long? openLogId = null) =>
        new() { IsSuccess = true, Message = message, SessionId = sessionId, OpenLogId = openLogId };

    public static CashDrawerOpenResult Fail(string message, string? errorMessage = null, long? sessionId = null, long? openLogId = null) =>
        new() { IsSuccess = false, Message = message, ErrorMessage = errorMessage, SessionId = sessionId, OpenLogId = openLogId };
}

public sealed class CashDrawerShiftCloseResult
{
    public CashDrawerSessionModel Session { get; init; } = new();
    public bool RequiresManagerReview => Session.Difference.HasValue && Session.Difference.Value != 0;
}

public sealed class CashDrawerStatusModel
{
    public bool HasActiveShift => Session is not null && Session.Status.Equals("Open", StringComparison.OrdinalIgnoreCase);
    public CashDrawerSessionModel? Session { get; init; }
    public bool CashDrawerEnabled { get; init; }
    public bool AllowManualOpenDrawer { get; init; }
    public string PrinterName { get; init; } = string.Empty;
}
