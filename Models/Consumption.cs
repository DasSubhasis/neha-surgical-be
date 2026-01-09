using System.ComponentModel.DataAnnotations.Schema;

namespace NehaSurgicalAPI.Models;

public class Consumption
{
    [Column("consumption_id")]
    public int ConsumptionId { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("item_group_id")]
    public int? ItemGroupId { get; set; }

    [Column("item_group_name")]
    public string? ItemGroupName { get; set; }

    [Column("consumed_items")]
    public string ConsumedItems { get; set; } = string.Empty; // JSON string

    [Column("created_by")]
    public string CreatedBy { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("is_active")]
    public char IsActive { get; set; } = 'Y';

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
