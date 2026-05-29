using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using AphiwatPOS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using SalesEngine.Models;
using SalesEngine.Services;

namespace AphiwatPOS.Pages.Settings;

public sealed class IndexModel : PageModel
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly IOptionsMonitor<PromptPayOptions> _promptPayOptions;
    private readonly IOptionsMonitor<ReceiptPrinterOptions> _receiptPrinterOptions;
    private readonly IPaymentMethodService _paymentMethodService;
    private readonly IReceiptPrinterService _receiptPrinterService;

    public IndexModel(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        IOptionsMonitor<PromptPayOptions> promptPayOptions,
        IOptionsMonitor<ReceiptPrinterOptions> receiptPrinterOptions,
        IPaymentMethodService paymentMethodService,
        IReceiptPrinterService receiptPrinterService)
    {
        _configuration = configuration;
        _environment = environment;
        _promptPayOptions = promptPayOptions;
        _receiptPrinterOptions = receiptPrinterOptions;
        _paymentMethodService = paymentMethodService;
        _receiptPrinterService = receiptPrinterService;
    }

    [BindProperty] public SystemSettingsInput SystemInput { get; set; } = new();
    [BindProperty] public PaymentMethodInput PaymentInput { get; set; } = new();

    public IReadOnlyCollection<PaymentMethodModel> PaymentMethods { get; private set; } = Array.Empty<PaymentMethodModel>();
    public IReadOnlyCollection<SettingsGroup> SettingsGroups { get; private set; } = Array.Empty<SettingsGroup>();
    public string ConnectionSummary { get; private set; } = string.Empty;
    public bool IsAdmin => User.IsInRole("Admin");
    public bool IsPrinterAvailable { get; private set; }
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        StatusMessage = TempData["StatusMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        await LoadAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostSaveSystemSettingsAsync(CancellationToken cancellationToken)
    {
        if (!IsAdmin) return Forbid();

        try
        {
            ValidateSystemInput(SystemInput);
            await SaveSystemSettingsAsync(SystemInput, cancellationToken);
            TempData["StatusMessage"] = "System settings saved.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreatePaymentMethodAsync(CancellationToken cancellationToken)
    {
        if (!IsAdmin) return Forbid();

        try
        {
            ValidatePaymentInput(PaymentInput, requireId: false);
            if (await _paymentMethodService.IsCodeExistsAsync(PaymentInput.PaymentMethodCode, null, cancellationToken))
            {
                throw new InvalidOperationException("Payment method code already exists.");
            }

            await _paymentMethodService.CreateAsync(new PaymentMethodCreateModel
            {
                PaymentMethodCode = PaymentInput.PaymentMethodCode,
                PaymentMethodName = PaymentInput.PaymentMethodName,
                Description = PaymentInput.Description,
                RequireReferenceNo = PaymentInput.RequireReferenceNo,
                IsCash = PaymentInput.IsCash,
                DisplayOrder = PaymentInput.DisplayOrder,
                CreatedByUserId = CurrentUserId()
            }, cancellationToken);
            TempData["StatusMessage"] = "Payment method created.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdatePaymentMethodAsync(CancellationToken cancellationToken)
    {
        if (!IsAdmin) return Forbid();

        try
        {
            ValidatePaymentInput(PaymentInput, requireId: true);
            if (await _paymentMethodService.IsCodeExistsAsync(PaymentInput.PaymentMethodCode, PaymentInput.PaymentMethodId, cancellationToken))
            {
                throw new InvalidOperationException("Payment method code already exists.");
            }

            await _paymentMethodService.UpdateAsync(new PaymentMethodUpdateModel
            {
                PaymentMethodId = PaymentInput.PaymentMethodId,
                PaymentMethodCode = PaymentInput.PaymentMethodCode,
                PaymentMethodName = PaymentInput.PaymentMethodName,
                Description = PaymentInput.Description,
                RequireReferenceNo = PaymentInput.RequireReferenceNo,
                IsCash = PaymentInput.IsCash,
                IsActive = PaymentInput.IsActive,
                DisplayOrder = PaymentInput.DisplayOrder,
                UpdatedByUserId = CurrentUserId()
            }, cancellationToken);
            TempData["StatusMessage"] = "Payment method updated.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostTogglePaymentMethodAsync(int paymentMethodId, bool isActive, CancellationToken cancellationToken)
    {
        if (!IsAdmin) return Forbid();

        try
        {
            await _paymentMethodService.ToggleActiveAsync(paymentMethodId, isActive, CurrentUserId(), cancellationToken);
            TempData["StatusMessage"] = isActive ? "Payment method activated." : "Payment method deactivated.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        var promptPay = _promptPayOptions.CurrentValue;
        var receiptPrinter = _receiptPrinterOptions.CurrentValue;
        SystemInput = new SystemSettingsInput
        {
            PromptPayPayeeId = promptPay.PayeeId,
            PromptPayDisplayName = promptPay.DisplayName,
            ReceiptPrinterName = receiptPrinter.PrinterName,
            CashDrawerEnabled = receiptPrinter.CashDrawerEnabled,
            DrawerKickCommand = receiptPrinter.DrawerKickCommand,
            DrawerPin = receiptPrinter.DrawerPin,
            OpenDrawerAfterReceiptPrint = receiptPrinter.OpenDrawerAfterReceiptPrint,
            AllowManualOpenDrawer = receiptPrinter.AllowManualOpenDrawer
        };

        ConnectionSummary = MaskConnectionString(_configuration.GetConnectionString("DefaultConnection"));
        SettingsGroups = BuildSettingsGroups();

        try
        {
            PaymentMethods = (await _paymentMethodService.GetAllAsync(cancellationToken))
                .OrderBy(method => method.DisplayOrder)
                .ThenBy(method => method.PaymentMethodName)
                .ToArray();
        }
        catch
        {
            ErrorMessage ??= "Failed to load payment methods. Confirm the Sales Management SQL has been deployed.";
        }

        try
        {
            IsPrinterAvailable = await _receiptPrinterService.CheckPrinterAvailableAsync(cancellationToken);
        }
        catch
        {
            IsPrinterAvailable = false;
        }
    }

    private async Task SaveSystemSettingsAsync(SystemSettingsInput input, CancellationToken cancellationToken)
    {
        var path = Path.Combine(_environment.ContentRootPath, "appsettings.json");
        JsonObject root;
        await using (var readStream = System.IO.File.OpenRead(path))
        {
            root = await JsonNode.ParseAsync(readStream, cancellationToken: cancellationToken) as JsonObject ?? new JsonObject();
        }

        var promptPay = EnsureObject(root, "PromptPay");
        promptPay["PayeeId"] = input.PromptPayPayeeId.Trim();
        promptPay["DisplayName"] = input.PromptPayDisplayName.Trim();

        var receiptPrinter = EnsureObject(root, "ReceiptPrinter");
        receiptPrinter["PrinterName"] = input.ReceiptPrinterName.Trim();
        receiptPrinter["CashDrawerEnabled"] = input.CashDrawerEnabled;
        receiptPrinter["DrawerKickCommand"] = input.DrawerKickCommand.Trim();
        receiptPrinter["DrawerPin"] = input.DrawerPin;
        receiptPrinter["OpenDrawerAfterReceiptPrint"] = input.OpenDrawerAfterReceiptPrint;
        receiptPrinter["AllowManualOpenDrawer"] = input.AllowManualOpenDrawer;

        await System.IO.File.WriteAllTextAsync(path, root.ToJsonString(JsonOptions) + Environment.NewLine, cancellationToken);
    }

    private static JsonObject EnsureObject(JsonObject root, string key)
    {
        if (root[key] is JsonObject section)
        {
            return section;
        }

        section = new JsonObject();
        root[key] = section;
        return section;
    }

    private static void ValidateSystemInput(SystemSettingsInput input)
    {
        if (string.IsNullOrWhiteSpace(input.PromptPayPayeeId)) throw new InvalidOperationException("PromptPay payee ID is required.");
        var payeeDigits = new string(input.PromptPayPayeeId.Where(char.IsDigit).ToArray());
        if (payeeDigits.Length is not (10 or 13 or 15)) throw new InvalidOperationException("PromptPay payee ID must be a 10-digit phone number, 13-digit tax ID, or 15-digit e-wallet ID.");
        if (string.IsNullOrWhiteSpace(input.PromptPayDisplayName)) throw new InvalidOperationException("PromptPay display name is required.");
        if (string.IsNullOrWhiteSpace(input.ReceiptPrinterName)) throw new InvalidOperationException("Receipt printer name is required.");
        if (input.DrawerPin is not (2 or 5)) throw new InvalidOperationException("Drawer pin must be 2 or 5.");

        var commandParts = input.DrawerKickCommand.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (commandParts.Length == 0) throw new InvalidOperationException("Drawer kick command is required.");
        foreach (var part in commandParts)
        {
            if (!byte.TryParse(part, out _)) throw new InvalidOperationException("Drawer kick command must contain comma-separated byte values.");
        }
    }

    private static void ValidatePaymentInput(PaymentMethodInput input, bool requireId)
    {
        if (requireId && input.PaymentMethodId <= 0) throw new InvalidOperationException("Payment method ID is required.");
        if (string.IsNullOrWhiteSpace(input.PaymentMethodCode)) throw new InvalidOperationException("Payment method code is required.");
        if (string.IsNullOrWhiteSpace(input.PaymentMethodName)) throw new InvalidOperationException("Payment method name is required.");
        if (input.PaymentMethodCode.Length > 30) throw new InvalidOperationException("Payment method code must be 30 characters or less.");
        if (input.PaymentMethodName.Length > 100) throw new InvalidOperationException("Payment method name must be 100 characters or less.");
        if (input.DisplayOrder < 0) throw new InvalidOperationException("Display order cannot be negative.");
    }

    private IReadOnlyCollection<SettingsGroup> BuildSettingsGroups() =>
        new[]
        {
            new SettingsGroup("Store Setup", new[]
            {
                new SettingsLink("Products", "Items, price, tax, barcode, stock tracking", "bi-box-seam", "/Products/Product/Index"),
                new SettingsLink("Categories", "Product category setup", "bi-tags", "/Products/Category/Index"),
                new SettingsLink("Brands", "Brand list and logos", "bi-award", "/Products/Brand/Index"),
                new SettingsLink("Units", "Sale units and conversions", "bi-rulers", "/Products/Unit/Index")
            }),
            new SettingsGroup("Sales Setup", new[]
            {
                new SettingsLink("Payment Methods", "Cash, transfer, PromptPay, credit and reference rules", "bi-wallet2", "#payment-methods"),
                new SettingsLink("PromptPay QR", "Payee ID and QR display name", "bi-qr-code", "#system-settings"),
                new SettingsLink("Receipt Printer", "Printer queue and drawer command", "bi-printer", "#system-settings"),
                new SettingsLink("Daily Closing", "Cashier close and sales reconciliation", "bi-clipboard2-check", "/Sales/DailyClosing/Index")
            }),
            new SettingsGroup("Inventory Setup", new[]
            {
                new SettingsLink("Locations", "Warehouses, branches, and default stock location", "bi-geo-alt", "/Inventory/Location/Index"),
                new SettingsLink("Low Stock", "Review products below reorder level", "bi-exclamation-triangle", "/Inventory/LowStock/Index"),
                new SettingsLink("Stock Adjustment", "Adjustment reasons and approvals", "bi-sliders", "/Inventory/Adjustment/Index"),
                new SettingsLink("Stock Count", "Cycle count setup and approval", "bi-clipboard-check", "/Inventory/StockCount/Index")
            }),
            new SettingsGroup("Customer Setup", new[]
            {
                new SettingsLink("Member Levels", "Discount, point earn, credit rules", "bi-gem", "/Customer/MemberLevel/Index"),
                new SettingsLink("Loyalty Points", "Adjust and expire point balances", "bi-stars", "/Customer/LoyaltyPoints/Index"),
                new SettingsLink("Customer Credit", "Credit limits and terms", "bi-credit-card", "/Customers/Credit/Index"),
                new SettingsLink("Customers", "Profiles and contact data", "bi-person-lines-fill", "/Customer/CustomerList/Index")
            }),
            new SettingsGroup("Access Setup", new[]
            {
                new SettingsLink("Employees", "User accounts and profile setup", "bi-people", "/Employees/Employee/Index"),
                new SettingsLink("Roles", "Role groups and assignments", "bi-person-badge", "/Employees/Role/Index"),
                new SettingsLink("Permissions", "Feature access flags", "bi-shield-lock", "/Employees/Permission/Index"),
                new SettingsLink("My Profile", "Personal account details", "bi-person-circle", "/Account/Profile")
            })
        };

    private static string MaskConnectionString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "Not configured";
        var parts = value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var visible = parts.Where(part =>
            part.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase) ||
            part.StartsWith("Initial Catalog=", StringComparison.OrdinalIgnoreCase) ||
            part.StartsWith("Integrated Security=", StringComparison.OrdinalIgnoreCase));
        return string.Join("; ", visible);
    }

    private int CurrentUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    public sealed class SystemSettingsInput
    {
        public string PromptPayPayeeId { get; set; } = string.Empty;
        public string PromptPayDisplayName { get; set; } = string.Empty;
        public string ReceiptPrinterName { get; set; } = string.Empty;
        public bool CashDrawerEnabled { get; set; }
        public string DrawerKickCommand { get; set; } = string.Empty;
        public int DrawerPin { get; set; } = 2;
        public bool OpenDrawerAfterReceiptPrint { get; set; }
        public bool AllowManualOpenDrawer { get; set; }
    }

    public sealed class PaymentMethodInput
    {
        public int PaymentMethodId { get; set; }
        public string PaymentMethodCode { get; set; } = string.Empty;
        public string PaymentMethodName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool RequireReferenceNo { get; set; }
        public bool IsCash { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }

    public sealed record SettingsGroup(string Title, IReadOnlyCollection<SettingsLink> Links);
    public sealed record SettingsLink(string Title, string Description, string Icon, string Url);
}
