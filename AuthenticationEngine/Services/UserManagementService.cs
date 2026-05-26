using AccessEngine.Services;
using AuthenticationEngine.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationEngine.Services;

public sealed class UserManagementService : IUserManagementService
{
    private readonly IAccessService _accessService;
    private readonly PasswordHasher<UserCredentialRecord> _passwordHasher = new();

    public UserManagementService(IAccessService accessService)
    {
        _accessService = accessService;
    }

    public async Task<IReadOnlyCollection<UserSummary>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _accessService.QueryAsync<UserSummary, object>(
            "dbo.Access_User_List",
            new { },
            cancellationToken);

        return users.ToArray();
    }

    public async Task<UserPagedResult> GetUsersPagedAsync(UserPagedRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 200);

        return await _accessService.QueryMultipleAsync<object, UserPagedResult>(
            "dbo.Access_User_ListPaged",
            new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchText = request.SearchText?.Trim(),
                request.IsActive,
                request.IsLocked,
                RoleCode = request.RoleCode?.Trim()
            },
            async reader =>
            {
                var users = (await reader.ReadAsync<UserSummary>()).ToArray();
                var totalCount = await reader.ReadSingleAsync<int>();
                return new UserPagedResult
                {
                    Users = users,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            },
            cancellationToken);
    }

    public Task<UserSummary?> GetUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _accessService.QuerySingleOrDefaultAsync<UserSummary, object>(
            "dbo.Access_User_Get",
            new { UserId = userId },
            cancellationToken);
    }

    public async Task<int> CreateUserAsync(
        string username,
        string displayName,
        string email,
        string password,
        string roles,
        int createdByUserId,
        CancellationToken cancellationToken = default)
    {
        var passwordHash = _passwordHasher.HashPassword(new UserCredentialRecord { Username = username }, password);

        return await _accessService.QuerySingleAsync<int, object>(
            "dbo.Access_User_Create",
            new
            {
                Username = username.Trim(),
                DisplayName = displayName.Trim(),
                Email = email.Trim(),
                PasswordHash = passwordHash,
                Roles = NormalizeRoles(roles),
                CreatedByUserId = createdByUserId
            },
            cancellationToken);
    }

    public Task UpdateUserAsync(
        int userId,
        string displayName,
        string email,
        bool isActive,
        string roles,
        int updatedByUserId,
        CancellationToken cancellationToken = default)
    {
        return _accessService.ExecuteAsync(
            "dbo.Access_User_Update",
            new
            {
                UserId = userId,
                DisplayName = displayName.Trim(),
                Email = email.Trim(),
                IsActive = isActive,
                Roles = NormalizeRoles(roles),
                UpdatedByUserId = updatedByUserId
            },
            cancellationToken);
    }

    public Task UpdateProfileAsync(int userId, string displayName, string email, CancellationToken cancellationToken = default)
    {
        return _accessService.ExecuteAsync(
            "dbo.Access_User_UpdateProfile",
            new { UserId = userId, DisplayName = displayName.Trim(), Email = email.Trim() },
            cancellationToken);
    }

    public Task UpdateProfileImageAsync(int userId, string imageUrl, CancellationToken cancellationToken = default)
    {
        return _accessService.ExecuteAsync(
            "dbo.Access_User_UpdateProfileImage",
            new { UserId = userId, ProfileImageUrl = imageUrl.Trim() },
            cancellationToken);
    }

    public Task DeactivateUserAsync(int userId, int updatedByUserId, CancellationToken cancellationToken = default)
    {
        return _accessService.ExecuteAsync(
            "dbo.Access_User_SetActive",
            new { UserId = userId, IsActive = false, UpdatedByUserId = updatedByUserId },
            cancellationToken);
    }

    public Task ResetPasswordAsync(int userId, string password, int updatedByUserId, CancellationToken cancellationToken = default)
    {
        var passwordHash = _passwordHasher.HashPassword(new UserCredentialRecord { UserId = userId }, password);

        return _accessService.ExecuteAsync(
            "dbo.Access_User_ResetPassword",
            new { UserId = userId, PasswordHash = passwordHash, UpdatedByUserId = updatedByUserId },
            cancellationToken);
    }

    public Task AssignRoleAsync(int userId, string roles, int updatedByUserId, CancellationToken cancellationToken = default)
    {
        return _accessService.ExecuteAsync(
            "dbo.Access_User_AssignRoles",
            new { UserId = userId, Roles = NormalizeList(roles), UpdatedByUserId = updatedByUserId },
            cancellationToken);
    }

    public Task AssignPermissionAsync(int userId, string permissions, int updatedByUserId, CancellationToken cancellationToken = default)
    {
        return _accessService.ExecuteAsync(
            "dbo.Access_User_AssignPermissions",
            new { UserId = userId, Permissions = NormalizeList(permissions), UpdatedByUserId = updatedByUserId },
            cancellationToken);
    }

    public Task SetUserLockAsync(
        int userId,
        bool isLocked,
        DateTime? lockoutEndAtUtc,
        int updatedByUserId,
        CancellationToken cancellationToken = default)
    {
        return _accessService.ExecuteAsync(
            "dbo.Access_User_SetLock",
            new { UserId = userId, IsLocked = isLocked, LockoutEndAtUtc = lockoutEndAtUtc, UpdatedByUserId = updatedByUserId },
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<LoginHistorySummary>> GetLoginHistoryAsync(int? userId = null, CancellationToken cancellationToken = default)
    {
        var history = await _accessService.QueryAsync<LoginHistorySummary, object>(
            "dbo.Access_LoginHistory_List",
            new { UserId = userId },
            cancellationToken);

        return history.ToArray();
    }

    public Task<bool> IsUsernameExistsAsync(string username, int? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        return _accessService.QuerySingleAsync<bool, object>(
            "dbo.Access_User_UsernameExists",
            new { Username = username.Trim(), ExcludeUserId = excludeUserId },
            cancellationToken);
    }

    public Task<bool> IsEmailExistsAsync(string email, int? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        return _accessService.QuerySingleAsync<bool, object>(
            "dbo.Access_User_EmailExists",
            new { Email = email.Trim(), ExcludeUserId = excludeUserId },
            cancellationToken);
    }

    private static string NormalizeRoles(string roles)
    {
        return NormalizeList(roles);
    }

    private static string NormalizeList(string values)
    {
        return string.Join(
            ',',
            values.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase));
    }
}
