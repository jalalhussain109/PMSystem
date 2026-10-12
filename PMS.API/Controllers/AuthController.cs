using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs;
using PMS.Application.Interfaces;
using PMS.Domain.Interfaces;
using PMS.Domain.Entities;
using System.Security.Claims;

namespace PMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly IIdentityUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<long>> _roleManager;

    public AuthController(AuthService authService, IIdentityUserRepository userRepository, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<long>> roleManager)
    {
        _authService = authService;
        _userRepository = userRepository;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [Authorize]
    [HttpGet("permissions")]
    public IActionResult GetPermissions()
    {
        // collect permission claims from token
        var perms = User.Claims
            .Where(c => string.Equals(c.Type, "permission", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Value)
            .Distinct()
            .ToArray();

        // collect roles from token (try both standard ClaimTypes.Role and custom 'role')
        var roles = User.Claims
            .Where(c => string.Equals(c.Type, ClaimTypes.Role, StringComparison.OrdinalIgnoreCase) || string.Equals(c.Type, "role", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Value)
            .Distinct()
            .ToArray();

        return Ok(new { roles, permissions = perms });
    }

    [Authorize]
    [HttpGet("permissions/effective")]
    public async Task<IActionResult> GetEffectivePermissions()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        if (!long.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return Unauthorized();

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

        return Ok(new { roles, permissions = effective });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var token = await _authService.LoginAsync(dto);

        if (token is null)
        {
            return Unauthorized(new
            {
                message = "Invalid username or password."
            });
        }

        return Ok(new { token });
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        if (!long.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        return Ok(new UserDto(
            user.Id,
            user.UserName,
            user.DisplayName,
            user.Email,
            user.PhoneNumber,
            user.IsActive,
            user.CreatedDate,
            user.CreatedBy,
            user.ModifiedDate,
            user.ModifiedBy));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        if (!long.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        var result = await _userRepository.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(error => error.Description) });

        return Ok(new { message = "Password changed successfully." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        // try find by user name first then by email
        var user = await _userRepository.GetByUserNameAsync(dto.UserNameOrEmail);
        if (user == null)
        {
            // try email
            // repository doesn't have GetByEmail, fallback: search all
            var all = await _userRepository.GetAllAsync();
            user = all.FirstOrDefault(u => string.Equals(u.Email, dto.UserNameOrEmail, StringComparison.OrdinalIgnoreCase));
        }

        if (user == null) return NotFound(new { message = "User not found" });

        var token = await _userRepository.GeneratePasswordResetTokenAsync(user);
        // In production send token via email. Here return token for testing.
        return Ok(new { token });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var user = await _userRepository.GetByIdAsync(dto.UserId);
        if (user == null) return NotFound(new { message = "User not found" });

        var result = await _userRepository.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded) return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return Ok(new { message = "Password reset successful" });
    }
}
