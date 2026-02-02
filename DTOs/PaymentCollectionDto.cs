using System.ComponentModel.DataAnnotations;

namespace NehaSurgicalAPI.DTOs;

public class PaymentCollectionDto
{
    public int CollectionId { get; set; }
    public string CollectionDate { get; set; } = string.Empty;
    public string CollectedBy { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int HospitalId { get; set; }
    public string HospitalName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Remarks { get; set; }
    public string? CreatedBy { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}

public class CreatePaymentCollectionDto
{
    [Required]
    public string CollectionDate { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string CollectedBy { get; set; } = string.Empty;

    [Required]
    public int DoctorId { get; set; }

    [Required]
    public int HospitalId { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Amount must be greater than or equal to 0")]
    public decimal Amount { get; set; }

    [StringLength(1000)]
    public string? Remarks { get; set; }

    [StringLength(100)]
    public string? CreatedBy { get; set; }
}

public class UpdatePaymentCollectionDto
{
    public string? CollectionDate { get; set; }

    [StringLength(100)]
    public string? CollectedBy { get; set; }

    public int? DoctorId { get; set; }

    public int? HospitalId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Amount must be greater than or equal to 0")]
    public decimal? Amount { get; set; }

    [StringLength(1000)]
    public string? Remarks { get; set; }
}
