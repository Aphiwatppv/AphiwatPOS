using AccessEngine.Services;
using AuthenticationEngine.Models;

namespace AuthenticationEngine.Services;

public sealed class RoleService : IRoleService
{
    private readonly IAccessService _accessService;

    public RoleService(IAccessService accessService)
    {
        _accessService = accessService;
    }

    public async Task<IReadOnlyCollection<RoleSummary>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _accessService.QueryAsync<RoleSummary, object>(
            "dbo.Access_Role_List",
            new { },
            cancellationToken);

        return roles.ToArray();
    }

    public Task<RoleSummary?> GetRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return _accessService.QuerySingleOrDefaultAsync<RoleSummary, object>(
            "dbo.Access_Role_Get",
            new { RoleId = roleId },
            cancellationToken);
    }

    public Task<int> CreateRoleAsync(string roleCode, string roleName, string? description, int createdByUserId, CancellationToken cancellationToken = default)
    {
        return _accessService.QuerySingleAsync<int, object>(
            "dbo.Access_Role_Create",
            new
            {
                RoleCode = roleCode.Trim(),
                RoleName = roleName.Trim(),
                Description = description?.Trim() ?? string.Empty,
                CreatedByUserId = createdByUserId
            },
            cancellationToken);
    }

    public Task UpdateRoleAsync(int roleId, string roleName, string? description, bool isActive, int updatedByUserId, CancellationToken cancellationToken = default)
    {
        return _accessService.ExecuteAsync(
            "dbo.Access_Role_Update",
            new
            {
                RoleId = roleId,
                RoleName = roleName.Trim(),
                Description = description?.Trim() ?? string.Empty,
                IsActive = isActive,
                UpdatedByUserId = updatedByUserId
            },
            cancellationToken);
    }

    public Task DeactivateRoleAsync(int roleId, int updatedByUserId, CancellationToken cancellationToken = default)
    {
        return _accessService.ExecuteAsync(
            "dbo.Access_Role_SetActive",
            new { RoleId = roleId, IsActive = false, UpdatedByUserId = updatedByUserId },
            cancellationToken);
    }

    public Task AssignPermissionsToRoleAsync(int roleId, string permissions, int updatedByUserId, CancellationToken cancellationToken = default)
    {
        return _accessService.ExecuteAsync(
            "dbo.Access_Role_AssignPermissions",
            new { RoleId = roleId, Permissions = NormalizeList(permissions), UpdatedByUserId = updatedByUserId },
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<PermissionSummary>> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var permissions = await _accessService.QueryAsync<PermissionSummary, object>(
            "dbo.Access_Role_PermissionList",
            new { RoleId = roleId },
            cancellationToken);

        return permissions.ToArray();
    }

    public Task<bool> IsRoleCodeExistsAsync(string roleCode, int? excludeRoleId = null, CancellationToken cancellationToken = default)
    {
        return _accessService.QuerySingleAsync<bool, object>(
            "dbo.Access_Role_CodeExists",
            new { RoleCode = roleCode.Trim(), ExcludeRoleId = excludeRoleId },
            cancellationToken);
    }

    private static string NormalizeList(string values)
    {
        return string.Join(
            ',',
            values.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase));
    }
}
