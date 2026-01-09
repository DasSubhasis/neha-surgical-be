using System.ComponentModel.DataAnnotations;

// Doctor DTOs for API requests and responses
namespace NehaSurgicalAPI.DTOs;

public class DoctorResponseDto
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string ContactNo { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Specialization { get; set; }
    public string? Dob { get; set; }
    public string? Doa { get; set; }
    public string? Identifier { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Location { get; set; }
    public string? Remarks { get; set; }
    public string IsActive { get; set; } = "Y";
}

public class CreateDoctorDto
{
    [Required(ErrorMessage = "Doctor name is required")]
    [MaxLength(100)]
    public string DoctorName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact number is required")]
    [MaxLength(20)]
    public string ContactNo { get; set; } = string.Empty;

    [MaxLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(100)]
    public string? Specialization { get; set; }

    public string? Dob { get; set; }

    public string? Doa { get; set; }

    [MaxLength(100)]
    public string? Identifier { get; set; }

    [MaxLength(100)]
    public string? RegistrationNumber { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(250)]
    public string? Remarks { get; set; }

    [Required]
    [RegularExpression("^[YN]$", ErrorMessage = "IsActive must be 'Y' or 'N'")]
    public string IsActive { get; set; } = "Y";
}

public class UpdateDoctorDto
{
    [Required(ErrorMessage = "Doctor name is required")]
    [MaxLength(100)]
    public string DoctorName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact number is required")]
    [MaxLength(20)]
    public string ContactNo { get; set; } = string.Empty;

    [MaxLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(100)]
    public string? Specialization { get; set; }

    public string? Dob { get; set; }

    public string? Doa { get; set; }

    [MaxLength(100)]
    public string? Identifier { get; set; }

    [MaxLength(100)]
    public string? RegistrationNumber { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(250)]
    public string? Remarks { get; set; }

    [Required]
    [RegularExpression("^[YN]$", ErrorMessage = "IsActive must be 'Y' or 'N'")]
    public string IsActive { get; set; } = "Y";
}
