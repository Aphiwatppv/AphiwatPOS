using AuthenticationEngine.Models;

namespace AuthenticationEngine.Services;

public interface IUserManagementService
{
    Task<UserPagedResult> GetUsersPagedAsync(UserPagedRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserSummary>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<UserSummary?> GetUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> CreateUserAsync(string username, string displayName, string email, string password, string roles, int createdByUserId, CancellationToken cancellationToken = default);
    Task UpdateUserAsync(int userId, string displayName, string email, bool isActive, string roles, int updatedByUserId, CancellationToken cancellationToken = default);
    Task UpdateProfileAsync(int userId, string displayName, string email, CancellationToken cancellationToken = default);
    Task UpdateProfileImageAsync(int userId, string imageUrl, CancellationToken cancellationToken = default);
    Task DeactivateUserAsync(int userId, int updatedByUserId, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(int userId, string password, int updatedByUserId, CancellationToken cancellationToken = default);
    Task AssignRoleAsync(int userId, string roles, int updatedByUserId, CancellationToken cancellationToken = default);
    Task AssignPermissionAsync(int userId, string permissions, int updatedByUserId, CancellationToken cancellationToken = default);
    Task SetUserLockAsync(int userId, bool isLocked, DateTime? lockoutEndAtUtc, int updatedByUserId, CancellationToken cancellationToken = default);
    Task<bool> IsUsernameExistsAsync(string username, int? excludeUserId = null, CancellationToken cancellationToken = default);
    Task<bool> IsEmailExistsAsync(string email, int? excludeUserId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<LoginHistorySummary>> GetLoginHistoryAsync(int? userId = null, CancellationToken cancellationToken = default);
}
