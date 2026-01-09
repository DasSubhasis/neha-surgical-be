using System.ComponentModel.DataAnnotations;

namespace NehaSurgicalAPI.DTOs;

public class ItemGroupDto
{
    public int ItemGroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    [RegularExpression("^[YN]$", ErrorMessage = "IsActive must be 'Y' or 'N'")]
    public string IsActive { get; set; } = "Y";
}

public class CreateItemGroupDto
{
    [Required(ErrorMessage = "Item group name is required")]
    [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters")]
    public string? Description { get; set; }

    [RegularExpression("^[YN]$", ErrorMessage = "IsActive must be 'Y' or 'N'")]
    public string IsActive { get; set; } = "Y";
}

public class UpdateItemGroupDto
{
    [Required(ErrorMessage = "Item group name is required")]
    [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters")]
    public string? Description { get; set; }

    [RegularExpression("^[YN]$", ErrorMessage = "IsActive must be 'Y' or 'N'")]
    public string IsActive { get; set; } = "Y";
}
