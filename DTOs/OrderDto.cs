using System.ComponentModel.DataAnnotations;

namespace NehaSurgicalAPI.DTOs;

public class OrderItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool? Manual { get; set; }
    public bool? IsGroup { get; set; }
    public int Quantity { get; set; } = 1;
}

public class OrderAuditDto
{
    public string When { get; set; } = string.Empty;
    public string By { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}

public class OrderDto
{
    public int OrderId { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public string OrderDate { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int HospitalId { get; set; }
    public string HospitalName { get; set; } = string.Empty;
    public string OperationDate { get; set; } = string.Empty;
    public string OperationTime { get; set; } = string.Empty;
    public string MaterialSendDate { get; set; } = string.Empty;
    public List<string> ItemGroups { get; set; } = new();
    public List<OrderItemDto> Items { get; set; } = new();
    public string? Remarks { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string IsDelivered { get; set; } = "Pending";
    public List<OrderAuditDto> Audits { get; set; } = new();
    public OrderMaterialDeliveryDto? MaterialDelivery { get; set; }
}

public class OrderMaterialDeliveryDto
{
    public string? DeliveryStatus { get; set; }
    public string? ActualDeliveryBy { get; set; }
    public int? ActualDeliveryByUserId { get; set; }
    public string? ActualDeliveryTime { get; set; }
    public string? Remarks { get; set; }
}

public class CreateOrderDto
{
    [Required(ErrorMessage = "Order date is required")]
    public string OrderDate { get; set; } = string.Empty;

    [Required(ErrorMessage = "Doctor ID is required")]
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "Hospital ID is required")]
    public int HospitalId { get; set; }

    [Required(ErrorMessage = "Operation date is required")]
    public string OperationDate { get; set; } = string.Empty;

    [Required(ErrorMessage = "Operation time is required")]
    public string OperationTime { get; set; } = string.Empty;

    [Required(ErrorMessage = "Material send date is required")]
    public string MaterialSendDate { get; set; } = string.Empty;

    public List<string> ItemGroups { get; set; } = new();
    
    public List<OrderItemDto> Items { get; set; } = new();

    public string? Remarks { get; set; }

    [Required(ErrorMessage = "Created by is required")]
    [StringLength(100, ErrorMessage = "Created by cannot exceed 100 characters")]
    public string CreatedBy { get; set; } = string.Empty;
}

public class UpdateOrderDto
{
    [StringLength(50, ErrorMessage = "Order number cannot exceed 50 characters")]
    public string? OrderNo { get; set; }

    public string? OrderDate { get; set; }

    public int? DoctorId { get; set; }

    public int? HospitalId { get; set; }

    public string? OperationDate { get; set; }

    public string? OperationTime { get; set; }

    public string? MaterialSendDate { get; set; }

    public List<string>? ItemGroups { get; set; }
    
    public List<OrderItemDto>? Items { get; set; }

    public string? Remarks { get; set; }

    [RegularExpression("^(Pending|Confirmed|Dispatched|Completed|Cancelled)$", 
        ErrorMessage = "Status must be one of: Pending, Confirmed, Dispatched, Completed, Cancelled")]
    public string? Status { get; set; }

    [StringLength(100, ErrorMessage = "Updated by cannot exceed 100 characters")]
    public string? UpdatedBy { get; set; }
}
