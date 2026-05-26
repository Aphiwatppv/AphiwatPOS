using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AuthService = AuthenticationEngine.Services.IAuthenticationService;

namespace AphiwatPOS.Pages.Account;

[AllowAnonymous]
public sealed class LoginModel : PageModel
{
    private readonly AuthService _authenticationService;

    public LoginModel(AuthService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await _authenticationService.SignInAsync(Input.Username, Input.Password, cancellationToken);
        if (!result.Succeeded || result.User is null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to sign in.");
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.User.UserId.ToString()),
            new(ClaimTypes.Name, result.User.Username),
            new("DisplayName", result.User.DisplayName),
            new("ProfileImageUrl", result.User.ProfileImageUrl)
        };

        claims.AddRange(result.User.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(result.User.Permissions.Select(permission => new Claim("Permission", permission)));
        claims.Add(new Claim("LoginHistoryId", result.User.LoginHistoryId.ToString()));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = Input.RememberMe });

        if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            return LocalRedirect(ReturnUrl);

        return RedirectToPage("/Index");
    }

    public sealed class LoginInput
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
