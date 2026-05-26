using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AuthenticationEngine.Models;
using AuthenticationEngine.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AphiwatPOS.Pages.Account;

[Authorize(Roles = "Admin,Manager")]
public sealed class RegisterModel : PageModel
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<RegisterModel> _logger;
    private readonly IRoleService _roleService;
    private readonly IUserManagementService _userManagementService;

    public RegisterModel(
        IWebHostEnvironment environment,
        ILogger<RegisterModel> logger,
        IRoleService roleService,
        IUserManagementService userManagementService)
    {
        _environment = environment;
        _logger = logger;
        _roleService = roleService;
        _userManagementService = userManagementService;
    }

    [BindProperty]
    public RegisterInput Input { get; set; } = new();

    public IReadOnlyCollection<RoleSummary> Roles { get; private set; } = Array.Empty<RoleSummary>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Input.IsActive = true;
        await LoadRolesAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadRolesAsync(cancellationToken);

        var imageUrl = await SaveProfileImageAsync(GetPostedProfileImage(), cancellationToken);

        if (await _userManagementService.IsUsernameExistsAsync(Input.Username, cancellationToken: cancellationToken))
            ModelState.AddModelError("Input.Username", "Username already exists.");

        if (!string.IsNullOrWhiteSpace(Input.Email) &&
            await _userManagementService.IsEmailExistsAsync(Input.Email, cancellationToken: cancellationToken))
            ModelState.AddModelError("Input.Email", "Email already exists.");

        if (!ModelState.IsValid)
            return Page();

        var userId = await _userManagementService.CreateUserAsync(
            Input.Username,
            Input.DisplayName,
            Input.Email ?? string.Empty,
            Input.Password,
            string.Join(',', Input.SelectedRoles),
            CurrentUserId(),
            cancellationToken);

        if (!Input.IsActive)
            await _userManagementService.DeactivateUserAsync(userId, CurrentUserId(), cancellationToken);

        if (!string.IsNullOrWhiteSpace(imageUrl))
            await _userManagementService.UpdateProfileImageAsync(userId, imageUrl, cancellationToken);

        return RedirectToPage("/Employees/Employee/Index");
    }

    private async Task LoadRolesAsync(CancellationToken cancellationToken)
    {
        Roles = (await _roleService.GetRolesAsync(cancellationToken))
            .Where(role => role.IsActive)
            .ToArray();
    }

    private int CurrentUserId()
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : 0;
    }

    private async Task<string?> SaveProfileImageAsync(IFormFile? image, CancellationToken cancellationToken)
    {
        if (image is null || image.Length == 0)
        {
            _logger.LogInformation("No register profile image file was posted.");
            return null;
        }

        var extension = Path.GetExtension(image.FileName);
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowedExtensions.Contains(extension))
        {
            ModelState.AddModelError("Input.ProfileImage", "Use a JPG, PNG, or WebP profile image.");
            return null;
        }

        const long maxImageBytes = 2 * 1024 * 1024;
        if (image.Length > maxImageBytes)
        {
            ModelState.AddModelError("Input.ProfileImage", "Profile image must be 2 MB or smaller.");
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

    private IFormFile? GetPostedProfileImage()
    {
        return Input.ProfileImage ??
            Request.Form.Files.GetFile("Input.ProfileImage") ??
            Request.Form.Files.FirstOrDefault();
    }

    public sealed class RegisterInput
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

        [MinLength(1, ErrorMessage = "Select at least one role.")]
        public List<string> SelectedRoles { get; set; } = new();

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        public IFormFile? ProfileImage { get; set; }
    }
}
