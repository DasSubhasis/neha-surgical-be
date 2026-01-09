using System.ComponentModel.DataAnnotations;

namespace NehaSurgicalAPI.DTOs;

public class ItemDto
{
    public int ItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Shortname { get; set; } = string.Empty;
    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int? ItemGroupId { get; set; }
    public string? ItemGroupName { get; set; }
    public int? SpecificationId { get; set; }
    public string? SpecificationName { get; set; }
    public int? SizeId { get; set; }
    public string? SizeName { get; set; }
    public string? Material { get; set; }
    public string? Model { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = "Active";
    public string IsActive { get; set; } = "Y";
    public DateTime CreatedAt { get; set; }
}

public class CreateItemDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Shortname { get; set; } = string.Empty;

    [Required]
    public int BrandId { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public int? ItemGroupId { get; set; }

    public int? SpecificationId { get; set; }

    public int? SizeId { get; set; }

    [StringLength(100)]
    public string? Material { get; set; }

    [StringLength(100)]
    public string? Model { get; set; }

    [StringLength(250)]
    public string? Description { get; set; }

    [Required]
    public decimal Price { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Active";

    [RegularExpression("^[YN]$", ErrorMessage = "IsActive must be 'Y' or 'N'")]
    public string IsActive { get; set; } = "Y";
}

public class UpdateItemDto : CreateItemDto {}
