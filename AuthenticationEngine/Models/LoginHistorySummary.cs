namespace AuthenticationEngine.Models;

public sealed class LoginHistorySummary
{
    public long LoginHistoryId { get; set; }
    public int? UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool Succeeded { get; set; }
    public string FailureReason { get; set; } = string.Empty;
    public DateTime AttemptedAtUtc { get; set; }
    public DateTime? LogoutAtUtc { get; set; }
}
