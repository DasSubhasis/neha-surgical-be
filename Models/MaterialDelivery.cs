using System.ComponentModel.DataAnnotations.Schema;

namespace NehaSurgicalAPI.Models;

public class MaterialDelivery
{
    [Column("delivery_id")]
    public int DeliveryId { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("delivery_date")]
    public DateOnly DeliveryDate { get; set; }

    [Column("delivered_by")]
    public string DeliveredBy { get; set; } = string.Empty;

    [Column("remarks")]
    public string? Remarks { get; set; }

    [Column("delivery_status")]
    public string DeliveryStatus { get; set; } = "Pending";

    [Column("created_by")]
    public string CreatedBy { get; set; } = string.Empty;

    [Column("updated_by")]
    public string? UpdatedBy { get; set; }

    [Column("is_active")]
    public string IsActive { get; set; } = "Y";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
