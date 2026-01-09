using System.ComponentModel.DataAnnotations;

namespace NehaSurgicalAPI.DTOs;

using System.Collections.Generic;

public class HospitalContactDto
{
    public int? ContactId { get; set; }
    public string? Name { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Location { get; set; }
    public string? Remarks { get; set; }
}

public class HospitalDto
{
    public int HospitalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactNo { get; set; }
    public string? Email { get; set; }
    [RegularExpression("^[YN]$", ErrorMessage = "IsActive must be 'Y' or 'N'")]
    public string IsActive { get; set; } = "Y";
    public List<HospitalContactDto> Contacts { get; set; } = new();
}

public class CreateHospitalDto
{
    [Required(ErrorMessage = "Hospital name is required")]
    [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters")]
    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }

    [StringLength(100, ErrorMessage = "Contact person name cannot exceed 100 characters")]
    public string? ContactPerson { get; set; }

    [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
    public string? ContactNo { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string? Email { get; set; }

    [RegularExpression("^[YN]$", ErrorMessage = "IsActive must be 'Y' or 'N'")]
    public string IsActive { get; set; } = "Y";

    public List<HospitalContactDto>? Contacts { get; set; }
}

public class UpdateHospitalDto
{
    [Required(ErrorMessage = "Hospital name is required")]
    [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters")]
    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }

    [StringLength(100, ErrorMessage = "Contact person name cannot exceed 100 characters")]
    public string? ContactPerson { get; set; }

    [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
    public string? ContactNo { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string? Email { get; set; }

    [RegularExpression("^[YN]$", ErrorMessage = "IsActive must be 'Y' or 'N'")]
    public string IsActive { get; set; } = "Y";

    public List<HospitalContactDto>? Contacts { get; set; }
}
