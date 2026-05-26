using AccessEngine.Services;
using AuthenticationEngine.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationEngine.Services;

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly IAccessService _accessService;
    private readonly PasswordHasher<UserCredentialRecord> _passwordHasher = new();

    public AuthenticationService(IAccessService accessService)
    {
        _accessService = accessService;
    }

    public async Task<AuthResult> SignInAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return AuthResult.Failure("Username and password are required.");

        var normalizedUsername = username.Trim();

        var user = await _accessService.QuerySingleOrDefaultAsync<UserCredentialRecord, object>(
            "dbo.Access_User_GetForSignIn",
            new { Username = normalizedUsername },
            cancellationToken);

        if (user is null)
        {
            await RecordSignInAttemptAsync(null, normalizedUsername, false, "Invalid username.", cancellationToken);
            return AuthResult.Failure("Invalid username or password.");
        }

        if (!user.IsActive)
        {
            await RecordSignInAttemptAsync(user.UserId, user.Username, false, "Inactive account.", cancellationToken);
            return AuthResult.Failure("This account is inactive.");
        }

        if (user.IsLocked || user.LockoutEndAtUtc > DateTime.UtcNow)
        {
            await RecordSignInAttemptAsync(user.UserId, user.Username, false, "Locked account.", cancellationToken);
            return AuthResult.Failure("This account is locked.");
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verification == PasswordVerificationResult.Failed)
        {
            await RecordSignInAttemptAsync(user.UserId, user.Username, false, "Invalid password.", cancellationToken);
            return AuthResult.Failure("Invalid username or password.");
        }

        var loginHistoryId = await RecordSignInAttemptAsync(
            user.UserId,
            user.Username,
            true,
            string.Empty,
            cancellationToken);

        var roles = user.Roles
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var permissions = user.Permissions
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return AuthResult.Success(new AuthenticatedUser(user.UserId, user.Username, user.DisplayName, user.ProfileImageUrl, roles, permissions, loginHistoryId));
    }

    public Task SignOutAsync(long loginHistoryId, CancellationToken cancellationToken = default)
    {
        return _accessService.ExecuteAsync(
            "dbo.Access_LoginHistory_RecordLogout",
            new { LoginHistoryId = loginHistoryId },
            cancellationToken);
    }

    public async Task<bool> ChangePasswordAsync(
        int userId,
        string currentPassword,
        string newPassword,
        int updatedByUserId,
        CancellationToken cancellationToken = default)
    {
        var user = await _accessService.QuerySingleOrDefaultAsync<UserCredentialRecord, object>(
            "dbo.Access_User_GetCredential",
            new { UserId = userId },
            cancellationToken);

        if (user is null || !user.IsActive)
            return false;

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
        if (verification == PasswordVerificationResult.Failed)
            return false;

        var passwordHash = _passwordHasher.HashPassword(user, newPassword);
        await _accessService.ExecuteAsync(
            "dbo.Access_User_ChangePassword",
            new { UserId = userId, PasswordHash = passwordHash, UpdatedByUserId = updatedByUserId },
            cancellationToken);

        return true;
    }

    public async Task<AuthorizationCheckResult> HasPermissionAsync(
        int userId,
        string permissionCode,
        CancellationToken cancellationToken = default)
    {
        var isAuthorized = await _accessService.QuerySingleAsync<bool, object>(
            "dbo.Access_User_HasPermission",
            new { UserId = userId, PermissionCode = permissionCode.Trim() },
            cancellationToken);

        return new AuthorizationCheckResult(isAuthorized, permissionCode);
    }

    private Task<long> RecordSignInAttemptAsync(
        int? userId,
        string username,
        bool succeeded,
        string failureReason,
        CancellationToken cancellationToken)
    {
        return _accessService.QuerySingleAsync<long, object>(
            "dbo.Access_LoginHistory_RecordAttempt",
            new
            {
                UserId = userId,
                Username = username,
                Succeeded = succeeded,
                FailureReason = failureReason
            },
            cancellationToken);
    }
}
