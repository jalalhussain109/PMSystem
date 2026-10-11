namespace PMS.Application.DTOs;

public record LoginDto(string UserName, string Password);

public record ForgotPasswordDto(string UserNameOrEmail);

public record ResetPasswordDto(long UserId, string Token, string NewPassword);

public record AuthResultDto(long UserId, string UserName, string DisplayName, string? Email, bool IsActive , string? Token = null);

public record ChangePasswordDto(string CurrentPassword, string NewPassword);
