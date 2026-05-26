using AuthenticationEngine.Models;

namespace AuthenticationEngine.Services;

public interface IPermissionService
{
    Task<IReadOnlyCollection<PermissionSummary>> GetPermissionsAsync(CancellationToken cancellationToken = default);
    Task<PermissionSummary?> GetPermissionAsync(int permissionId, CancellationToken cancellationToken = default);
    Task<int> CreatePermissionAsync(string permissionCode, string permissionName, string? moduleName, string? description, int createdByUserId, CancellationToken cancellationToken = default);
    Task UpdatePermissionAsync(int permissionId, string permissionName, string? moduleName, string? description, bool isActive, int updatedByUserId, CancellationToken cancellationToken = default);
    Task DeactivatePermissionAsync(int permissionId, int updatedByUserId, CancellationToken cancellationToken = default);
    Task<bool> IsPermissionCodeExistsAsync(string permissionCode, int? excludePermissionId = null, CancellationToken cancellationToken = default);
}
