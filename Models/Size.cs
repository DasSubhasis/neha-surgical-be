using System.ComponentModel.DataAnnotations.Schema;

namespace NehaSurgicalAPI.Models;

public class Size
{
    [Column("size_id")]
    public int SizeId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("is_active")]
    public string IsActive { get; set; } = "Y";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
