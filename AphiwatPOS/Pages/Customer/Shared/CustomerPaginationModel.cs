namespace AphiwatPOS.Pages.Customer.Shared;

public sealed class CustomerPaginationModel
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public Func<int, string> PageUrl { get; init; } = _ => "#";
}
