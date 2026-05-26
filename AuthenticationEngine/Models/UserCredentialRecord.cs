namespace AuthenticationEngine.Models;

public sealed class UserCredentialRecord
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockoutEndAtUtc { get; set; }
    public string Roles { get; set; } = string.Empty;
    public string Permissions { get; set; } = string.Empty;
}
