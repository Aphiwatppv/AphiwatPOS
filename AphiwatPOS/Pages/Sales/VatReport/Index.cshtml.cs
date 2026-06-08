using System.ComponentModel.DataAnnotations;
using System.Text;
using AuthenticationEngine.Models;
using AuthenticationEngine.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SalesEngine.Models;
using SalesEngine.Services;

namespace AphiwatPOS.Pages.Sales.VatReport;

[Authorize(Policy = "ReportView")]
public sealed class IndexModel : PageModel
{
    private readonly ISalesHistoryService _salesHistoryService;
    private readonly IUserManagementService _userManagementService;

    public IndexModel(ISalesHistoryService salesHistoryService, IUserManagementService userManagementService)
    {
        _salesHistoryService = salesHistoryService;
        _userManagementService = userManagementService;
    }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? ToDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? CashierUserId { get; set; }

    public IReadOnlyCollection<SalesSummaryModel> DailyRows { get; private set; } = Array.Empty<SalesSummaryModel>();
    public IReadOnlyCollection<SalesVatBillReportModel> BillRows { get; private set; } = Array.Empty<SalesVatBillReportModel>();
    public IReadOnlyCollection<UserSummary> Cashiers { get; private set; } = Array.Empty<UserSummary>();
    public IReadOnlyCollection<MonthlyVatSummary> MonthlyRows { get; private set; } = Array.Empty<MonthlyVatSummary>();

    public decimal TotalTaxableAmount => BillRows.Sum(x => x.TaxableAmount);
    public decimal TotalVatInAmount => DailyRows.Sum(x => x.VatInAmount);
    public decimal TotalVatOutAmount => DailyRows.Sum(x => x.VatOutAmount);
    public decimal TotalVatPayableAmount => TotalVatOutAmount - TotalVatInAmount;
    public decimal TotalNetAmount => DailyRows.Sum(x => x.NetAmount);
    public decimal TotalRefundAmount => DailyRows.Sum(x => x.RefundAmount);

    public DateTime EffectiveFromDate => (FromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
    public DateTime EffectiveToDate => (ToDate ?? DateTime.Today).Date;

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
    }

    public async Task<IActionResult> OnGetExportRevenueCsvAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine("Sale Date,Tax Invoice No,Customer Name,Customer Tax ID,Taxable Amount,VAT Out,VAT In,VAT Payable,Net Amount,Status");
        foreach (var row in BillRows)
        {
            builder.AppendCsv(row.SaleDate.ToString("yyyy-MM-dd"));
            builder.AppendCsv(row.SaleNo);
            builder.AppendCsv(row.CustomerName);
            builder.AppendCsv(row.CustomerTaxId);
            builder.AppendCsv(row.TaxableAmount.ToString("0.00"));
            builder.AppendCsv(row.VatOutAmount.ToString("0.00"));
            builder.AppendCsv(row.VatInAmount.ToString("0.00"));
            builder.AppendCsv(row.VatPayableAmount.ToString("0.00"));
            builder.AppendCsv(row.NetAmount.ToString("0.00"));
            builder.AppendCsv(row.Status, endLine: true);
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
        var fileName = $"vat-report-{EffectiveFromDate:yyyyMMdd}-{EffectiveToDate:yyyyMMdd}.csv";
        return File(bytes, "text/csv; charset=utf-8", fileName);
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        var fromDate = EffectiveFromDate;
        var toDate = EffectiveToDate;
        if (toDate < fromDate)
        {
            toDate = fromDate;
            ToDate = toDate;
        }

        Cashiers = (await _userManagementService.GetUsersAsync(cancellationToken))
            .Where(user => user.IsActive)
            .OrderBy(user => user.DisplayName)
            .ToArray();

        DailyRows = await _salesHistoryService.GetSummaryByDateRangeAsync(fromDate, toDate, CashierUserId, cancellationToken);
        BillRows = await _salesHistoryService.GetVatBillReportAsync(fromDate, toDate, CashierUserId, cancellationToken);
        MonthlyRows = DailyRows
            .GroupBy(row => new DateTime(row.SaleDate.Year, row.SaleDate.Month, 1))
            .Select(group => new MonthlyVatSummary
            {
                Month = group.Key,
                TransactionCount = group.Sum(x => x.TransactionCount),
                GrossAmount = group.Sum(x => x.GrossAmount),
                DiscountAmount = group.Sum(x => x.DiscountAmount),
                TaxAmount = group.Sum(x => x.TaxAmount),
                NetAmount = group.Sum(x => x.NetAmount),
                RefundAmount = group.Sum(x => x.RefundAmount),
                VatInAmount = group.Sum(x => x.VatInAmount),
                VatOutAmount = group.Sum(x => x.VatOutAmount)
            })
            .OrderBy(row => row.Month)
            .ToArray();
    }

    public sealed class MonthlyVatSummary
    {
        public DateTime Month { get; init; }
        public int TransactionCount { get; init; }
        public decimal GrossAmount { get; init; }
        public decimal DiscountAmount { get; init; }
        public decimal TaxAmount { get; init; }
        public decimal NetAmount { get; init; }
        public decimal RefundAmount { get; init; }
        public decimal VatInAmount { get; init; }
        public decimal VatOutAmount { get; init; }
        public decimal VatPayableAmount => VatOutAmount - VatInAmount;
    }
}

internal static class VatReportCsvExtensions
{
    public static void AppendCsv(this StringBuilder builder, string? value, bool endLine = false)
    {
        var escaped = (value ?? string.Empty).Replace("\"", "\"\"");
        builder.Append('"').Append(escaped).Append('"');
        builder.Append(endLine ? Environment.NewLine : ',');
    }
}
