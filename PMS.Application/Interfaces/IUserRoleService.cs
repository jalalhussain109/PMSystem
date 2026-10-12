using System.Collections.Generic;
using System.Threading.Tasks;
using PMS.Application.Common;
using PMS.Application.DTOs;

namespace PMS.Application.Interfaces
{
    public interface IUserRoleService
    {
        Task<IEnumerable<object>> GetRolesWithPermissionsAsync();
        Task<bool> CreateRoleAsync(string roleName);
        Task<ServiceResult<bool>> RenameRoleAsync(string currentName, string newName);
        Task<ServiceResult<bool>> DeleteRoleAsync(string roleName);
        Task<bool> AddPermissionToRoleAsync(string roleName, string permission);
        Task<bool> RemovePermissionFromRoleAsync(string roleName, string permission);
        Task<bool> AssignRoleToUserAsync(long userId, string roleName);
        Task<bool> RemoveRoleFromUserAsync(long userId, string roleName);
        Task<(string[] roles, string[] permissions)> GetUserEffectivePermissionsAsync(long userId);
        Task<PagedResult<UserPermissionsDto>> GetUsersPagedAsync(int page, int pageSize, string? search);
        Task<IEnumerable<UserPermissionsDto>> GetPermissionsMatrixAsync();
    }
}
