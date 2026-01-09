using System.ComponentModel.DataAnnotations.Schema;

namespace NehaSurgicalAPI.Models;

public class SystemUser
{
    [Column("system_user_id")]
    public int SystemUserId { get; set; }

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("full_name")]
    public string FullName { get; set; } = string.Empty;

    [Column("role_id")]
    public int? RoleId { get; set; }

    [Column("phone_no")]
    public string? PhoneNo { get; set; }

    // Not mapped to database - populated from JOIN with Roles table
    [NotMapped]
    public string? RoleName { get; set; }

    [Column("is_active")]
    public string IsActive { get; set; } = "Y";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}

public class UserOtp
{
    [Column("otp_id")]
    public int OtpId { get; set; }

    [Column("system_user_id")]
    public int SystemUserId { get; set; }

    [Column("otp_code")]
    public string OtpCode { get; set; } = string.Empty;

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("is_used")]
    public string IsUsed { get; set; } = "N";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
