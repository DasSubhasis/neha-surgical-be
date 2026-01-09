using System.ComponentModel.DataAnnotations.Schema;

namespace NehaSurgicalAPI.Models;

public class User
{
    [Column("system_user_id")]
    public int UserId { get; set; }

    [Column("username")]
    public string? Username { get; set; }

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("full_name")]
    public string FullName { get; set; } = string.Empty;

    [Column("phone_no")]
    public string? Phone { get; set; }

    [Column("employee_id")]
    public string? EmployeeId { get; set; }

    [Column("identifier")]
    public string? Identifier { get; set; }

    [Column("role_id")]
    public int? RoleId { get; set; }

    [Column("role_name")]
    public string? RoleName { get; set; }

    [Column("is_active")]
    public string IsActive { get; set; } = "Y";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("last_login")]
    public DateTime? LastLogin { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
