using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AphiwatPOS.Pages.Account;

public sealed class LogoutModel : PageModel
{
    private readonly AuthenticationEngine.Services.IAuthenticationService _authenticationService;

    public LogoutModel(AuthenticationEngine.Services.IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var loginHistoryIdValue = User.FindFirst("LoginHistoryId")?.Value;
        if (long.TryParse(loginHistoryIdValue, out var loginHistoryId))
            await _authenticationService.SignOutAsync(loginHistoryId, cancellationToken);

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Account/Login");
    }
}
