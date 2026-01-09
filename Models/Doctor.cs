using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NehaSurgicalAPI.Models;

[Table("doctors")]
public class Doctor
{
    [Key]
    [Column("doctor_id")]
    public int DoctorId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("doctor_name")]
    public string DoctorName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Column("contact_no")]
    public string ContactNo { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("email")]
    public string? Email { get; set; }

    [MaxLength(100)]
    [Column("specialization")]
    public string? Specialization { get; set; }

    [Column("dob")]
    public DateTime? Dob { get; set; }

    [Column("doa")]
    public DateTime? Doa { get; set; }

    [MaxLength(100)]
    [Column("identifier")]
    public string? Identifier { get; set; }

    [MaxLength(100)]
    [Column("registration_number")]
    public string? RegistrationNumber { get; set; }

    [MaxLength(200)]
    [Column("location")]
    public string? Location { get; set; }

    [MaxLength(250)]
    [Column("remarks")]
    public string? Remarks { get; set; }

    [Required]
    [MaxLength(1)]
    [Column("is_active")]
    public string IsActive { get; set; } = "Y";
}
