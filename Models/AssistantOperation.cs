using System.ComponentModel.DataAnnotations.Schema;

namespace NehaSurgicalAPI.Models;

public class AssistantOperation
{
    [Column("operation_record_id")]
    public int OperationRecordId { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("assistant_id")]
    public int AssistantId { get; set; }

    [Column("gps_latitude")]
    public decimal? GpsLatitude { get; set; }

    [Column("gps_longitude")]
    public decimal? GpsLongitude { get; set; }

    [Column("gps_location")]
    public string? GpsLocation { get; set; }

    [Column("checkin_time")]
    public DateTime? CheckinTime { get; set; }

    [Column("checkout_time")]
    public DateTime? CheckoutTime { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("is_active")]
    public char IsActive { get; set; } = 'Y';

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
