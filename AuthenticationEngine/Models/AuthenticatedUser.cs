namespace AuthenticationEngine.Models;

public sealed record AuthenticatedUser(
    int UserId,
    string Username,
    string DisplayName,
    string ProfileImageUrl,
    string[] Roles,
    string[] Permissions,
    long LoginHistoryId);
