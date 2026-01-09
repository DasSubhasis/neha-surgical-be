using System.ComponentModel.DataAnnotations.Schema;

namespace NehaSurgicalAPI.Models;

public class OrderItem
{
    [Column("order_item_id")]
    public int OrderItemId { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("item_id")]
    public int? ItemId { get; set; }

    [Column("item_group_id")]
    public int? ItemGroupId { get; set; }

    [Column("item_name")]
    public string ItemName { get; set; } = string.Empty;

    [Column("is_manual")]
    public string IsManual { get; set; } = "N";

    [Column("is_group")]
    public string IsGroup { get; set; } = "N";

    [Column("quantity")]
    public int Quantity { get; set; } = 1;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
