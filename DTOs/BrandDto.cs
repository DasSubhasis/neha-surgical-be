using System.ComponentModel.DataAnnotations;

namespace NehaSurgicalAPI.DTOs;

public class BrandDto
{
    public int BrandId { get; set; }
    public string Name { get; set; } = string.Empty;
    
    [RegularExpression("^[YN]$", ErrorMessage = "IsActive must be 'Y' or 'N'")]
    public string IsActive { get; set; } = "Y";
    public DateTime CreatedAt { get; set; }
}

public class CreateBrandDto
{
    [Required(ErrorMessage = "Brand name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [RegularExpression("^[YN]$", ErrorMessage = "IsActive must be 'Y' or 'N'")]
    public string IsActive { get; set; } = "Y";
}

public class UpdateBrandDto
{
    [Required(ErrorMessage = "Brand name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [RegularExpression("^[YN]$", ErrorMessage = "IsActive must be 'Y' or 'N'")]
    public string IsActive { get; set; } = "Y";
}
