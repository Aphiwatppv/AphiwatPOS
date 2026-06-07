using System.ComponentModel.DataAnnotations;
using CustomerEngine.Models;
using CustomerEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AphiwatPOS.Pages.Customer.CustomerList;

public sealed class IndexModel : PageModel
{
    private readonly ICustomerService _customerService;
    private readonly ICustomerRegistrationService _registrationService;
    private readonly IMemberLevelService _memberLevelService;
    private readonly ICustomerCreditService _creditService;
    private readonly ICustomerReportService _reportService;

    public IndexModel(ICustomerService customerService, ICustomerRegistrationService registrationService, IMemberLevelService memberLevelService, ICustomerCreditService creditService, ICustomerReportService reportService)
    {
        _customerService = customerService;
        _registrationService = registrationService;
        _memberLevelService = memberLevelService;
        _creditService = creditService;
        _reportService = reportService;
    }

    [BindProperty(SupportsGet = true)] public string? SearchText { get; set; }
    [BindProperty(SupportsGet = true)] public string? MemberType { get; set; }
    [BindProperty(SupportsGet = true)] public int? MemberLevelId { get; set; }
    [BindProperty(SupportsGet = true)] public string? CreditStatus { get; set; }
    [BindProperty(SupportsGet = true)] public bool? IsActive { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    [BindProperty] public CustomerFormInput CustomerInput { get; set; } = new();
    [BindProperty] public CreditFormInput CreditInput { get; set; } = new();

    public CustomerPagedResultModel Customers { get; private set; } = new();
    public IReadOnlyCollection<MemberLevelModel> MemberLevels { get; private set; } = Array.Empty<MemberLevelModel>();
    public IReadOnlyDictionary<int, CustomerModel> CustomerEditModels { get; private set; } = new Dictionary<int, CustomerModel>();
    public CustomerReportSummaryModel Summary { get; private set; } = new();
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        StatusMessage = TempData["StatusMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        await LoadAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        NormalizeOptionalEmail();
        if (!ModelState.IsValid) return await FailedAsync("Please review the customer form.", cancellationToken);
        if (await _customerService.IsPhoneNumberExistsAsync(CustomerInput.PhoneNumber, null, cancellationToken)) return await FailedAsync("Phone number already exists.", cancellationToken);
        if (!string.IsNullOrWhiteSpace(CustomerInput.Email) && await _customerService.IsEmailExistsAsync(CustomerInput.Email, null, cancellationToken)) return await FailedAsync("Email already exists.", cancellationToken);

        await _registrationService.RegisterAsync(new CustomerCreateModel
        {
            CustomerCode = CustomerInput.CustomerCode,
            CustomerName = CustomerInput.CustomerName,
            PhoneNumber = CustomerInput.PhoneNumber,
            Email = CustomerInput.Email,
            MemberType = CustomerInput.MemberType,
            MemberTypeCodes = CustomerInput.ActiveMemberTypeCodes,
            WholesaleProfile = new WholesaleMemberProfileSaveModel
            {
                BusinessName = CustomerInput.WholesaleBusinessName,
                IsApproved = CustomerInput.WholesaleIsApproved,
                PaymentTermDays = CustomerInput.WholesalePaymentTermDays,
                ApprovedByUserId = CustomerInput.WholesaleIsApproved ? CustomerPageHelpers.CurrentUserId(User) : null
            },
            RubberSupplierProfile = new RubberSupplierMemberProfileSaveModel
            {
                SupplierCode = CustomerInput.RubberSupplierCode,
                Remark = CustomerInput.RubberSupplierRemark
            },
            MemberLevelId = CustomerInput.MemberLevelId,
            DateOfBirth = CustomerInput.DateOfBirth,
            Gender = CustomerInput.Gender,
            Address = CustomerInput.Address,
            CreatedByUserId = CustomerPageHelpers.CurrentUserId(User)
        }, cancellationToken);

        TempData["StatusMessage"] = "Customer created successfully.";
        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostUpdateAsync(CancellationToken cancellationToken)
    {
        NormalizeOptionalEmail();
        if (!ModelState.IsValid) return await FailedAsync("Please review the customer form.", cancellationToken);
        if (await _customerService.IsPhoneNumberExistsAsync(CustomerInput.PhoneNumber, CustomerInput.CustomerId, cancellationToken)) return await FailedAsync("Phone number already exists.", cancellationToken);
        if (!string.IsNullOrWhiteSpace(CustomerInput.Email) && await _customerService.IsEmailExistsAsync(CustomerInput.Email, CustomerInput.CustomerId, cancellationToken)) return await FailedAsync("Email already exists.", cancellationToken);

        await _registrationService.UpdateMembershipsAsync(new CustomerUpdateModel
        {
            CustomerId = CustomerInput.CustomerId,
            CustomerName = CustomerInput.CustomerName,
            PhoneNumber = CustomerInput.PhoneNumber,
            Email = CustomerInput.Email,
            MemberType = CustomerInput.MemberType,
            MemberTypeCodes = CustomerInput.ActiveMemberTypeCodes,
            WholesaleProfile = new WholesaleMemberProfileSaveModel
            {
                BusinessName = CustomerInput.WholesaleBusinessName,
                IsApproved = CustomerInput.WholesaleIsApproved,
                PaymentTermDays = CustomerInput.WholesalePaymentTermDays,
                ApprovedByUserId = CustomerInput.WholesaleIsApproved ? CustomerPageHelpers.CurrentUserId(User) : null
            },
            RubberSupplierProfile = new RubberSupplierMemberProfileSaveModel
            {
                SupplierCode = CustomerInput.RubberSupplierCode,
                Remark = CustomerInput.RubberSupplierRemark
            },
            MemberLevelId = CustomerInput.MemberLevelId,
            DateOfBirth = CustomerInput.DateOfBirth,
            Gender = CustomerInput.Gender,
            Address = CustomerInput.Address,
            ApplyMemberLevelCreditDefault = CustomerInput.ApplyMemberLevelCreditDefault,
            UpdatedByUserId = CustomerPageHelpers.CurrentUserId(User)
        }, cancellationToken);

        await _customerService.ToggleActiveAsync(CustomerInput.CustomerId, CustomerInput.IsActive, CustomerPageHelpers.CurrentUserId(User), cancellationToken);
        TempData["StatusMessage"] = "Customer updated successfully.";
        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(int customerId, bool isActive, CancellationToken cancellationToken)
    {
        await _customerService.ToggleActiveAsync(customerId, isActive, CustomerPageHelpers.CurrentUserId(User), cancellationToken);
        TempData["StatusMessage"] = isActive ? "Customer activated." : "Customer deactivated.";
        return RedirectToCurrentFilters();
    }

    public async Task<IActionResult> OnPostSetCreditAsync(CancellationToken cancellationToken)
    {
        await _creditService.SetCreditAsync(new CustomerCreditUpdateModel
        {
            CustomerId = CreditInput.CustomerId,
            AllowCredit = CreditInput.AllowCredit,
            CreditLimit = CreditInput.CreditLimit,
            CreditTermDays = CreditInput.CreditTermDays,
            CreditStatus = CreditInput.CreditStatus,
            RequireManagerApproval = CreditInput.RequireManagerApproval,
            ApprovedByUserId = CustomerPageHelpers.CurrentUserId(User),
            Remark = CreditInput.Remark,
            UpdatedByUserId = CustomerPageHelpers.CurrentUserId(User)
        }, cancellationToken);
        TempData["StatusMessage"] = "Customer credit settings updated.";
        return RedirectToCurrentFilters();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            MemberLevels = await _memberLevelService.GetAllActiveAsync(cancellationToken);
            Customers = await _customerService.GetPagedAsync(new CustomerPagedRequestModel { PageNumber = PageNumber, PageSize = 10, SearchText = SearchText, MemberType = MemberType, MemberLevelId = MemberLevelId, IsActive = IsActive, CreditStatus = CreditStatus }, cancellationToken);
            CustomerEditModels = await LoadCustomerEditModelsAsync(Customers.Customers, cancellationToken);
            Summary = await _reportService.GetSummaryAsync(new CustomerReportRequestModel { MemberType = MemberType, MemberLevelId = MemberLevelId, IsActive = IsActive }, cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorMessage ??= $"Failed to load customer data. {ex.Message}";
        }
    }

    private async Task<IActionResult> FailedAsync(string message, CancellationToken cancellationToken)
    {
        ErrorMessage = message;
        await LoadAsync(cancellationToken);
        return Page();
    }

    private IActionResult RedirectToCurrentFilters() => RedirectToPage(new { SearchText, MemberType, MemberLevelId, CreditStatus, IsActive, PageNumber });

    private void NormalizeOptionalEmail()
    {
        if (string.IsNullOrWhiteSpace(CustomerInput.Email))
        {
            CustomerInput.Email = null;
            ModelState.Remove("CustomerInput.Email");
        }
        else
        {
            CustomerInput.Email = CustomerInput.Email.Trim();
        }
    }

    private async Task<IReadOnlyDictionary<int, CustomerModel>> LoadCustomerEditModelsAsync(IReadOnlyCollection<CustomerSummaryModel> rows, CancellationToken cancellationToken)
    {
        if (rows.Count == 0) return new Dictionary<int, CustomerModel>();

        var details = await Task.WhenAll(rows.Select(row => _customerService.GetByIdAsync(row.CustomerId, cancellationToken)));
        return details
            .Where(customer => customer is not null)
            .Cast<CustomerModel>()
            .ToDictionary(customer => customer.CustomerId);
    }

    public sealed class CustomerFormInput
    {
        public int CustomerId { get; set; }
        [StringLength(50)] public string? CustomerCode { get; set; }
        [Required, StringLength(255)] public string CustomerName { get; set; } = string.Empty;
        [Required, StringLength(50)] public string PhoneNumber { get; set; } = string.Empty;
        [EmailAddress, StringLength(255)] public string? Email { get; set; }
        [Required] public string MemberType { get; set; } = "Retail";
        public List<string> ActiveMemberTypeCodes { get; set; } = new() { MemberTypeCodes.Retail };
        public int? MemberLevelId { get; set; }
        public string? WholesaleBusinessName { get; set; }
        public bool WholesaleIsApproved { get; set; }
        [Range(0, int.MaxValue)] public int WholesalePaymentTermDays { get; set; }
        public string? RubberSupplierCode { get; set; }
        public string? RubberSupplierRemark { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
        public bool ApplyMemberLevelCreditDefault { get; set; }
    }

    public sealed class CreditFormInput
    {
        public int CustomerId { get; set; }
        public bool AllowCredit { get; set; }
        [Range(0, double.MaxValue)] public decimal CreditLimit { get; set; }
        [Range(0, int.MaxValue)] public int CreditTermDays { get; set; }
        public string CreditStatus { get; set; } = "Good";
        public bool RequireManagerApproval { get; set; }
        public string? Remark { get; set; }
    }
}
