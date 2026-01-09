using System.ComponentModel.DataAnnotations.Schema;

namespace NehaSurgicalAPI.Models;

public class Permission
{
    [Column("permission_id")]
    public int PermissionId { get; set; }

    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("module")]
    public string Module { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("is_active")]
    public string IsActive { get; set; } = "Y";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}

public class RolePermission
{
    [Column("role_permission_id")]
    public int RolePermissionId { get; set; }

    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("permission_id")]
    public int PermissionId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
