using System.ComponentModel.DataAnnotations.Schema;

namespace NehaSurgicalAPI.Models;

public class ItemGroup
{
    [Column("item_group_id")]
    public int ItemGroupId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("is_active")]
    public string IsActive { get; set; } = "Y";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
