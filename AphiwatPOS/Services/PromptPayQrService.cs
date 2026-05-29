using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;
using QRCoder;

namespace AphiwatPOS.Services;

public sealed class PromptPayOptions
{
    public string PayeeId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = "PromptPay";
}

public sealed record PromptPayQrResult(
    string Payload,
    string Svg,
    string SvgDataUri,
    decimal Amount,
    string PayeeDisplayName);

public interface IPromptPayQrService
{
    PromptPayQrResult Generate(decimal amount);
}

public sealed class PromptPayQrService : IPromptPayQrService
{
    private const string PromptPayApplicationId = "A000000677010111";
    private readonly IOptionsMonitor<PromptPayOptions> _options;

    public PromptPayQrService(IOptionsMonitor<PromptPayOptions> options)
    {
        _options = options;
    }

    public PromptPayQrResult Generate(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("PromptPay amount must be greater than zero.", nameof(amount));
        if (amount > 9999999999999.99m) throw new ArgumentException("PromptPay amount is too large.", nameof(amount));

        var options = _options.CurrentValue;
        var target = NormalizeTarget(options.PayeeId);
        if (string.IsNullOrWhiteSpace(target.Value))
            throw new InvalidOperationException("PromptPay payee ID is not configured.");

        var payloadWithoutCrc =
            Field("00", "01") +
            Field("01", "12") +
            Field("29", Field("00", PromptPayApplicationId) + Field(target.Tag, target.Value)) +
            Field("53", "764") +
            Field("54", amount.ToString("0.00", CultureInfo.InvariantCulture)) +
            Field("58", "TH") +
            "6304";

        var payload = payloadWithoutCrc + ComputeCrc16(payloadWithoutCrc);
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
        var qrCode = new SvgQRCode(data);
        var svg = qrCode.GetGraphic(4);
        var dataUri = "data:image/svg+xml;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));

        return new PromptPayQrResult(payload, svg, dataUri, decimal.Round(amount, 2), options.DisplayName);
    }

    private static (string Tag, string Value) NormalizeTarget(string value)
    {
        var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length == 10 && digits.StartsWith('0'))
            return ("01", "0066" + digits[1..]);

        if (digits.Length == 13)
            return ("02", digits);

        if (digits.Length == 15)
            return ("03", digits);

        return (string.Empty, string.Empty);
    }

    private static string Field(string id, string value)
    {
        return id + value.Length.ToString("00", CultureInfo.InvariantCulture) + value;
    }

    private static string ComputeCrc16(string value)
    {
        const ushort polynomial = 0x1021;
        ushort crc = 0xFFFF;
        foreach (var b in Encoding.ASCII.GetBytes(value))
        {
            crc ^= (ushort)(b << 8);
            for (var i = 0; i < 8; i++)
            {
                crc = (crc & 0x8000) != 0
                    ? (ushort)((crc << 1) ^ polynomial)
                    : (ushort)(crc << 1);
            }
        }

        return crc.ToString("X4", CultureInfo.InvariantCulture);
    }
}
