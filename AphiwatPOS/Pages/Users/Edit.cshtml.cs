using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AuthenticationEngine.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AphiwatPOS.Pages.Users;

[Authorize(Roles = "Admin")]
public sealed class EditModel : PageModel
{
    private readonly IUserManagementService _userManagementService;

    public EditModel(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [BindProperty]
    public UserInput Input { get; set; } = new();

    [BindProperty]
    public PasswordResetInput PasswordInput { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        var user = await _userManagementService.GetUserAsync(id, cancellationToken);
        if (user is null)
            return NotFound();

        Input = new UserInput
        {
            UserId = user.UserId,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Roles = user.Roles,
            IsActive = user.IsActive
        };
        PasswordInput.UserId = user.UserId;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        await _userManagementService.UpdateUserAsync(
            Input.UserId,
            Input.DisplayName,
            Input.Email ?? string.Empty,
            Input.IsActive,
            Input.Roles,
            CurrentUserId(),
            cancellationToken);

        return RedirectToPage("/Users/Index");
    }

    public async Task<IActionResult> OnPostPasswordAsync(CancellationToken cancellationToken)
    {
        ModelState.ClearValidationState(nameof(Input));
        if (!TryValidateModel(PasswordInput, nameof(PasswordInput)))
            return Page();

        await _userManagementService.ResetPasswordAsync(
            PasswordInput.UserId,
            PasswordInput.Password,
            CurrentUserId(),
            cancellationToken);

        return RedirectToPage("/Users/Index");
    }

    private int CurrentUserId()
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : 0;
    }

    public sealed class UserInput
    {
        public int UserId { get; set; }

        [Required]
        [Display(Name = "Display name")]
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(254)]
        public string? Email { get; set; }

        [Required]
        [StringLength(200)]
        public string Roles { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }

    public sealed class PasswordResetInput
    {
        public int UserId { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8)]
        [Display(Name = "New password")]
        public string Password { get; set; } = string.Empty;
    }
}
