
using System.ComponentModel.DataAnnotations;

namespace PMS.Application.DTOs;

public record CreateUserDto
{
    [Required]
    public string UserName { get; init; } = string.Empty;


    public string? DisplayName { get; init; }

    [EmailAddress]
    public string? Email { get; init; }

    [Phone]
    public string? PhoneNumber { get; init; }

    [Required]
    public string Password { get; init; } = string.Empty;
}

public record UpdateUserDto
{
    [Required]
    public string UserName { get; init; } = string.Empty;

    public string? DisplayName { get; init; }

    [EmailAddress]
    public string? Email { get; init; }

    [Phone]
    public string? PhoneNumber { get; init; }

    public bool IsActive { get; init; }
}

public record UserDto(
    long Id,
    string? UserName,
    string? DisplayName,
    string? Email,
    string? PhoneNumber,
    bool IsActive,
    DateTime CreatedDate,
    long? CreatedBy,
    DateTime? ModifiedDate,
    long? ModifiedBy
);