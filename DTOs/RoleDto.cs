using System.ComponentModel.DataAnnotations;

namespace NehaSurgicalAPI.DTOs;

// Role DTOs
public class RoleDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string IsActive { get; set; } = "Y";
}

public class CreateRoleDto
{
    [Required(ErrorMessage = "Role name is required")]
    [StringLength(50, ErrorMessage = "Role name cannot exceed 50 characters")]
    public string RoleName { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters")]
    public string? Description { get; set; }

    [RegularExpression("^[YN]$", ErrorMessage = "IsActive must be 'Y' or 'N'")]
    public string IsActive { get; set; } = "Y";
}

public class UpdateRoleDto
{
    [Required(ErrorMessage = "Role name is required")]
    [StringLength(50, ErrorMessage = "Role name cannot exceed 50 characters")]
    public string RoleName { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters")]
    public string? Description { get; set; }

    [RegularExpression("^[YN]$", ErrorMessage = "IsActive must be 'Y' or 'N'")]
    public string IsActive { get; set; } = "Y";
}

// Menu DTOs
public class MenuDto
{
    public int MenuId { get; set; }
    public string MenuName { get; set; } = string.Empty;
    public string MenuPath { get; set; } = string.Empty;
    public string? MenuIcon { get; set; }
    public int? ParentMenuId { get; set; }
    public int SortOrder { get; set; }
    public string IsActive { get; set; } = "Y";
    public List<MenuDto>? SubMenus { get; set; }
}

public class CreateMenuDto
{
    [Required(ErrorMessage = "Menu name is required")]
    [StringLength(100, ErrorMessage = "Menu name cannot exceed 100 characters")]
    public string MenuName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Menu path is required")]
    [StringLength(200, ErrorMessage = "Menu path cannot exceed 200 characters")]
    public string MenuPath { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Menu icon cannot exceed 50 characters")]
    public string? MenuIcon { get; set; }

    public int? ParentMenuId { get; set; }

    public int SortOrder { get; set; } = 0;

    [RegularExpression("^[YN]$", ErrorMessage = "IsActive must be 'Y' or 'N'")]
    public string IsActive { get; set; } = "Y";
}

public class UpdateMenuDto
{
    [Required(ErrorMessage = "Menu name is required")]
    [StringLength(100, ErrorMessage = "Menu name cannot exceed 100 characters")]
    public string MenuName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Menu path is required")]
    [StringLength(200, ErrorMessage = "Menu path cannot exceed 200 characters")]
    public string MenuPath { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Menu icon cannot exceed 50 characters")]
    public string? MenuIcon { get; set; }

    public int? ParentMenuId { get; set; }

    public int SortOrder { get; set; } = 0;

    [RegularExpression("^[YN]$", ErrorMessage = "IsActive must be 'Y' or 'N'")]
    public string IsActive { get; set; } = "Y";
}

// Permission DTOs
public class RoleMenuPermissionDto
{
    public int PermissionId { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int MenuId { get; set; }
    public string MenuName { get; set; } = string.Empty;
    public string MenuPath { get; set; } = string.Empty;
    public string CanView { get; set; } = "Y";
    public string CanCreate { get; set; } = "N";
    public string CanEdit { get; set; } = "N";
    public string CanDelete { get; set; } = "N";
}

public class AssignRolePermissionsDto
{
    [Required(ErrorMessage = "Role ID is required")]
    public int RoleId { get; set; }

    [Required(ErrorMessage = "At least one permission is required")]
    public List<MenuPermissionDto> Permissions { get; set; } = new();
}

public class MenuPermissionDto
{
    [Required(ErrorMessage = "Menu ID is required")]
    public int MenuId { get; set; }

    [RegularExpression("^[YN]$", ErrorMessage = "CanView must be 'Y' or 'N'")]
    public string CanView { get; set; } = "Y";

    [RegularExpression("^[YN]$", ErrorMessage = "CanCreate must be 'Y' or 'N'")]
    public string CanCreate { get; set; } = "N";

    [RegularExpression("^[YN]$", ErrorMessage = "CanEdit must be 'Y' or 'N'")]
    public string CanEdit { get; set; } = "N";

    [RegularExpression("^[YN]$", ErrorMessage = "CanDelete must be 'Y' or 'N'")]
    public string CanDelete { get; set; } = "N";
}

// User menu response (what frontend needs)
public class UserMenuResponseDto
{
    public List<MenuWithPermissionsDto> Menus { get; set; } = new();
}

public class MenuWithPermissionsDto
{
    public int MenuId { get; set; }
    public string MenuName { get; set; } = string.Empty;
    public string MenuPath { get; set; } = string.Empty;
    public string? MenuIcon { get; set; }
    public int? ParentMenuId { get; set; }
    public int SortOrder { get; set; }
    public string CanView { get; set; } = "Y";
    public string CanCreate { get; set; } = "N";
    public string CanEdit { get; set; } = "N";
    public string CanDelete { get; set; } = "N";
    public List<MenuWithPermissionsDto>? SubMenus { get; set; }
}
