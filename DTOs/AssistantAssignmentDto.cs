using System.ComponentModel.DataAnnotations;

namespace NehaSurgicalAPI.DTOs;

// Assistant DTO
public class AssistantDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// Assignment DTO (order with assignment details)
public class AssistantAssignmentDto
{
    public int Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public string Patient { get; set; } = string.Empty;
    public string OperationDate { get; set; } = string.Empty;
    public string OperationTime { get; set; } = string.Empty;
    public int? AssistantId { get; set; }
    public string? AssistantName { get; set; }
    public string? ReportingDate { get; set; }
    public string? ReportingTime { get; set; }
    public string? Remarks { get; set; }
    public string Status { get; set; } = "Pending";
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int HospitalId { get; set; }
    public string HospitalName { get; set; } = string.Empty;
}

// Create/Update assignment request
public class AssignAssistantDto
{
    [Required(ErrorMessage = "Order ID is required")]
    public int OrderId { get; set; }

    [Required(ErrorMessage = "Assistant ID is required")]
    public int AssistantId { get; set; }

    [Required(ErrorMessage = "Reporting date is required")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Reporting date must be in yyyy-MM-dd format")]
    public string ReportingDate { get; set; } = string.Empty;

    [Required(ErrorMessage = "Reporting time is required")]
    [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Reporting time must be in HH:mm format")]
    public string ReportingTime { get; set; } = string.Empty;

    public string? Remarks { get; set; }

    public int? AssignedBy { get; set; }
}

// Existing assignment DTO (for checking assistant's schedule)
public class ExistingAssignmentDto
{
    public int Id { get; set; }
    public int AssistantId { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string OperationTime { get; set; } = string.Empty;
    public string ReportingDate { get; set; } = string.Empty;
    public string ReportingTime { get; set; } = string.Empty;
}
