using System.ComponentModel.DataAnnotations.Schema;

namespace NehaSurgicalAPI.Models;

public class Order
{
    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("order_no")]
    public string OrderNo { get; set; } = string.Empty;

    [Column("order_date")]
    public DateOnly OrderDate { get; set; }

    [Column("doctor_id")]
    public int DoctorId { get; set; }

    [Column("hospital_id")]
    public int HospitalId { get; set; }

    [Column("operation_date")]
    public DateOnly OperationDate { get; set; }

    [Column("operation_time")]
    public TimeOnly OperationTime { get; set; }

    [Column("material_send_date")]
    public DateOnly MaterialSendDate { get; set; }

    [Column("remarks")]
    public string? Remarks { get; set; }

    [Column("created_by")]
    public string CreatedBy { get; set; } = string.Empty;

    [Column("status")]
    public string Status { get; set; } = "Pending";

    [Column("is_delivered")]
    public string IsDelivered { get; set; } = "Pending";

    [Column("is_active")]
    public string IsActive { get; set; } = "Y";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
