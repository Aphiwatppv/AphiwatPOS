namespace AuthenticationEngine.Models;

public sealed class UserPagedResult
{
    public IReadOnlyCollection<UserSummary> Users { get; init; } = Array.Empty<UserSummary>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}
