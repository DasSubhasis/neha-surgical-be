using System.ComponentModel.DataAnnotations;

namespace NehaSurgicalAPI.DTOs;

public class MaterialDeliveryDto
{
    public int DeliveryId { get; set; }
    public int OrderId { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string HospitalName { get; set; } = string.Empty;
    public string OperationDate { get; set; } = string.Empty;
    public string OperationTime { get; set; } = string.Empty;
    public string DeliveryDate { get; set; } = string.Empty;
    public int? DeliveredById { get; set; }
    public string DeliveredBy { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string DeliveryStatus { get; set; } = "Pending";
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}

public class CreateMaterialDeliveryDto
{
    [Required(ErrorMessage = "Order ID is required")]
    public int OrderId { get; set; }

    [Required(ErrorMessage = "Delivery date is required")]
    public string DeliveryDate { get; set; } = string.Empty;

    [Required(ErrorMessage = "Delivered by user ID is required")]
    public int DeliveredById { get; set; }

    public string? Remarks { get; set; }

    [Required(ErrorMessage = "Delivery status is required")]
    [RegularExpression("^(Pending|Assigned|Delivered)$", 
        ErrorMessage = "Delivery status must be one of: Pending, Assigned, Delivered")]
    public string DeliveryStatus { get; set; } = "Pending";

    [Required(ErrorMessage = "Created by is required")]
    [StringLength(100, ErrorMessage = "Created by cannot exceed 100 characters")]
    public string CreatedBy { get; set; } = string.Empty;
}

public class UpdateMaterialDeliveryDto
{
    public int? OrderId { get; set; }

    public string? DeliveryDate { get; set; }

    public int? DeliveredById { get; set; }

    public string? Remarks { get; set; }

    [StringLength(100, ErrorMessage = "Updated by cannot exceed 100 characters")]
    public string? UpdatedBy { get; set; }

    [RegularExpression("^(Pending|Assigned|Delivered)$", 
        ErrorMessage = "Delivery status must be one of: Pending, Assigned, Delivered")]
    public string? DeliveryStatus { get; set; }
}

public class MarkAsDeliveredDto
{
    [Required(ErrorMessage = "Delivered by user ID is required")]
    public int DeliveredById { get; set; }

    public string? Remarks { get; set; }
}
