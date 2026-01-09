using System.ComponentModel.DataAnnotations.Schema;

namespace NehaSurgicalAPI.Models;

public class SubAssistantAssignment
{
    [Column("sub_assignment_id")]
    public int SubAssignmentId { get; set; }

    [Column("assignment_id")]
    public int AssignmentId { get; set; }

    [Column("sub_assistant_id")]
    public int SubAssistantId { get; set; }

    [Column("remarks")]
    public string? Remarks { get; set; }

    [Column("assigned_at")]
    public DateTime AssignedAt { get; set; }

    [Column("assigned_by")]
    public int? AssignedBy { get; set; }

    [Column("is_active")]
    public char IsActive { get; set; } = 'Y';

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
