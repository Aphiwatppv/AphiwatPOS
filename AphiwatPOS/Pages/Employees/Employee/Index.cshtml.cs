using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AuthenticationEngine.Models;
using AuthenticationEngine.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AphiwatPOS.Pages.Employees.Employee;

[Authorize(Roles = "Admin,Manager")]
public sealed class IndexModel : PageModel
{
    private readonly IWebHostEnvironment _environment;
    private readonly IPermissionService _permissionService;
    private readonly IRoleService _roleService;
    private readonly IUserManagementService _userManagementService;

    public IndexModel(
        IWebHostEnvironment environment,
        IPermissionService permissionService,
        IRoleService roleService,
        IUserManagementService userManagementService)
    {
        _environment = environment;
        _permissionService = permissionService;
        _roleService = roleService;
        _userManagementService = userManagementService;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchText { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? RoleCode { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? LockStatus { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty]
    public UserCreateInput CreateInput { get; set; } = new();

    [BindProperty]
    public UserEditInput EditInput { get; set; } = new();

    [BindProperty]
    public PasswordResetInput ResetPasswordInput { get; set; } = new();

    [BindProperty]
    public UserLockInput LockInput { get; set; } = new();

    [BindProperty]
    public AssignmentFormInput AssignmentInput { get; set; } = new();

    public UserPagedResult Users { get; private set; } = new();
    public IReadOnlyCollection<RoleSummary> Roles { get; private set; } = Array.Empty<RoleSummary>();
    public IReadOnlyCollection<PermissionSummary> Permissions { get; private set; } = Array.Empty<PermissionSummary>();
    public Dictionary<int, IReadOnlyCollection<LoginHistorySummary>> LoginHistoryByUserId { get; private set; } = new();
    public bool CanCreateUser { get; private set; }
    public bool CanUpdateUser { get; private set; }
    public bool CanAssignRoles { get; private set; }
    public bool CanAssignPermissions { get; private set; }
    public bool CanResetPassword { get; private set; }
    public bool CanLockUser { get; private set; }
    public bool CanDeactivateUser { get; private set; }

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

        if (await _userManagementService.IsUsernameExistsAsync(CreateInput.Username, cancellationToken: cancellationToken))
            ModelState.AddModelError("CreateInput.Username", "Username already exists.");

        if (!string.IsNullOrWhiteSpace(CreateInput.Email) &&
            await _userManagementService.IsEmailExistsAsync(CreateInput.Email, cancellationToken: cancellationToken))
            ModelState.AddModelError("CreateInput.Email", "Email already exists.");

        var imageUrl = await SaveProfileImageAsync(CreateInput.ProfileImage, "CreateInput.ProfileImage", cancellationToken);
        if (!ModelState.IsValid)
        {
            await LoadPageAsync(cancellationToken);
            return Page();
        }

        var userId = await _userManagementService.CreateUserAsync(
            CreateInput.Username,
            CreateInput.DisplayName,
            CreateInput.Email ?? string.Empty,
            CreateInput.Password,
            string.Join(',', CreateInput.SelectedRoles),
            CurrentUserId(),
            cancellationToken);

        if (!CreateInput.IsActive)
            await _userManagementService.DeactivateUserAsync(userId, CurrentUserId(), cancellationToken);

        if (!string.IsNullOrWhiteSpace(imageUrl))
            await _userManagementService.UpdateProfileImageAsync(userId, imageUrl, cancellationToken);

        StatusMessage = "Employee account created.";
        return RedirectToPage(CurrentRouteValues());
    }

    public async Task<IActionResult> OnPostUpdateAsync(CancellationToken cancellationToken)
    {
        ModelState.Clear();
        TryValidateModel(EditInput, nameof(EditInput));

        if (!string.IsNullOrWhiteSpace(EditInput.Email) &&
            await _userManagementService.IsEmailExistsAsync(EditInput.Email, EditInput.UserId, cancellationToken))
            ModelState.AddModelError("EditInput.Email", "Email already exists.");

        var imageUrl = await SaveProfileImageAsync(EditInput.ProfileImage, "EditInput.ProfileImage", cancellationToken);
        if (!ModelState.IsValid)
        {
            await LoadPageAsync(cancellationToken);
            return Page();
        }

        await _userManagementService.UpdateUserAsync(
            EditInput.UserId,
            EditInput.DisplayName,
            EditInput.Email ?? string.Empty,
            EditInput.IsActive,
            string.Join(',', EditInput.SelectedRoles),
            CurrentUserId(),
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(imageUrl))
            await _userManagementService.UpdateProfileImageAsync(EditInput.UserId, imageUrl, cancellationToken);

        StatusMessage = "Employee updated.";
        return RedirectToPage(CurrentRouteValues());
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int userId, CancellationToken cancellationToken)
    {
        await _userManagementService.DeactivateUserAsync(userId, CurrentUserId(), cancellationToken);
        StatusMessage = "Employee deactivated.";
        return RedirectToPage(CurrentRouteValues());
    }

    public async Task<IActionResult> OnPostResetPasswordAsync(CancellationToken cancellationToken)
    {
        ModelState.Clear();
        TryValidateModel(ResetPasswordInput, nameof(ResetPasswordInput));

        if (!ModelState.IsValid)
        {
            await LoadPageAsync(cancellationToken);
            return Page();
        }

        await _userManagementService.ResetPasswordAsync(
            ResetPasswordInput.UserId,
            ResetPasswordInput.NewPassword,
            CurrentUserId(),
            cancellationToken);

        StatusMessage = "Password reset.";
        return RedirectToPage(CurrentRouteValues());
    }

    public async Task<IActionResult> OnPostLockAsync(CancellationToken cancellationToken)
    {
        await _userManagementService.SetUserLockAsync(
            LockInput.UserId,
            LockInput.IsLocked,
            LockInput.LockoutEndAtUtc,
            CurrentUserId(),
            cancellationToken);

        StatusMessage = LockInput.IsLocked ? "Employee locked." : "Employee unlocked.";
        return RedirectToPage(CurrentRouteValues());
    }

    public async Task<IActionResult> OnPostAssignRolesAsync(CancellationToken cancellationToken)
    {
        await _userManagementService.AssignRoleAsync(
            AssignmentInput.UserId,
            string.Join(',', AssignmentInput.SelectedValues),
            CurrentUserId(),
            cancellationToken);

        StatusMessage = "Roles assigned.";
        return RedirectToPage(CurrentRouteValues());
    }

    public async Task<IActionResult> OnPostAssignPermissionsAsync(CancellationToken cancellationToken)
    {
        await _userManagementService.AssignPermissionAsync(
            AssignmentInput.UserId,
            string.Join(',', AssignmentInput.SelectedValues),
            CurrentUserId(),
            cancellationToken);

        StatusMessage = "Permissions assigned.";
        return RedirectToPage(CurrentRouteValues());
    }

    private async Task LoadPageAsync(CancellationToken cancellationToken)
    {
        Roles = await _roleService.GetRolesAsync(cancellationToken);
        Permissions = await _permissionService.GetPermissionsAsync(cancellationToken);
        Users = await _userManagementService.GetUsersPagedAsync(
            new UserPagedRequest
            {
                PageNumber = PageNumber,
                PageSize = 10,
                SearchText = SearchText,
                RoleCode = RoleCode,
                IsActive = Status switch
                {
                    "active" => true,
                    "inactive" => false,
                    _ => null
                },
                IsLocked = LockStatus switch
                {
                    "locked" => true,
                    "unlocked" => false,
                    _ => null
                }
            },
            cancellationToken);

        var historyPairs = new Dictionary<int, IReadOnlyCollection<LoginHistorySummary>>();
        foreach (var user in Users.Users)
            historyPairs[user.UserId] = await _userManagementService.GetLoginHistoryAsync(user.UserId, cancellationToken);

        LoginHistoryByUserId = historyPairs;
        CanCreateUser = HasPermission("USER_CREATE");
        CanUpdateUser = HasPermission("USER_UPDATE");
        CanAssignRoles = HasPermission("USER_ASSIGN_ROLE");
        CanAssignPermissions = HasPermission("USER_ASSIGN_PERMISSION");
        CanResetPassword = HasPermission("USER_PASSWORD_RESET");
        CanLockUser = HasPermission("USER_LOCK");
        CanDeactivateUser = HasPermission("USER_DEACTIVATE");
    }

    private object CurrentRouteValues()
    {
        return new { SearchText, RoleCode, Status, LockStatus, PageNumber };
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

    private async Task<string?> SaveProfileImageAsync(IFormFile? image, string modelStateKey, CancellationToken cancellationToken)
    {
        if (image is null || image.Length == 0)
            return null;

        const long maxImageBytes = 2 * 1024 * 1024;
        if (image.Length > maxImageBytes)
        {
            ModelState.AddModelError(modelStateKey, "Profile image must be 2 MB or smaller.");
            return null;
        }

        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };
        var extension = Path.GetExtension(image.FileName);
        if (!allowedExtensions.Contains(extension))
        {
            ModelState.AddModelError(modelStateKey, "Use a JPG, PNG, or WebP profile image.");
            return null;
        }

        var uploadRoot = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
        Directory.CreateDirectory(uploadRoot);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadRoot, fileName);
        await using var stream = System.IO.File.Create(filePath);
        await image.CopyToAsync(stream, cancellationToken);

        return $"/uploads/profiles/{fileName}";
    }

    public sealed class UserCreateInput
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Display Name")]
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(254)]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public List<string> SelectedRoles { get; set; } = new();
        public IFormFile? ProfileImage { get; set; }
    }

    public sealed class UserEditInput
    {
        public int UserId { get; set; }

        [Required]
        [Display(Name = "Display Name")]
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(254)]
        public string? Email { get; set; }

        public bool IsActive { get; set; }
        public List<string> SelectedRoles { get; set; } = new();
        public IFormFile? ProfileImage { get; set; }
    }

    public sealed class PasswordResetInput
    {
        public int UserId { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword))]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public sealed class UserLockInput
    {
        public int UserId { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockoutEndAtUtc { get; set; }
    }

    public sealed class AssignmentFormInput
    {
        public int UserId { get; set; }
        public List<string> SelectedValues { get; set; } = new();
    }
}
