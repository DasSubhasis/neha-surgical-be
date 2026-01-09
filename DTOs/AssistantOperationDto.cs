namespace NehaSurgicalAPI.DTOs;

public class AssistantOperationDto
{
    public int OperationRecordId { get; set; }
    public int OrderId { get; set; }
    public string? OrderNo { get; set; }
    public int AssistantId { get; set; }
    public string? AssistantName { get; set; }
    public decimal? GpsLatitude { get; set; }
    public decimal? GpsLongitude { get; set; }
    public string? GpsLocation { get; set; }
    public string? CheckinTime { get; set; }
    public string? CheckoutTime { get; set; }
    public string? Notes { get; set; }
    public string? CreatedAt { get; set; }
    public string? UpdatedAt { get; set; }
}

public class CreateAssistantOperationDto
{
    public int OrderId { get; set; }
    public int AssistantId { get; set; }
    public decimal? GpsLatitude { get; set; }
    public decimal? GpsLongitude { get; set; }
    public string? GpsLocation { get; set; }
    public DateTime? CheckinTime { get; set; }
    public DateTime? CheckoutTime { get; set; }
    public string? Notes { get; set; }
}

public class UpdateAssistantOperationDto
{
    public decimal? GpsLatitude { get; set; }
    public decimal? GpsLongitude { get; set; }
    public string? GpsLocation { get; set; }
    public DateTime? CheckinTime { get; set; }
    public DateTime? CheckoutTime { get; set; }
    public string? Notes { get; set; }
}
