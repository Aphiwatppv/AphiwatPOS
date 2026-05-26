namespace AuthenticationEngine.Models;

public sealed record AuthResult(
    bool Succeeded,
    AuthenticatedUser? User,
    string? ErrorMessage)
{
    public static AuthResult Success(AuthenticatedUser user) => new(true, user, null);

    public static AuthResult Failure(string message) => new(false, null, message);
}
