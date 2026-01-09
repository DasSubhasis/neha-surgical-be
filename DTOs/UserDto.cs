using System.ComponentModel.DataAnnotations;

namespace NehaSurgicalAPI.DTOs;

// User DTOs (matches Angular User interface)
public class UserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? EmployeeId { get; set; }
    public string? Identifier { get; set; }
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }
    public string IsActive { get; set; } = "Y";
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
}

public class CreateUserDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string? Phone { get; set; }

    [StringLength(50, ErrorMessage = "Employee ID cannot exceed 50 characters")]
    public string? EmployeeId { get; set; }

    [StringLength(100, ErrorMessage = "Identifier cannot exceed 100 characters")]
    public string? Identifier { get; set; }

    [Required(ErrorMessage = "Role ID is required")]
    public int RoleId { get; set; }

    [RegularExpression("^[YN]$", ErrorMessage = "IsActive must be 'Y' or 'N'")]
    public string IsActive { get; set; } = "Y";
}

public class UpdateUserDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string? Phone { get; set; }

    [StringLength(50, ErrorMessage = "Employee ID cannot exceed 50 characters")]
    public string? EmployeeId { get; set; }

    [StringLength(100, ErrorMessage = "Identifier cannot exceed 100 characters")]
    public string? Identifier { get; set; }

    [Required(ErrorMessage = "Role ID is required")]
    public int RoleId { get; set; }

    [RegularExpression("^[YN]$", ErrorMessage = "IsActive must be 'Y' or 'N'")]
    public string IsActive { get; set; } = "Y";
}
