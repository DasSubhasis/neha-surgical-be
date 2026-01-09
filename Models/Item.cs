using System.ComponentModel.DataAnnotations.Schema;

namespace NehaSurgicalAPI.Models;

public class Item
{
    [Column("item_id")]
    public int ItemId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("shortname")]
    public string Shortname { get; set; } = string.Empty;

    [Column("brand_id")]
    public int BrandId { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("specification_id")]
    public int? SpecificationId { get; set; }

    [Column("size_id")]
    public int? SizeId { get; set; }

    [Column("material")]
    public string? Material { get; set; }

    [Column("model")]
    public string? Model { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("price")]
    public decimal Price { get; set; }

    [Column("status")]
    public string Status { get; set; } = "Active";

    [Column("is_active")]
    public string IsActive { get; set; } = "Y";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
