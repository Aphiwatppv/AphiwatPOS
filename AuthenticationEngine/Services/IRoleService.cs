using AuthenticationEngine.Models;

namespace AuthenticationEngine.Services;

public interface IRoleService
{
    Task<IReadOnlyCollection<RoleSummary>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<RoleSummary?> GetRoleAsync(int roleId, CancellationToken cancellationToken = default);
    Task<int> CreateRoleAsync(string roleCode, string roleName, string? description, int createdByUserId, CancellationToken cancellationToken = default);
    Task UpdateRoleAsync(int roleId, string roleName, string? description, bool isActive, int updatedByUserId, CancellationToken cancellationToken = default);
    Task DeactivateRoleAsync(int roleId, int updatedByUserId, CancellationToken cancellationToken = default);
    Task AssignPermissionsToRoleAsync(int roleId, string permissions, int updatedByUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PermissionSummary>> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default);
    Task<bool> IsRoleCodeExistsAsync(string roleCode, int? excludeRoleId = null, CancellationToken cancellationToken = default);
}
