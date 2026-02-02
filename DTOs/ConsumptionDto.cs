namespace NehaSurgicalAPI.DTOs;

public class ConsumptionDto
{
    public int ConsumptionId { get; set; }
    public int OrderId { get; set; }
    public string? OrderNo { get; set; }
    public int? ItemGroupId { get; set; }
    public string? ItemGroupName { get; set; }
    public List<ConsumedItemDto> ConsumedItems { get; set; } = new();
    public List<ConsumptionImageDto> Images { get; set; } = new();
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}

public class ConsumptionImageDto
{
    public int ImageId { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long? FileSize { get; set; }
    public string? ContentType { get; set; }
    public string? UploadedBy { get; set; }
    public string UploadedAt { get; set; } = string.Empty;
}

public class ConsumedItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Type { get; set; } = "Manual"; // Auto or Manual
}

public class CreateConsumptionDto
{
    public int OrderId { get; set; }
    public int? ItemGroupId { get; set; }
    public string? ItemGroupName { get; set; }
    public List<ConsumedItemDto> ConsumedItems { get; set; } = new();
    public List<string>? Images { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class UpdateConsumptionDto
{
    public int? ItemGroupId { get; set; }
    public string? ItemGroupName { get; set; }
    public List<ConsumedItemDto>? ConsumedItems { get; set; }
    public List<string>? Images { get; set; }
    public string? UpdatedBy { get; set; }
}
