namespace AuthenticationEngine.Models;

public sealed class UserPagedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchText { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsLocked { get; set; }
    public string? RoleCode { get; set; }
}
