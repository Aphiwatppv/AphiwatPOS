using System.Security.Claims;

namespace AphiwatPOS.Pages.Customer;

internal static class CustomerPageHelpers
{
    public static int CurrentUserId(ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0 ? id : 1;

    public static string Money(decimal value) => $"THB {value:N2}";
    public static string Date(DateTime? value) => value?.ToString("yyyy-MM-dd") ?? "-";
    public static string DateTimeText(DateTime? value) => value?.ToString("yyyy-MM-dd HH:mm") ?? "-";
}
