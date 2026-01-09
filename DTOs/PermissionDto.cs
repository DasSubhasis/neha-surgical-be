using System.ComponentModel.DataAnnotations;

namespace NehaSurgicalAPI.DTOs;

// Permission DTOs
public class PermissionDto
{
    public int PermissionId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class PermissionsByModuleDto
{
    public string Module { get; set; } = string.Empty;
    public List<PermissionDto> Permissions { get; set; } = new();
}

// Role with Permissions DTOs
public class RoleWithPermissionsDto
{
    public int RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<int> Permissions { get; set; } = new();
    public List<PermissionDto>? PermissionDetails { get; set; }
    public string IsActive { get; set; } = "Y";
    public DateTime CreatedAt { get; set; }
}

public class CreateRoleWithPermissionsDto
{
    [Required(ErrorMessage = "Role name is required")]
    [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters")]
    public string? Description { get; set; }

    public List<int> Permissions { get; set; } = new();

    [RegularExpression("^[YN]$", ErrorMessage = "IsActive must be 'Y' or 'N'")]
    public string IsActive { get; set; } = "Y";
}

public class UpdateRoleWithPermissionsDto
{
    [Required(ErrorMessage = "Role name is required")]
    [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters")]
    public string? Description { get; set; }

    public List<int> Permissions { get; set; } = new();

    [RegularExpression("^[YN]$", ErrorMessage = "IsActive must be 'Y' or 'N'")]
    public string IsActive { get; set; } = "Y";
}
