using Microsoft.AspNetCore.Identity;
using PMS.Application.Interfaces;
using PMS.Domain.Constants;
using PMS.Domain.Entities;
using System.Security.Claims;

namespace PMS.Infrastructure.Identity;

public class IdentityPermissionService : IEffectivePermissionService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<long>> _roleManager;

    public IdentityPermissionService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<long>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IReadOnlyCollection<string>> GetEffectivePermissionsAsync(long userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null || !user.IsActive)
            throw new KeyNotFoundException("User not found or inactive.");

        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. Permissions assigned directly to the user
        var userClaims = await _userManager.GetClaimsAsync(user);

        AddPermissions(userClaims, permissions);

        // 2. Permissions inherited from the user's roles
        var roles = await _userManager.GetRolesAsync(user);

        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
                continue;

            var roleClaims = await _roleManager.GetClaimsAsync(role);

            AddPermissions(roleClaims, permissions);
        }

        return permissions.OrderBy(p => p).ToArray();
    }

    private static void AddPermissions(IEnumerable<Claim> claims, HashSet<string> permissions)
    {
        foreach (var claim in claims)
        {
            if (claim.Type == PermissionConstants.ClaimType &&
                !string.IsNullOrWhiteSpace(claim.Value))
            {
                permissions.Add(claim.Value);
            }
        }
    }
}