using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Microsoft.Extensions.Options;
using SalesEngine.Models;
using SalesEngine.Services;

namespace AphiwatPOS.Services;

public sealed class ReceiptPrinterService : IReceiptPrinterService
{
    private const int ReceiptColumns = 32;
    private const int ReceiptImagePadding = 16;
    private const int DefaultReceiptImageWidth = 384;
    private const string DefaultReceiptEncodingName = "windows-874";
    private readonly IOptionsMonitor<ReceiptPrinterOptions> _options;
    private readonly ISalesHistoryService _salesHistoryService;
    private readonly IReceiptService _receiptService;
    private readonly ILogger<ReceiptPrinterService> _logger;

    static ReceiptPrinterService()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

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

        var bytes = BuildReceiptBytes(sale, _options.CurrentValue);
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

    private static byte[] BuildReceiptBytes(SalesEngine.Models.SalesDetailModel sale, ReceiptPrinterOptions options)
    {
        if (options.RenderTextAsImage)
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            {
                throw new PlatformNotSupportedException("Image receipt printing uses Windows font rendering.");
            }

            return BuildReceiptImageBytes(sale, options);
        }

        var text = BuildReceiptText(sale, options);
        return GetReceiptEncoding(options).GetBytes(text);
    }

    [SupportedOSPlatform("windows6.1")]
    private static byte[] BuildReceiptImageBytes(SalesEngine.Models.SalesDetailModel sale, ReceiptPrinterOptions options)
    {
        var rows = BuildReceiptRows(sale);
        var imageWidth = options.ReceiptImageWidth > 0 ? options.ReceiptImageWidth : DefaultReceiptImageWidth;
        using var font = CreateReceiptFont(options, FontStyle.Regular);
        using var boldFont = CreateReceiptFont(options, FontStyle.Bold);
        var lineHeight = (int)Math.Ceiling(font.GetHeight()) + 8;
        var imageHeight = Math.Max(1, ReceiptImagePadding * 2 + rows.Count * lineHeight + lineHeight * 4);

        using var image = new Bitmap(imageWidth, imageHeight, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(image))
        {
            graphics.Clear(Color.White);
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

            using var brush = new SolidBrush(Color.Black);
            var y = ReceiptImagePadding;
            foreach (var row in rows)
            {
                var rowFont = row.IsEmphasized ? boldFont : font;
                if (!string.IsNullOrEmpty(row.RightText))
                {
                    graphics.DrawString(row.LeftText, rowFont, brush, ReceiptImagePadding, y);
                    var rightSize = graphics.MeasureString(row.RightText, rowFont);
                    graphics.DrawString(row.RightText, rowFont, brush, imageWidth - ReceiptImagePadding - rightSize.Width, y);
                }
                else if (row.IsCentered)
                {
                    var textSize = graphics.MeasureString(row.LeftText, rowFont);
                    graphics.DrawString(row.LeftText, rowFont, brush, Math.Max(ReceiptImagePadding, (imageWidth - textSize.Width) / 2), y);
                }
                else
                {
                    graphics.DrawString(row.LeftText, rowFont, brush, ReceiptImagePadding, y);
                }

                y += lineHeight;
            }
        }

        return BuildRasterReceiptBytes(image);
    }

    [SupportedOSPlatform("windows6.1")]
    private static Font CreateReceiptFont(ReceiptPrinterOptions options, FontStyle style)
    {
        var fontName = string.IsNullOrWhiteSpace(options.ReceiptFontName) ? "Tahoma" : options.ReceiptFontName.Trim();
        var fontSize = options.ReceiptFontSize > 0 ? options.ReceiptFontSize : 20;
        return new Font(fontName, fontSize, style, GraphicsUnit.Pixel);
    }

    [SupportedOSPlatform("windows6.1")]
    private static byte[] BuildRasterReceiptBytes(Bitmap image)
    {
        var widthBytes = (image.Width + 7) / 8;
        var raster = new byte[widthBytes * image.Height];

        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                var pixel = image.GetPixel(x, y);
                var luminance = (pixel.R * 299 + pixel.G * 587 + pixel.B * 114) / 1000;
                if (luminance >= 180)
                {
                    continue;
                }

                raster[y * widthBytes + x / 8] |= (byte)(0x80 >> (x % 8));
            }
        }

        using var stream = new MemoryStream();
        stream.WriteByte(0x1B);
        stream.WriteByte(0x40);
        stream.WriteByte(0x1D);
        stream.WriteByte(0x76);
        stream.WriteByte(0x30);
        stream.WriteByte(0x00);
        stream.WriteByte((byte)(widthBytes & 0xFF));
        stream.WriteByte((byte)((widthBytes >> 8) & 0xFF));
        stream.WriteByte((byte)(image.Height & 0xFF));
        stream.WriteByte((byte)((image.Height >> 8) & 0xFF));
        stream.Write(raster, 0, raster.Length);
        stream.WriteByte(0x0A);
        stream.WriteByte(0x0A);
        stream.WriteByte(0x0A);
        stream.WriteByte(0x1D);
        stream.WriteByte(0x56);
        stream.WriteByte(0x00);
        return stream.ToArray();
    }

    private static Encoding GetReceiptEncoding(ReceiptPrinterOptions options)
    {
        var encodingName = string.IsNullOrWhiteSpace(options.TextEncodingName)
            ? DefaultReceiptEncodingName
            : options.TextEncodingName.Trim();

        return Encoding.GetEncoding(
            encodingName,
            EncoderFallback.ReplacementFallback,
            DecoderFallback.ReplacementFallback);
    }

    private static string BuildReceiptText(SalesEngine.Models.SalesDetailModel sale, ReceiptPrinterOptions options)
    {
        var builder = new StringBuilder();
        builder.Append('\x1B').Append('@');
        if (options.CharacterCodeTable >= 0)
        {
            builder.Append('\x1B').Append('t').Append((char)options.CharacterCodeTable);
        }

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

    private static IReadOnlyList<ReceiptRow> BuildReceiptRows(SalesEngine.Models.SalesDetailModel sale)
    {
        var rows = new List<ReceiptRow>
        {
            ReceiptRow.Center("AphiwatPOS", true),
            ReceiptRow.Text($"Receipt: {sale.SaleNo}"),
            ReceiptRow.Text($"Date: {sale.SaleDate:yyyy-MM-dd HH:mm}"),
            ReceiptRow.Text($"Cashier: {ToPrintable(sale.CashierName, "Cashier")}"),
            ReceiptRow.Text(new string('-', ReceiptColumns))
        };

        foreach (var item in sale.Items)
        {
            rows.AddRange(WrapRows(PrintableProductName(item)));
            rows.Add(ReceiptRow.Amount($"{item.Quantity:N2} x {item.UnitPrice:N2}", item.LineTotal));
        }

        rows.Add(ReceiptRow.Text(new string('-', ReceiptColumns)));
        rows.Add(ReceiptRow.Amount("Subtotal:", sale.SubtotalAmount));
        rows.Add(ReceiptRow.Amount("Discount:", sale.TotalDiscountAmount));
        rows.Add(ReceiptRow.Amount("VAT:", sale.TaxAmount));
        rows.Add(ReceiptRow.Amount("Total:", sale.NetAmount, true));
        rows.Add(ReceiptRow.Amount("Paid:", sale.PaidAmount));
        rows.Add(ReceiptRow.Amount("Change:", sale.ChangeAmount));
        rows.Add(ReceiptRow.Text(string.Empty));
        rows.Add(ReceiptRow.Center("Thank you."));
        return rows;
    }

    private static IEnumerable<ReceiptRow> WrapRows(string text)
    {
        var safeText = ToPrintable(text, string.Empty);
        if (string.IsNullOrWhiteSpace(safeText))
        {
            yield break;
        }

        for (var index = 0; index < safeText.Length; index += ReceiptColumns)
        {
            var length = Math.Min(ReceiptColumns, safeText.Length - index);
            yield return ReceiptRow.Text(safeText.Substring(index, length));
        }
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
        if (!string.IsNullOrWhiteSpace(item.ProductNameSnapshot))
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
            builder.Append(char.IsControl(ch) ? ' ' : ch);
        }

        var value = builder.ToString().Trim();
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private sealed record ReceiptRow(string LeftText, string? RightText = null, bool IsCentered = false, bool IsEmphasized = false)
    {
        public static ReceiptRow Text(string text) => new(text);
        public static ReceiptRow Center(string text, bool isEmphasized = false) => new(text, IsCentered: true, IsEmphasized: isEmphasized);
        public static ReceiptRow Amount(string label, decimal amount, bool isEmphasized = false) => new(label, amount.ToString("N2"), IsEmphasized: isEmphasized);
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
