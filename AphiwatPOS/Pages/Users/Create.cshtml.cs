using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AuthenticationEngine.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AphiwatPOS.Pages.Users;

[Authorize(Roles = "Admin")]
public sealed class CreateModel : PageModel
{
    private readonly IUserManagementService _userManagementService;

    public CreateModel(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [BindProperty]
    public UserInput Input { get; set; } = new();

    public void OnGet()
    {
        Input.Roles = "Cashier";
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        await _userManagementService.CreateUserAsync(
            Input.Username,
            Input.DisplayName,
            Input.Email ?? string.Empty,
            Input.Password,
            Input.Roles,
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
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

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

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;
    }
}
