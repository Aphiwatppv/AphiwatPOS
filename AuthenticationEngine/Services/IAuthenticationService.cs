using AuthenticationEngine.Models;

namespace AuthenticationEngine.Services;

public interface IAuthenticationService
{
    Task<AuthResult> SignInAsync(string username, string password, CancellationToken cancellationToken = default);
    Task SignOutAsync(long loginHistoryId, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword, int updatedByUserId, CancellationToken cancellationToken = default);
    Task<AuthorizationCheckResult> HasPermissionAsync(int userId, string permissionCode, CancellationToken cancellationToken = default);
}
