using System.ComponentModel.DataAnnotations;

namespace NehaSurgicalAPI.DTOs;

public class SpecificationDto
{
    public int SpecificationId { get; set; }
    public string Name { get; set; } = string.Empty;
    
    [RegularExpression("^[YN]$", ErrorMessage = "IsActive must be 'Y' or 'N'")]
    public string IsActive { get; set; } = "Y";
    public DateTime CreatedAt { get; set; }
}

public class CreateSpecificationDto
{
    [Required(ErrorMessage = "Specification name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [RegularExpression("^[YN]$", ErrorMessage = "IsActive must be 'Y' or 'N'")]
    public string IsActive { get; set; } = "Y";
}

public class UpdateSpecificationDto
{
    [Required(ErrorMessage = "Specification name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [RegularExpression("^[YN]$", ErrorMessage = "IsActive must be 'Y' or 'N'")]
    public string IsActive { get; set; } = "Y";
}
