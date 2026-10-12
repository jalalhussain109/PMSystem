using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.Interfaces;
using PMS.Domain.Entities;

namespace PMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEffectivePermissionService _permissionService;

    public PermissionsController(UserManager<ApplicationUser> userManager, IEffectivePermissionService permissionService)
    {
        _userManager = userManager;
        _permissionService = permissionService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyPermissions()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null || !user.IsActive)
            return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await _permissionService.GetEffectivePermissionsAsync(user.Id);

        return Ok(new
        {
            userId = user.Id,
            userName = user.UserName,
            roles,
            permissions
        });
    }
}