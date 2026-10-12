using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PMS.Application.DTOs;
using PMS.Application.Interfaces;
using PMS.Domain.Constants;
using PMS.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static PMS.Domain.Constants.PermissionConstants;

public class AuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<long>> _roleManager;
    private readonly IEffectivePermissionService _permissionService;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<long>> roleManager,
        IEffectivePermissionService permissionService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _permissionService = permissionService;
    }

    public async Task<string?> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.UserName);

        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            return null;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, user.UserName ?? string.Empty),
            new(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var roles = await _userManager.GetRolesAsync(user);

        foreach (var role in roles)
        {
            claims.Add(new Claim("role", role));

            //var identityRole = await _roleManager.FindByNameAsync(role);
            //if (identityRole is null)
            //    continue;

            //var roleClaims = await _roleManager.GetClaimsAsync(identityRole);

            //foreach (var claim in roleClaims.Where(c => c.Type == PermissionConstants.ClaimType))
            //{
            //    claims.Add(new Claim(PermissionConstants.ClaimType, claim.Value));
            //}
        }

        var permissions = await _permissionService.GetEffectivePermissionsAsync(user.Id);
        foreach (var permission in permissions)
        {
            claims.Add(new Claim(PermissionConstants.ClaimType, permission));
        }

        var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is missing.");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryMinutes"] ?? "30")),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


}