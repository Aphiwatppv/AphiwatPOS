using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Options;
using SalesEngine.Models;
using SalesEngine.Services;

namespace AphiwatPOS.Services;

public sealed class ReceiptPrinterService : IReceiptPrinterService
{
    private const int ReceiptColumns = 32;
    private readonly IOptionsMonitor<ReceiptPrinterOptions> _options;
    private readonly ISalesHistoryService _salesHistoryService;
    private readonly IReceiptService _receiptService;
    private readonly ILogger<ReceiptPrinterService> _logger;

    public ReceiptPrinterService(IOptionsMonitor<ReceiptPrinterOptions> options, ISalesHistoryService salesHistoryService, IReceiptService receiptService, ILogger<ReceiptPrinterService> logger)
    {
        _options = options;
        _salesHistoryService = salesHistoryService;
        _receiptService = receiptService;
        _logger = logger;
    }

    public async Task PrintReceiptAsync(long salesHeaderId, int printedByUserId, CancellationToken cancellationToken = default)
    {
        var sale = await _salesHistoryService.GetDetailAsync(salesHeaderId, cancellationToken)
            ?? throw new InvalidOperationException("Sale was saved, but receipt data could not be loaded.");

        var bytes = Encoding.ASCII.GetBytes(BuildReceiptText(sale));
        await PrintRawBytesAsync(bytes, cancellationToken);
        await _receiptService.CreatePrintHistoryAsync(new()
        {
            SalesHeaderId = salesHeaderId,
            ReceiptNo = sale.SaleNo,
            ReceiptType = "Sale",
            PrintedByUserId = printedByUserId
        }, cancellationToken);
    }

    public Task PrintRawBytesAsync(byte[] bytes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        if (bytes.Length == 0) throw new ArgumentException("Print bytes cannot be empty.", nameof(bytes));
        var options = _options.CurrentValue;
        if (string.IsNullOrWhiteSpace(options.PrinterName)) throw new InvalidOperationException("Receipt printer name is not configured.");

        cancellationToken.ThrowIfCancellationRequested();
        RawPrinterHelper.SendBytesToPrinter(options.PrinterName, bytes);
        return Task.CompletedTask;
    }

    public Task<bool> CheckPrinterAvailableAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var options = _options.CurrentValue;
        var available = !string.IsNullOrWhiteSpace(options.PrinterName) &&
                        RawPrinterHelper.IsPrinterAvailable(options.PrinterName);
        return Task.FromResult(available);
    }

    private static string BuildReceiptText(SalesEngine.Models.SalesDetailModel sale)
    {
        var builder = new StringBuilder();
        builder.Append('\x1B').Append('@');
        builder.AppendLine("AphiwatPOS");
        AppendWrapped(builder, $"Receipt: {sale.SaleNo}");
        AppendWrapped(builder, $"Date: {sale.SaleDate:yyyy-MM-dd HH:mm}");
        AppendWrapped(builder, $"Cashier: {ToPrintable(sale.CashierName, "Cashier")}");
        builder.AppendLine(new string('-', ReceiptColumns));

        foreach (var item in sale.Items)
        {
            var productName = PrintableProductName(item);
            AppendWrapped(builder, productName);
            AppendAmountLine(builder, $"{item.Quantity:N2} x {item.UnitPrice:N2}", item.LineTotal);
        }

        builder.AppendLine(new string('-', ReceiptColumns));
        AppendAmountLine(builder, "Subtotal:", sale.SubtotalAmount);
        AppendAmountLine(builder, "Discount:", sale.TotalDiscountAmount);
        AppendAmountLine(builder, "VAT:", sale.TaxAmount);
        AppendAmountLine(builder, "Total:", sale.NetAmount);
        AppendAmountLine(builder, "Paid:", sale.PaidAmount);
        AppendAmountLine(builder, "Change:", sale.ChangeAmount);
        builder.AppendLine();
        builder.AppendLine("Thank you.");
        builder.AppendLine();
        builder.AppendLine();
        builder.AppendLine();
        builder.Append('\x1D').Append('V').Append('\x00');
        return builder.ToString();
    }

    private static void AppendAmountLine(StringBuilder builder, string label, decimal amount)
    {
        var safeLabel = ToPrintable(label, "Item");
        var value = amount.ToString("N2");
        var spaces = Math.Max(1, ReceiptColumns - safeLabel.Length - value.Length);
        builder.Append(safeLabel);
        builder.Append(' ', spaces);
        builder.AppendLine(value);
    }

    private static void AppendWrapped(StringBuilder builder, string text)
    {
        var safeText = ToPrintable(text, string.Empty);
        if (string.IsNullOrWhiteSpace(safeText))
        {
            return;
        }

        for (var index = 0; index < safeText.Length; index += ReceiptColumns)
        {
            var length = Math.Min(ReceiptColumns, safeText.Length - index);
            builder.AppendLine(safeText.Substring(index, length));
        }
    }

    private static string PrintableProductName(SalesItemModel item)
    {
        if (IsPrintableAscii(item.ProductNameSnapshot))
        {
            return item.ProductNameSnapshot.Trim();
        }

        if (!string.IsNullOrWhiteSpace(item.ProductCodeSnapshot))
        {
            return item.ProductCodeSnapshot.Trim();
        }

        if (!string.IsNullOrWhiteSpace(item.BarcodeSnapshot))
        {
            return item.BarcodeSnapshot.Trim();
        }

        return $"Product {item.ProductId}";
    }

    private static string ToPrintable(string? text, string fallback)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return fallback;
        }

        var builder = new StringBuilder(text.Length);
        foreach (var ch in text.Trim())
        {
            builder.Append(ch is >= ' ' and <= '~' ? ch : ' ');
        }

        var value = builder.ToString().Trim();
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static bool IsPrintableAscii(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return text.Trim().All(ch => ch is >= ' ' and <= '~');
    }

    private static class RawPrinterHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private sealed class DocInfo
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string DocumentName = "AphiwatPOS ESC/POS";
            [MarshalAs(UnmanagedType.LPWStr)] public string? OutputFile;
            [MarshalAs(UnmanagedType.LPWStr)] public string DataType = "RAW";
        }

        [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool OpenPrinter(string printerName, out IntPtr printerHandle, IntPtr defaultPrinter);

        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool ClosePrinter(IntPtr printerHandle);

        [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool StartDocPrinter(IntPtr printerHandle, int level, [In] DocInfo docInfo);

        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool EndDocPrinter(IntPtr printerHandle);

        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool StartPagePrinter(IntPtr printerHandle);

        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool EndPagePrinter(IntPtr printerHandle);

        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool WritePrinter(IntPtr printerHandle, byte[] bytes, int count, out int written);

        public static bool IsPrinterAvailable(string printerName)
        {
            if (!OperatingSystem.IsWindows()) return false;
            if (!OpenPrinter(printerName, out var handle, IntPtr.Zero)) return false;
            ClosePrinter(handle);
            return true;
        }

        public static void SendBytesToPrinter(string printerName, byte[] bytes)
        {
            if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Raw ESC/POS printing is supported on Windows print queues.");

            if (!OpenPrinter(printerName, out var handle, IntPtr.Zero))
                ThrowPrinterError($"Printer '{printerName}' was not found or could not be opened.");

            try
            {
                var docInfo = new DocInfo();
                if (!StartDocPrinter(handle, 1, docInfo)) ThrowPrinterError("Could not start the print document.");
                try
                {
                    if (!StartPagePrinter(handle)) ThrowPrinterError("Could not start the print page.");
                    try
                    {
                        if (!WritePrinter(handle, bytes, bytes.Length, out var written) || written != bytes.Length)
                            ThrowPrinterError("The printer did not accept the complete ESC/POS command.");
                    }
                    finally
                    {
                        EndPagePrinter(handle);
                    }
                }
                finally
                {
                    EndDocPrinter(handle);
                }
            }
            finally
            {
                ClosePrinter(handle);
            }
        }

        private static void ThrowPrinterError(string message)
        {
            var error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"{message} Win32 error: {error}.");
        }
    }
}
