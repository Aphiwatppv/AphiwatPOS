using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AuthenticationEngine.Models;
using AuthenticationEngine.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AphiwatPOS.Pages.Employees.Role;

[Authorize(Roles = "Admin,Manager")]
public sealed class IndexModel : PageModel
{
    private readonly IPermissionService _permissionService;
    private readonly IRoleService _roleService;

    public IndexModel(IPermissionService permissionService, IRoleService roleService)
    {
        _permissionService = permissionService;
        _roleService = roleService;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchText { get; set; }

    [BindProperty]
    public RoleCreateInput CreateInput { get; set; } = new();

    [BindProperty]
    public RoleEditInput EditInput { get; set; } = new();

    [BindProperty]
    public RolePermissionInput PermissionInput { get; set; } = new();

    public IReadOnlyCollection<RoleSummary> Roles { get; private set; } = Array.Empty<RoleSummary>();
    public IReadOnlyCollection<PermissionSummary> Permissions { get; private set; } = Array.Empty<PermissionSummary>();
    public bool CanCreateRole { get; private set; }
    public bool CanUpdateRole { get; private set; }
    public bool CanAssignPermissions { get; private set; }
    public bool CanDeactivateRole { get; private set; }

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

        if (await _roleService.IsRoleCodeExistsAsync(CreateInput.RoleCode, cancellationToken: cancellationToken))
            ModelState.AddModelError("CreateInput.RoleCode", "Role code already exists.");

        if (!ModelState.IsValid)
        {
            await LoadPageAsync(cancellationToken);
            return Page();
        }

        await _roleService.CreateRoleAsync(
            CreateInput.RoleCode,
            CreateInput.RoleName,
            CreateInput.Description,
            CurrentUserId(),
            cancellationToken);

        StatusMessage = "Role created.";
        return RedirectToPage(new { SearchText });
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

        await _roleService.UpdateRoleAsync(
            EditInput.RoleId,
            EditInput.RoleName,
            EditInput.Description,
            EditInput.IsActive,
            CurrentUserId(),
            cancellationToken);

        StatusMessage = "Role updated.";
        return RedirectToPage(new { SearchText });
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int roleId, CancellationToken cancellationToken)
    {
        await _roleService.DeactivateRoleAsync(roleId, CurrentUserId(), cancellationToken);
        StatusMessage = "Role deactivated.";
        return RedirectToPage(new { SearchText });
    }

    public async Task<IActionResult> OnPostAssignPermissionsAsync(CancellationToken cancellationToken)
    {
        await _roleService.AssignPermissionsToRoleAsync(
            PermissionInput.RoleId,
            string.Join(',', PermissionInput.SelectedPermissions),
            CurrentUserId(),
            cancellationToken);

        StatusMessage = "Role permissions updated.";
        return RedirectToPage(new { SearchText });
    }

    private async Task LoadPageAsync(CancellationToken cancellationToken)
    {
        var roles = await _roleService.GetRolesAsync(cancellationToken);
        Roles = string.IsNullOrWhiteSpace(SearchText)
            ? roles
            : roles.Where(role =>
                    role.RoleCode.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    role.RoleName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .ToArray();

        Permissions = await _permissionService.GetPermissionsAsync(cancellationToken);
        CanCreateRole = HasPermission("ROLE_CREATE");
        CanUpdateRole = HasPermission("ROLE_UPDATE");
        CanAssignPermissions = HasPermission("ROLE_ASSIGN_PERMISSION");
        CanDeactivateRole = HasPermission("ROLE_DEACTIVATE");
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

    public sealed class RoleCreateInput
    {
        [Required]
        [Display(Name = "Role Code")]
        [StringLength(50)]
        public string RoleCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role Name")]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }
    }

    public sealed class RoleEditInput
    {
        public int RoleId { get; set; }

        [Required]
        [Display(Name = "Role Name")]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }

    public sealed class RolePermissionInput
    {
        public int RoleId { get; set; }
        public List<string> SelectedPermissions { get; set; } = new();
    }
}
