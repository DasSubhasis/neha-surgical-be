namespace NehaSurgicalAPI.DTOs;

public class SubAssistantAssignmentDto
{
    public int SubAssignmentId { get; set; }
    public int AssignmentId { get; set; }
    public int OrderId { get; set; }
    public string? OrderNo { get; set; }
    public int MainAssistantId { get; set; }
    public string? MainAssistantName { get; set; }
    public int SubAssistantId { get; set; }
    public string? SubAssistantName { get; set; }
    public string? SubAssistantPhone { get; set; }
    public string? Remarks { get; set; }
    public string? AssignedAt { get; set; }
}

public class AssignSubAssistantDto
{
    public int? AssignmentId { get; set; }
    public int? OrderId { get; set; }
    public int SubAssistantId { get; set; }
    public string? Remarks { get; set; }
    public int? AssignedBy { get; set; }
}
