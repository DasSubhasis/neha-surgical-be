using System.ComponentModel.DataAnnotations.Schema;

namespace NehaSurgicalAPI.Models;

public class OrderItemGroup
{
    [Column("order_item_group_id")]
    public int OrderItemGroupId { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("item_group_id")]
    public int ItemGroupId { get; set; }

    [Column("item_group_name")]
    public string ItemGroupName { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
