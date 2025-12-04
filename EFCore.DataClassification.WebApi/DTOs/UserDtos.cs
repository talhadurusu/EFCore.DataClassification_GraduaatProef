using System.ComponentModel.DataAnnotations;

namespace EFCore.DataClassification.WebApi.DTOs;

/// <summary>
/// DTO for creating a new user.
/// </summary>
public class CreateUserDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Surname { get; set; } = null!;

    [MaxLength(500)]
    public string? Address { get; set; }

    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Range(0, int.MaxValue)]
    public int? Salary { get; set; }

    public int? AdminId { get; set; }
}

/// <summary>
/// DTO for updating an existing user.
/// </summary>
public class UpdateUserDto
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(100)]
    public string? Surname { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Range(0, int.MaxValue)]
    public int? Salary { get; set; }

    public int? AdminId { get; set; }
}

/// <summary>
/// DTO for user response with classification info.
/// </summary>
public class UserResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public int Salary { get; set; }
    public int? AdminId { get; set; }
}


