using AuthenticationEngine.Models;
using AuthenticationEngine.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AphiwatPOS.Pages.Users;

[Authorize(Roles = "Admin")]
public sealed class IndexModel : PageModel
{
    private readonly IUserManagementService _userManagementService;

    public IndexModel(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    public IReadOnlyCollection<UserSummary> Users { get; private set; } = Array.Empty<UserSummary>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Users = await _userManagementService.GetUsersAsync(cancellationToken);
    }
}
