using System.ComponentModel.DataAnnotations.Schema;

namespace NehaSurgicalAPI.Models;

public class OrderAudit
{
    [Column("order_audit_id")]
    public int OrderAuditId { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("action")]
    public string Action { get; set; } = string.Empty;

    [Column("performed_by")]
    public string PerformedBy { get; set; } = string.Empty;

    [Column("performed_at")]
    public DateTime PerformedAt { get; set; }

    [Column("remarks")]
    public string? Remarks { get; set; }
}
