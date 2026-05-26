using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AuthenticationEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AuthService = AuthenticationEngine.Services.IAuthenticationService;

namespace AphiwatPOS.Pages.Account;

public sealed class ProfileModel : PageModel
{
    private readonly IWebHostEnvironment _environment;
    private readonly AuthService _authenticationService;
    private readonly ILogger<ProfileModel> _logger;
    private readonly IUserManagementService _userManagementService;

    public ProfileModel(
        IWebHostEnvironment environment,
        AuthService authenticationService,
        ILogger<ProfileModel> logger,
        IUserManagementService userManagementService)
    {
        _environment = environment;
        _authenticationService = authenticationService;
        _logger = logger;
        _userManagementService = userManagementService;
    }

    [BindProperty]
    public ProfileInput Profile { get; set; } = new();

    [BindProperty]
    public PasswordInput Password { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        return await LoadProfileAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostProfileAsync(CancellationToken cancellationToken)
    {
        RemoveModelStateFor(nameof(Password));

        var postedImage = GetPostedProfileImage();
        var imageUrl = await SaveProfileImageAsync(postedImage, cancellationToken);
        if (HasModelStateErrorsFor(nameof(Profile)))
            return await LoadProfileAsync(cancellationToken);

        if (!TryValidateModel(Profile, nameof(Profile)))
            return await LoadProfileAsync(cancellationToken);

        var userId = CurrentUserId();
        if (!string.IsNullOrWhiteSpace(Profile.Email) &&
            await _userManagementService.IsEmailExistsAsync(Profile.Email, userId, cancellationToken))
        {
            ModelState.AddModelError("Profile.Email", "Email already exists.");
            return await LoadProfileAsync(cancellationToken);
        }

        await _userManagementService.UpdateProfileAsync(userId, Profile.DisplayName, Profile.Email ?? string.Empty, cancellationToken);
        if (!string.IsNullOrWhiteSpace(imageUrl))
            await _userManagementService.UpdateProfileImageAsync(userId, imageUrl, cancellationToken);
        else
            await _userManagementService.UpdateProfileImageAsync(userId, Profile.ProfileImageUrl ?? string.Empty, cancellationToken);

        StatusMessage = "Profile updated.";
        return RedirectToPage();
    }

    public Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        return OnPostProfileAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostPasswordAsync(CancellationToken cancellationToken)
    {
        RemoveModelStateFor(nameof(Profile));
        if (!TryValidateModel(Password, nameof(Password)))
            return await LoadProfileAsync(cancellationToken);

        var changed = await _authenticationService.ChangePasswordAsync(
            CurrentUserId(),
            Password.CurrentPassword,
            Password.NewPassword,
            CurrentUserId(),
            cancellationToken);

        if (!changed)
        {
            ModelState.AddModelError(string.Empty, "Current password is incorrect.");
            return await LoadProfileAsync(cancellationToken);
        }

        StatusMessage = "Password changed.";
        return RedirectToPage();
    }

    private async Task<IActionResult> LoadProfileAsync(CancellationToken cancellationToken)
    {
        var user = await _userManagementService.GetUserAsync(CurrentUserId(), cancellationToken);
        if (user is null)
            return NotFound();

        Profile = new ProfileInput
        {
            Username = user.Username,
            DisplayName = user.DisplayName,
            Email = user.Email,
            ProfileImageUrl = user.ProfileImageUrl,
            Roles = user.Roles,
            IsActive = user.IsActive,
            IsLocked = user.IsLocked,
            CreatedAtUtc = user.CreatedAtUtc,
            LastLoginAtUtc = user.LastLoginAtUtc
        };

        return Page();
    }

    private int CurrentUserId()
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : 0;
    }

    private async Task<string?> SaveProfileImageAsync(IFormFile? image, CancellationToken cancellationToken)
    {
        if (image is null || image.Length == 0)
        {
            _logger.LogInformation("No profile image file was posted.");
            return null;
        }

        var extension = Path.GetExtension(image.FileName);
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowedExtensions.Contains(extension))
        {
            ModelState.AddModelError("Profile.ProfileImage", "Use a JPG, PNG, or WebP profile image.");
            return null;
        }

        const long maxImageBytes = 2 * 1024 * 1024;
        if (image.Length > maxImageBytes)
        {
            ModelState.AddModelError("Profile.ProfileImage", "Profile image must be 2 MB or smaller.");
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
        return Profile.ProfileImage ??
            Request.Form.Files.GetFile("Profile.ProfileImage") ??
            Request.Form.Files.FirstOrDefault();
    }

    private void RemoveModelStateFor(string prefix)
    {
        var keys = ModelState.Keys
            .Where(key => key.Equals(prefix, StringComparison.OrdinalIgnoreCase) ||
                key.StartsWith($"{prefix}.", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        foreach (var key in keys)
            ModelState.Remove(key);
    }

    private bool HasModelStateErrorsFor(string prefix)
    {
        return ModelState
            .Where(item => item.Key.Equals(prefix, StringComparison.OrdinalIgnoreCase) ||
                item.Key.StartsWith($"{prefix}.", StringComparison.OrdinalIgnoreCase))
            .Any(item => item.Value?.Errors.Count > 0);
    }

    public sealed class ProfileInput
    {
        public string Username { get; set; } = string.Empty;

        public string Roles { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public bool IsLocked { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? LastLoginAtUtc { get; set; }

        [Required]
        [Display(Name = "Display Name")]
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(254)]
        public string? Email { get; set; }

        [Display(Name = "Profile Image URL")]
        [StringLength(500)]
        public string? ProfileImageUrl { get; set; }

        [Display(Name = "Upload Profile Image")]
        public IFormFile? ProfileImage { get; set; }
    }

    public sealed class PasswordInput
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

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
}
