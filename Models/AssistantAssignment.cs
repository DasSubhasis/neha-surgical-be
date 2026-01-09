using System.ComponentModel.DataAnnotations.Schema;

namespace NehaSurgicalAPI.Models;

public class AssistantAssignment
{
    [Column("assignment_id")]
    public int AssignmentId { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("assistant_id")]
    public int AssistantId { get; set; }

    [Column("reporting_date")]
    public DateOnly ReportingDate { get; set; }

    [Column("reporting_time")]
    public TimeOnly ReportingTime { get; set; }

    [Column("remarks")]
    public string? Remarks { get; set; }

    [Column("assigned_at")]
    public DateTime AssignedAt { get; set; }

    [Column("assigned_by")]
    public int? AssignedBy { get; set; }

    [Column("is_active")]
    public string IsActive { get; set; } = "Y";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
