using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs;
using PMS.Application.Interfaces;
using PMS.Domain.Interfaces;

namespace PMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly IIdentityUserRepository _userRepository;

    public AuthController(AuthService authService, IIdentityUserRepository userRepository)
    {
        _authService = authService;
        _userRepository = userRepository;
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
