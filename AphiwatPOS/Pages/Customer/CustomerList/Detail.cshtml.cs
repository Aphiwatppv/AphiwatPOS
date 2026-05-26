using System.ComponentModel.DataAnnotations;
using CustomerEngine.Models;
using CustomerEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AphiwatPOS.Pages.Customer.CustomerList;

public sealed class DetailModel : PageModel
{
    private readonly ICustomerService _customerService;
    private readonly IMemberLevelService _memberLevelService;
    private readonly IMemberLevelUpgradeRuleService _upgradeService;
    private readonly ICustomerCreditService _creditService;
    private readonly ICustomerHistoryService _historyService;

    public DetailModel(ICustomerService customerService, IMemberLevelService memberLevelService, IMemberLevelUpgradeRuleService upgradeService, ICustomerCreditService creditService, ICustomerHistoryService historyService)
    {
        _customerService = customerService;
        _memberLevelService = memberLevelService;
        _upgradeService = upgradeService;
        _creditService = creditService;
        _historyService = historyService;
    }

    [BindProperty(SupportsGet = true)] public int CustomerId { get; set; }
    [BindProperty] public IndexModel.CustomerFormInput CustomerInput { get; set; } = new();
    [BindProperty] public IndexModel.CreditFormInput CreditInput { get; set; } = new();
    [BindProperty] public NoteFormInput NoteInput { get; set; } = new();

    public CustomerModel? Customer { get; private set; }
    public IReadOnlyCollection<MemberLevelModel> MemberLevels { get; private set; } = Array.Empty<MemberLevelModel>();
    public CustomerLevelEligibilityResultModel? Eligibility { get; private set; }
    public PagedResultModel<CustomerTimelineModel> Timeline { get; private set; } = new();
    public PagedResultModel<CustomerPurchaseHistoryModel> Purchases { get; private set; } = new();
    public PagedResultModel<CustomerPaymentHistoryModel> Payments { get; private set; } = new();
    public PagedResultModel<CustomerCreditHistoryModel> CreditHistory { get; private set; } = new();
    public PagedResultModel<CustomerPointHistoryModel> PointHistory { get; private set; } = new();
    public PagedResultModel<CustomerLevelHistoryModel> LevelHistory { get; private set; } = new();
    public CustomerNotePagedResultModel Notes { get; private set; } = new();
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(int customerId, CancellationToken cancellationToken)
    {
        CustomerId = customerId;
        StatusMessage = TempData["StatusMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        await LoadAsync(cancellationToken);
        return Customer is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(CancellationToken cancellationToken)
    {
        CustomerId = CustomerInput.CustomerId;
        await _customerService.UpdateAsync(new CustomerUpdateModel { CustomerId = CustomerInput.CustomerId, CustomerName = CustomerInput.CustomerName, PhoneNumber = CustomerInput.PhoneNumber, Email = CustomerInput.Email, MemberLevelId = CustomerInput.MemberLevelId, DateOfBirth = CustomerInput.DateOfBirth, Gender = CustomerInput.Gender, Address = CustomerInput.Address, ApplyMemberLevelCreditDefault = CustomerInput.ApplyMemberLevelCreditDefault, UpdatedByUserId = CustomerPageHelpers.CurrentUserId(User) }, cancellationToken);
        await _customerService.ToggleActiveAsync(CustomerInput.CustomerId, CustomerInput.IsActive, CustomerPageHelpers.CurrentUserId(User), cancellationToken);
        TempData["StatusMessage"] = "Customer updated.";
        return RedirectToPage(new { customerId = CustomerInput.CustomerId });
    }

    public async Task<IActionResult> OnPostSetCreditAsync(CancellationToken cancellationToken)
    {
        CustomerId = CreditInput.CustomerId;
        await _creditService.SetCreditAsync(new CustomerCreditUpdateModel { CustomerId = CreditInput.CustomerId, AllowCredit = CreditInput.AllowCredit, CreditLimit = CreditInput.CreditLimit, CreditTermDays = CreditInput.CreditTermDays, CreditStatus = CreditInput.CreditStatus, RequireManagerApproval = CreditInput.RequireManagerApproval, ApprovedByUserId = CustomerPageHelpers.CurrentUserId(User), Remark = CreditInput.Remark, UpdatedByUserId = CustomerPageHelpers.CurrentUserId(User) }, cancellationToken);
        TempData["StatusMessage"] = "Credit settings updated.";
        return RedirectToPage(new { customerId = CreditInput.CustomerId });
    }

    public async Task<IActionResult> OnPostCheckUpgradeEligibilityAsync(int customerId, CancellationToken cancellationToken)
    {
        CustomerId = customerId;
        Eligibility = await _upgradeService.CheckCustomerLevelEligibilityAsync(customerId, cancellationToken);
        await LoadAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostUpgradeLevelAsync(int customerId, CancellationToken cancellationToken)
    {
        await _upgradeService.UpgradeCustomerLevelAsync(customerId, CustomerPageHelpers.CurrentUserId(User), true, true, cancellationToken);
        TempData["StatusMessage"] = "Customer member level upgraded.";
        return RedirectToPage(new { customerId });
    }

    public async Task<IActionResult> OnPostAddNoteAsync(CancellationToken cancellationToken)
    {
        await _historyService.AddNoteAsync(new CustomerNoteCreateModel { CustomerId = NoteInput.CustomerId, NoteType = NoteInput.NoteType, NoteText = NoteInput.NoteText, IsImportant = NoteInput.IsImportant, CreatedByUserId = CustomerPageHelpers.CurrentUserId(User) }, cancellationToken);
        TempData["StatusMessage"] = "Note added.";
        return RedirectToPage(new { customerId = NoteInput.CustomerId });
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Customer = await _customerService.GetByIdAsync(CustomerId, cancellationToken);
        MemberLevels = await _memberLevelService.GetAllActiveAsync(cancellationToken);
        if (Customer is null) return;
        var request = new CustomerHistoryPagedRequestModel { CustomerId = CustomerId, PageNumber = 1, PageSize = 10 };
        Timeline = await _historyService.GetTimelineAsync(new CustomerTimelinePagedRequestModel { CustomerId = CustomerId, PageNumber = 1, PageSize = 10 }, cancellationToken);
        Purchases = await _historyService.GetPurchaseHistoryAsync(request, cancellationToken);
        Payments = await _historyService.GetPaymentHistoryAsync(request, cancellationToken);
        CreditHistory = await _historyService.GetCreditHistoryAsync(request, cancellationToken);
        PointHistory = await _historyService.GetPointHistoryAsync(request, cancellationToken);
        LevelHistory = await _historyService.GetLevelHistoryAsync(request, cancellationToken);
        Notes = await _historyService.GetNotesPagedAsync(new CustomerNotePagedRequestModel { CustomerId = CustomerId, PageNumber = 1, PageSize = 20, IsActive = true }, cancellationToken);
    }

    public sealed class NoteFormInput
    {
        public int CustomerId { get; set; }
        public string NoteType { get; set; } = "General";
        [Required] public string NoteText { get; set; } = string.Empty;
        public bool IsImportant { get; set; }
    }
}
