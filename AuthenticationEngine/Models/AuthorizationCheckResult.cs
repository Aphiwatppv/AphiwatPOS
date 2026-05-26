namespace AuthenticationEngine.Models;

public sealed record AuthorizationCheckResult(bool IsAuthorized, string PermissionCode);
