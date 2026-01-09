using System.ComponentModel.DataAnnotations.Schema;

namespace NehaSurgicalAPI.Models;

public class Role
{
    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("role_name")]
    public string RoleName { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("is_active")]
    public string IsActive { get; set; } = "Y";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}

public class Menu
{
    [Column("menu_id")]
    public int MenuId { get; set; }

    [Column("menu_name")]
    public string MenuName { get; set; } = string.Empty;

    [Column("menu_path")]
    public string MenuPath { get; set; } = string.Empty;

    [Column("menu_icon")]
    public string? MenuIcon { get; set; }

    [Column("parent_menu_id")]
    public int? ParentMenuId { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("is_active")]
    public string IsActive { get; set; } = "Y";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}

public class RoleMenuPermission
{
    [Column("permission_id")]
    public int PermissionId { get; set; }

    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("menu_id")]
    public int MenuId { get; set; }

    [Column("can_view")]
    public string CanView { get; set; } = "Y";

    [Column("can_create")]
    public string CanCreate { get; set; } = "N";

    [Column("can_edit")]
    public string CanEdit { get; set; } = "N";

    [Column("can_delete")]
    public string CanDelete { get; set; } = "N";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
