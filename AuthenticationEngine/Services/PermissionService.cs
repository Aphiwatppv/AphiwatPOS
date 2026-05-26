using AccessEngine.Services;
using AuthenticationEngine.Models;

namespace AuthenticationEngine.Services;

public sealed class PermissionService : IPermissionService
{
    private readonly IAccessService _accessService;

    public PermissionService(IAccessService accessService)
    {
        _accessService = accessService;
    }

    public async Task<IReadOnlyCollection<PermissionSummary>> GetPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await _accessService.QueryAsync<PermissionSummary, object>(
            "dbo.Access_Permission_List",
            new { },
            cancellationToken);

        return permissions.ToArray();
    }

    public Task<PermissionSummary?> GetPermissionAsync(int permissionId, CancellationToken cancellationToken = default)
    {
        return _accessService.QuerySingleOrDefaultAsync<PermissionSummary, object>(
            "dbo.Access_Permission_Get",
            new { PermissionId = permissionId },
            cancellationToken);
    }

    public Task<int> CreatePermissionAsync(string permissionCode, string permissionName, string? moduleName, string? description, int createdByUserId, CancellationToken cancellationToken = default)
    {
        return _accessService.QuerySingleAsync<int, object>(
            "dbo.Access_Permission_Create",
            new
            {
                PermissionCode = permissionCode.Trim(),
                PermissionName = permissionName.Trim(),
                ModuleName = moduleName?.Trim() ?? string.Empty,
                Description = description?.Trim() ?? string.Empty,
                CreatedByUserId = createdByUserId
            },
            cancellationToken);
    }

    public Task UpdatePermissionAsync(int permissionId, string permissionName, string? moduleName, string? description, bool isActive, int updatedByUserId, CancellationToken cancellationToken = default)
    {
        return _accessService.ExecuteAsync(
            "dbo.Access_Permission_Update",
            new
            {
                PermissionId = permissionId,
                PermissionName = permissionName.Trim(),
                ModuleName = moduleName?.Trim() ?? string.Empty,
                Description = description?.Trim() ?? string.Empty,
                IsActive = isActive,
                UpdatedByUserId = updatedByUserId
            },
            cancellationToken);
    }

    public Task DeactivatePermissionAsync(int permissionId, int updatedByUserId, CancellationToken cancellationToken = default)
    {
        return _accessService.ExecuteAsync(
            "dbo.Access_Permission_SetActive",
            new { PermissionId = permissionId, IsActive = false, UpdatedByUserId = updatedByUserId },
            cancellationToken);
    }

    public Task<bool> IsPermissionCodeExistsAsync(string permissionCode, int? excludePermissionId = null, CancellationToken cancellationToken = default)
    {
        return _accessService.QuerySingleAsync<bool, object>(
            "dbo.Access_Permission_CodeExists",
            new { PermissionCode = permissionCode.Trim(), ExcludePermissionId = excludePermissionId },
            cancellationToken);
    }
}
