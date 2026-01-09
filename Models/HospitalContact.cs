using System.ComponentModel.DataAnnotations.Schema;

namespace NehaSurgicalAPI.Models;

public class HospitalContact
{
    [Column("contact_id")]
    public int ContactId { get; set; }

    [Column("hospital_id")]
    public int HospitalId { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("mobile")]
    public string? Mobile { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("location")]
    public string? Location { get; set; }

    [Column("remarks")]
    public string? Remarks { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
