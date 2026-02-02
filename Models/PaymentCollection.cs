using System.ComponentModel.DataAnnotations.Schema;

namespace NehaSurgicalAPI.Models;

public class PaymentCollection
{
    [Column("collection_id")]
    public int CollectionId { get; set; }

    [Column("collection_date")]
    public DateOnly CollectionDate { get; set; }

    [Column("collected_by")]
    public string CollectedBy { get; set; } = string.Empty;

    [Column("doctor_id")]
    public int DoctorId { get; set; }

    [Column("hospital_id")]
    public int HospitalId { get; set; }

    [Column("amount")]
    public decimal Amount { get; set; }

    [Column("remarks")]
    public string? Remarks { get; set; }

    [Column("created_by")]
    public string? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("is_active")]
    public string IsActive { get; set; } = "Y";
}
