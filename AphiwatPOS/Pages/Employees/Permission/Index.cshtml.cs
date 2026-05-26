using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AuthenticationEngine.Models;
using AuthenticationEngine.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AphiwatPOS.Pages.Employees.Permission;

[Authorize(Roles = "Admin")]
public sealed class IndexModel : PageModel
{
    private readonly IPermissionService _permissionService;

    public IndexModel(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchText { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ModuleFilter { get; set; }

    [BindProperty]
    public PermissionCreateInput CreateInput { get; set; } = new();

    [BindProperty]
    public PermissionEditInput EditInput { get; set; } = new();

    public IReadOnlyCollection<PermissionSummary> Permissions { get; private set; } = Array.Empty<PermissionSummary>();
    public IReadOnlyCollection<string> Modules { get; private set; } = Array.Empty<string>();
    public bool CanCreatePermission { get; private set; }
    public bool CanUpdatePermission { get; private set; }
    public bool CanDeactivatePermission { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadPageAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        ModelState.Clear();
        TryValidateModel(CreateInput, nameof(CreateInput));

        if (await _permissionService.IsPermissionCodeExistsAsync(CreateInput.PermissionCode, cancellationToken: cancellationToken))
            ModelState.AddModelError("CreateInput.PermissionCode", "Permission code already exists.");

        if (!ModelState.IsValid)
        {
            await LoadPageAsync(cancellationToken);
            return Page();
        }

        await _permissionService.CreatePermissionAsync(
            CreateInput.PermissionCode,
            CreateInput.PermissionName,
            CreateInput.ModuleName,
            CreateInput.Description,
            CurrentUserId(),
            cancellationToken);

        StatusMessage = "Permission created.";
        return RedirectToPage(new { SearchText, ModuleFilter });
    }

    public async Task<IActionResult> OnPostUpdateAsync(CancellationToken cancellationToken)
    {
        ModelState.Clear();
        TryValidateModel(EditInput, nameof(EditInput));

        if (!ModelState.IsValid)
        {
            await LoadPageAsync(cancellationToken);
            return Page();
        }

        await _permissionService.UpdatePermissionAsync(
            EditInput.PermissionId,
            EditInput.PermissionName,
            EditInput.ModuleName,
            EditInput.Description,
            EditInput.IsActive,
            CurrentUserId(),
            cancellationToken);

        StatusMessage = "Permission updated.";
        return RedirectToPage(new { SearchText, ModuleFilter });
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int permissionId, CancellationToken cancellationToken)
    {
        await _permissionService.DeactivatePermissionAsync(permissionId, CurrentUserId(), cancellationToken);
        StatusMessage = "Permission deactivated.";
        return RedirectToPage(new { SearchText, ModuleFilter });
    }

    private async Task LoadPageAsync(CancellationToken cancellationToken)
    {
        var permissions = await _permissionService.GetPermissionsAsync(cancellationToken);
        Modules = permissions
            .Select(permission => permission.ModuleName)
            .Where(module => !string.IsNullOrWhiteSpace(module))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(module => module)
            .ToArray();

        Permissions = permissions
            .Where(permission =>
                string.IsNullOrWhiteSpace(SearchText) ||
                permission.PermissionCode.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                permission.PermissionName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            .Where(permission =>
                string.IsNullOrWhiteSpace(ModuleFilter) ||
                permission.ModuleName.Equals(ModuleFilter, StringComparison.OrdinalIgnoreCase))
            .OrderBy(permission => permission.ModuleName)
            .ThenBy(permission => permission.PermissionCode)
            .ToArray();
        CanCreatePermission = HasPermission("PERMISSION_CREATE");
        CanUpdatePermission = HasPermission("PERMISSION_UPDATE");
        CanDeactivatePermission = HasPermission("PERMISSION_DEACTIVATE");
    }

    private bool HasPermission(string permissionCode)
    {
        return User.IsInRole("Admin") ||
            User.Claims.Any(claim =>
                claim.Type == "Permission" &&
                string.Equals(claim.Value, permissionCode, StringComparison.OrdinalIgnoreCase));
    }

    private int CurrentUserId()
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : 0;
    }

    public sealed class PermissionCreateInput
    {
        [Required]
        [Display(Name = "Permission Code")]
        [StringLength(100)]
        public string PermissionCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Permission Name")]
        [StringLength(100)]
        public string PermissionName { get; set; } = string.Empty;

        [Display(Name = "Module Name")]
        [StringLength(100)]
        public string? ModuleName { get; set; }

        [StringLength(250)]
        public string? Description { get; set; }
    }

    public sealed class PermissionEditInput
    {
        public int PermissionId { get; set; }

        [Required]
        [Display(Name = "Permission Name")]
        [StringLength(100)]
        public string PermissionName { get; set; } = string.Empty;

        [Display(Name = "Module Name")]
        [StringLength(100)]
        public string? ModuleName { get; set; }

        [StringLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}
