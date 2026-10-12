using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PMS.Application.Common;
using PMS.Application.DTOs;
using PMS.Application.Interfaces;
using PMS.Domain.Entities;

namespace PMS.Infrastructure.Identity
{
    public class UserRoleService : IUserRoleService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<long>> _roleManager;

        public UserRoleService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<long>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IEnumerable<object>> GetRolesWithPermissionsAsync()
        {
            var roles = _roleManager.Roles.ToList();
            var list = new List<object>();
            foreach (var role in roles)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                list.Add(new { role = role.Name, permissions = claims.Where(c => c.Type == "permission").Select(c => c.Value) });
            }
            return list;
        }

        public async Task<bool> CreateRoleAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName)) return false;
            var exists = await _roleManager.FindByNameAsync(roleName);
            if (exists != null) return false;
            var role = new IdentityRole<long> { Name = roleName };
            var res = await _roleManager.CreateAsync(role);
            return res.Succeeded;
        }

        public async Task<ServiceResult<bool>> RenameRoleAsync(string currentName, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                return ServiceResult<bool>.Failure("Role name is required.");

            newName = newName.Trim();
            var role = await _roleManager.FindByNameAsync(currentName);
            if (role == null) return ServiceResult<bool>.Missing();

            var existingRole = await _roleManager.FindByNameAsync(newName);
            if (existingRole != null && existingRole.Id != role.Id)
                return ServiceResult<bool>.Failure("A role with this name already exists.");

            role.Name = newName;
            var result = await _roleManager.UpdateAsync(role);
            return result.Succeeded
                ? ServiceResult<bool>.Success(true)
                : ServiceResult<bool>.Failure(result.Errors.Select(error => error.Description).ToArray());
        }

        public async Task<ServiceResult<bool>> DeleteRoleAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) return ServiceResult<bool>.Missing();

            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            if (usersInRole.Count > 0)
                return ServiceResult<bool>.Failure("Remove this role from all users before deleting it.");

            var result = await _roleManager.DeleteAsync(role);
            return result.Succeeded
                ? ServiceResult<bool>.Success(true)
                : ServiceResult<bool>.Failure(result.Errors.Select(error => error.Description).ToArray());
        }

        public async Task<bool> AddPermissionToRoleAsync(string roleName, string permission)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) return false;
            var claims = await _roleManager.GetClaimsAsync(role);
            if (claims.Any(c => c.Type == "permission" && c.Value == permission)) return false;
            await _roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("permission", permission));
            return true;
        }

        public async Task<bool> RemovePermissionFromRoleAsync(string roleName, string permission)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) return false;
            var claims = await _roleManager.GetClaimsAsync(role);
            var claim = claims.FirstOrDefault(c => c.Type == "permission" && c.Value == permission);
            if (claim == null) return false;
            await _roleManager.RemoveClaimAsync(role, claim);
            return true;
        }

        public async Task<bool> AssignRoleToUserAsync(long userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) return false;
            var res = await _userManager.AddToRoleAsync(user, roleName);
            return res.Succeeded;
        }

        public async Task<bool> RemoveRoleFromUserAsync(long userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;
            var res = await _userManager.RemoveFromRoleAsync(user, roleName);
            return res.Succeeded;
        }

        public async Task<(string[] roles, string[] permissions)> GetUserEffectivePermissionsAsync(long userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return (Array.Empty<string>(), Array.Empty<string>());

            var userClaims = await _userManager.GetClaimsAsync(user);
            var userPermissions = userClaims.Where(c => string.Equals(c.Type, "permission", StringComparison.OrdinalIgnoreCase)).Select(c => c.Value);

            var roles = await _userManager.GetRolesAsync(user);
            var rolePermissions = new List<string>();
            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null) continue;
                var claims = await _roleManager.GetClaimsAsync(role);
                rolePermissions.AddRange(claims.Where(c => string.Equals(c.Type, "permission", StringComparison.OrdinalIgnoreCase)).Select(c => c.Value));
            }

            var effective = userPermissions.Concat(rolePermissions).Distinct().ToArray();
            return (roles.ToArray(), effective);
        }

        public async Task<PagedResult<UserPermissionsDto>> GetUsersPagedAsync(int page, int pageSize, string? search)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var query = _userManager.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(u => u.UserName.Contains(s) || (u.DisplayName != null && u.DisplayName.Contains(s)) || (u.Email != null && u.Email.Contains(s)));
            }

            var total = await query.LongCountAsync();
            var users = await query.OrderBy(u => u.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var items = new List<UserPermissionsDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userClaims = await _userManager.GetClaimsAsync(user);
                var userPerms = userClaims.Where(c => string.Equals(c.Type, "permission", StringComparison.OrdinalIgnoreCase)).Select(c => c.Value).ToList();

                var rolePerms = new List<string>();
                foreach (var rn in roles)
                {
                    var role = await _roleManager.FindByNameAsync(rn);
                    if (role == null) continue;
                    var claims = await _roleManager.GetClaimsAsync(role);
                    rolePerms.AddRange(claims.Where(c => string.Equals(c.Type, "permission", StringComparison.OrdinalIgnoreCase)).Select(c => c.Value));
                }

                var perms = userPerms.Concat(rolePerms).Distinct().ToArray();
                items.Add(new UserPermissionsDto
                {
                    UserId = user.Id.ToString(),
                    UserName = user.UserName ?? string.Empty,
                    Roles = roles.ToList(),
                    Permissions = perms.ToList()
                });
            }

            return new PagedResult<UserPermissionsDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)System.Math.Ceiling(total / (double)pageSize)
            };
        }

        public async Task<IEnumerable<UserPermissionsDto>> GetPermissionsMatrixAsync()
        {
            var users = await _userManager.Users.OrderBy(u => u.Id).ToListAsync();
            var list = new List<UserPermissionsDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userClaims = await _userManager.GetClaimsAsync(user);
                var userPerms = userClaims.Where(c => string.Equals(c.Type, "permission", StringComparison.OrdinalIgnoreCase)).Select(c => c.Value).ToList();

                var rolePerms = new List<string>();
                foreach (var rn in roles)
                {
                    var role = await _roleManager.FindByNameAsync(rn);
                    if (role == null) continue;
                    var claims = await _roleManager.GetClaimsAsync(role);
                    rolePerms.AddRange(claims.Where(c => string.Equals(c.Type, "permission", StringComparison.OrdinalIgnoreCase)).Select(c => c.Value));
                }

                var perms = userPerms.Concat(rolePerms).Distinct().ToArray();
                list.Add(new UserPermissionsDto
                {
                    UserId = user.Id.ToString(),
                    UserName = user.UserName ?? string.Empty,
                    Roles = roles.ToList(),
                    Permissions = perms.ToList()
                });
            }

            return list;
        }
    }
}
