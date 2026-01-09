using System.ComponentModel.DataAnnotations.Schema;

namespace NehaSurgicalAPI.Models;

public class Hospital
{
    [Column("hospital_id")]
    public int HospitalId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("address")]
    public string? Address { get; set; }

    [Column("contact_person")]
    public string? ContactPerson { get; set; }

    [Column("contact_no")]
    public string? ContactNo { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("is_active")]
    public string IsActive { get; set; } = "Y";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
