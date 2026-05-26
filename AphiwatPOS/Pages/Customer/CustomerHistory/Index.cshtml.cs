using System.ComponentModel.DataAnnotations;
using CustomerEngine.Models;
using CustomerEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AphiwatPOS.Pages.Customer.CustomerHistory;

public sealed class IndexModel : PageModel
{
    private readonly ICustomerService _customerService;
    private readonly ICustomerHistoryService _historyService;

    public IndexModel(ICustomerService customerService, ICustomerHistoryService historyService)
    {
        _customerService = customerService;
        _historyService = historyService;
    }

    [BindProperty(SupportsGet = true)] public int? CustomerId { get; set; }
    [BindProperty(SupportsGet = true)] public string? SearchText { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateFrom { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateTo { get; set; }
    [BindProperty] public NoteFormInput NoteInput { get; set; } = new();

    public IReadOnlyCollection<CustomerSummaryModel> Customers { get; private set; } = Array.Empty<CustomerSummaryModel>();
    public CustomerModel? SelectedCustomer { get; private set; }
    public CustomerHistorySummaryModel? Summary { get; private set; }
    public PagedResultModel<CustomerTimelineModel> Timeline { get; private set; } = new();
    public PagedResultModel<CustomerPurchaseHistoryModel> Purchases { get; private set; } = new();
    public PagedResultModel<CustomerPaymentHistoryModel> Payments { get; private set; } = new();
    public PagedResultModel<CustomerCreditHistoryModel> CreditHistory { get; private set; } = new();
    public PagedResultModel<CustomerPointHistoryModel> PointHistory { get; private set; } = new();
    public PagedResultModel<CustomerLevelHistoryModel> LevelHistory { get; private set; } = new();
    public PagedResultModel<CustomerRefundHistoryModel> Refunds { get; private set; } = new();
    public CustomerNotePagedResultModel Notes { get; private set; } = new();
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        StatusMessage = TempData["StatusMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        await LoadAsync(cancellationToken);
    }

    public IActionResult OnPostLoadCustomer(int customerId) => RedirectToPage(new { CustomerId = customerId, SearchText, DateFrom, DateTo });

    public async Task<IActionResult> OnPostAddNoteAsync(CancellationToken cancellationToken)
    {
        await _historyService.AddNoteAsync(new CustomerNoteCreateModel { CustomerId = NoteInput.CustomerId, NoteType = NoteInput.NoteType, NoteText = NoteInput.NoteText, IsImportant = NoteInput.IsImportant, CreatedByUserId = CustomerPageHelpers.CurrentUserId(User) }, cancellationToken);
        TempData["StatusMessage"] = "Note added.";
        return RedirectToPage(new { CustomerId = NoteInput.CustomerId, DateFrom, DateTo });
    }

    public async Task<IActionResult> OnPostUpdateNoteAsync(CancellationToken cancellationToken)
    {
        await _historyService.UpdateNoteAsync(new CustomerNoteUpdateModel { CustomerNoteId = NoteInput.CustomerNoteId, NoteType = NoteInput.NoteType, NoteText = NoteInput.NoteText, IsImportant = NoteInput.IsImportant, IsActive = true, UpdatedByUserId = CustomerPageHelpers.CurrentUserId(User) }, cancellationToken);
        TempData["StatusMessage"] = "Note updated.";
        return RedirectToPage(new { CustomerId = NoteInput.CustomerId, DateFrom, DateTo });
    }

    public async Task<IActionResult> OnPostToggleNoteActiveAsync(long customerNoteId, int customerId, bool isActive, CancellationToken cancellationToken)
    {
        await _historyService.ToggleNoteActiveAsync(customerNoteId, isActive, CustomerPageHelpers.CurrentUserId(User), cancellationToken);
        TempData["StatusMessage"] = isActive ? "Note restored." : "Note hidden.";
        return RedirectToPage(new { CustomerId = customerId, DateFrom, DateTo });
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Customers = (await _customerService.GetPagedAsync(new CustomerPagedRequestModel { PageNumber = 1, PageSize = 100, SearchText = SearchText }, cancellationToken)).Customers;
        if (CustomerId is not int id) return;
        SelectedCustomer = await _customerService.GetByIdAsync(id, cancellationToken);
        Summary = await _historyService.GetSummaryAsync(id, DateFrom, DateTo, cancellationToken);
        var request = new CustomerHistoryPagedRequestModel { CustomerId = id, DateFrom = DateFrom, DateTo = DateTo, PageNumber = 1, PageSize = 20 };
        Timeline = await _historyService.GetTimelineAsync(new CustomerTimelinePagedRequestModel { CustomerId = id, DateFrom = DateFrom, DateTo = DateTo, PageNumber = 1, PageSize = 30 }, cancellationToken);
        Purchases = await _historyService.GetPurchaseHistoryAsync(request, cancellationToken);
        Payments = await _historyService.GetPaymentHistoryAsync(request, cancellationToken);
        CreditHistory = await _historyService.GetCreditHistoryAsync(request, cancellationToken);
        PointHistory = await _historyService.GetPointHistoryAsync(request, cancellationToken);
        LevelHistory = await _historyService.GetLevelHistoryAsync(request, cancellationToken);
        Refunds = await _historyService.GetRefundHistoryAsync(request, cancellationToken);
        Notes = await _historyService.GetNotesPagedAsync(new CustomerNotePagedRequestModel { CustomerId = id, PageNumber = 1, PageSize = 50, IsActive = true }, cancellationToken);
    }

    public sealed class NoteFormInput
    {
        public long CustomerNoteId { get; set; }
        public int CustomerId { get; set; }
        public string NoteType { get; set; } = "General";
        [Required] public string NoteText { get; set; } = string.Empty;
        public bool IsImportant { get; set; }
    }
}
