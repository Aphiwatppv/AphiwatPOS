using System.ComponentModel.DataAnnotations;
using CustomerEngine.Models;
using CustomerEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AphiwatPOS.Pages.Customer.MemberLevel;

public sealed class IndexModel : PageModel
{
    private readonly IMemberLevelService _levelService;
    private readonly IMemberLevelUpgradeRuleService _ruleService;

    public IndexModel(IMemberLevelService levelService, IMemberLevelUpgradeRuleService ruleService)
    {
        _levelService = levelService;
        _ruleService = ruleService;
    }

    [BindProperty] public LevelFormInput LevelInput { get; set; } = new();
    [BindProperty] public RuleFormInput RuleInput { get; set; } = new();
    public IReadOnlyCollection<MemberLevelModel> Levels { get; private set; } = Array.Empty<MemberLevelModel>();
    public IReadOnlyCollection<MemberLevelUpgradeRuleModel> Rules { get; private set; } = Array.Empty<MemberLevelUpgradeRuleModel>();
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
        if (await _levelService.IsLevelCodeExistsAsync(LevelInput.LevelCode, null, cancellationToken)) return await Fail("Level code already exists.", cancellationToken);
        await _levelService.CreateAsync(ToCreate(LevelInput), cancellationToken);
        TempData["StatusMessage"] = "Member level created.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync(CancellationToken cancellationToken)
    {
        if (await _levelService.IsLevelCodeExistsAsync(LevelInput.LevelCode, LevelInput.MemberLevelId, cancellationToken)) return await Fail("Level code already exists.", cancellationToken);
        await _levelService.UpdateAsync(ToUpdate(LevelInput), cancellationToken);
        TempData["StatusMessage"] = "Member level updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(int memberLevelId, bool isActive, CancellationToken cancellationToken)
    {
        await _levelService.ToggleActiveAsync(memberLevelId, isActive, CustomerPageHelpers.CurrentUserId(User), cancellationToken);
        TempData["StatusMessage"] = isActive ? "Member level activated." : "Member level deactivated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreateOrUpdateUpgradeRuleAsync(CancellationToken cancellationToken)
    {
        if (RuleInput.FromMemberLevelId == RuleInput.ToMemberLevelId) return await Fail("Upgrade rule cannot upgrade to the same level.", cancellationToken);
        var existing = await _ruleService.GetByFromLevelIdAsync(RuleInput.FromMemberLevelId, cancellationToken);
        if (existing is null)
        {
            await _ruleService.CreateAsync(new MemberLevelUpgradeRuleCreateModel { FromMemberLevelId = RuleInput.FromMemberLevelId, ToMemberLevelId = RuleInput.ToMemberLevelId, RequiredTotalSpending = RuleInput.RequiredTotalSpending, RequiredPurchaseCount = RuleInput.RequiredPurchaseCount, RequiredMembershipDays = RuleInput.RequiredMembershipDays, RequireNoOverduePayment = RuleInput.RequireNoOverduePayment, RequireManagerApproval = RuleInput.RequireManagerApproval, CreatedByUserId = CustomerPageHelpers.CurrentUserId(User) }, cancellationToken);
        }
        else
        {
            await _ruleService.UpdateAsync(new MemberLevelUpgradeRuleUpdateModel { MemberLevelUpgradeRuleId = existing.MemberLevelUpgradeRuleId, FromMemberLevelId = RuleInput.FromMemberLevelId, ToMemberLevelId = RuleInput.ToMemberLevelId, RequiredTotalSpending = RuleInput.RequiredTotalSpending, RequiredPurchaseCount = RuleInput.RequiredPurchaseCount, RequiredMembershipDays = RuleInput.RequiredMembershipDays, RequireNoOverduePayment = RuleInput.RequireNoOverduePayment, RequireManagerApproval = RuleInput.RequireManagerApproval, IsActive = RuleInput.IsActive, UpdatedByUserId = CustomerPageHelpers.CurrentUserId(User) }, cancellationToken);
        }
        TempData["StatusMessage"] = "Upgrade rule saved.";
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Levels = await _levelService.GetAllAsync(cancellationToken);
        Rules = await _ruleService.GetAllAsync(cancellationToken);
    }

    private async Task<IActionResult> Fail(string message, CancellationToken cancellationToken) { ErrorMessage = message; await LoadAsync(cancellationToken); return Page(); }

    private MemberLevelCreateModel ToCreate(LevelFormInput x) => new() { LevelCode = x.LevelCode, LevelName = x.LevelName, Description = x.Description, MinSpendingAmount = x.MinSpendingAmount, DiscountPercent = x.DiscountPercent, PointEarnAmount = x.PointEarnAmount, PointEarnPoint = x.PointEarnPoint, PointMultiplier = x.PointMultiplier, AllowCredit = x.AllowCredit, DefaultCreditLimit = x.AllowCredit ? x.DefaultCreditLimit : 0, DefaultCreditTermDays = x.AllowCredit ? x.DefaultCreditTermDays : 0, RequireManagerApprovalForCredit = x.RequireManagerApprovalForCredit, MaxOverdueDaysAllowed = x.MaxOverdueDaysAllowed, DisplayOrder = x.DisplayOrder, CreatedByUserId = CustomerPageHelpers.CurrentUserId(User) };
    private MemberLevelUpdateModel ToUpdate(LevelFormInput x) => new() { MemberLevelId = x.MemberLevelId, LevelCode = x.LevelCode, LevelName = x.LevelName, Description = x.Description, MinSpendingAmount = x.MinSpendingAmount, DiscountPercent = x.DiscountPercent, PointEarnAmount = x.PointEarnAmount, PointEarnPoint = x.PointEarnPoint, PointMultiplier = x.PointMultiplier, AllowCredit = x.AllowCredit, DefaultCreditLimit = x.AllowCredit ? x.DefaultCreditLimit : 0, DefaultCreditTermDays = x.AllowCredit ? x.DefaultCreditTermDays : 0, RequireManagerApprovalForCredit = x.RequireManagerApprovalForCredit, MaxOverdueDaysAllowed = x.MaxOverdueDaysAllowed, DisplayOrder = x.DisplayOrder, IsActive = x.IsActive, UpdatedByUserId = CustomerPageHelpers.CurrentUserId(User) };

    public sealed class LevelFormInput
    {
        public int MemberLevelId { get; set; }
        [Required] public string LevelCode { get; set; } = string.Empty;
        [Required] public string LevelName { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Range(0, double.MaxValue)] public decimal MinSpendingAmount { get; set; }
        [Range(0, 100)] public decimal DiscountPercent { get; set; }
        [Range(0.01, double.MaxValue)] public decimal PointEarnAmount { get; set; } = 100;
        [Range(0, double.MaxValue)] public decimal PointEarnPoint { get; set; } = 1;
        [Range(0.01, double.MaxValue)] public decimal PointMultiplier { get; set; } = 1;
        public bool AllowCredit { get; set; }
        [Range(0, double.MaxValue)] public decimal DefaultCreditLimit { get; set; }
        [Range(0, int.MaxValue)] public int DefaultCreditTermDays { get; set; }
        public bool RequireManagerApprovalForCredit { get; set; }
        [Range(0, int.MaxValue)] public int MaxOverdueDaysAllowed { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public sealed class RuleFormInput
    {
        public int FromMemberLevelId { get; set; }
        public int ToMemberLevelId { get; set; }
        public decimal RequiredTotalSpending { get; set; }
        public int RequiredPurchaseCount { get; set; }
        public int RequiredMembershipDays { get; set; }
        public bool RequireNoOverduePayment { get; set; } = true;
        public bool RequireManagerApproval { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
