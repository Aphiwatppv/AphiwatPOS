namespace AuthenticationEngine.Models;

public sealed class RoleSummary
{
    public int RoleId { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Permissions { get; set; } = string.Empty;
}
